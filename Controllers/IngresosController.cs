using EasyData.Api.Data;
using EasyData.Api.DTOs;
using EasyData.Api.Models;
using EasyData.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyData.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/ingresos")]
public class IngresosController(
    AppDbContext db,
    IngresoPdfService ingresoPdf) : ControllerBase
{
    // =========================================================
    // LISTAR Y BUSCAR INGRESOS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? buscar)
    {
        var query = db.IngresosEquipos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var patron =
                $"%{buscar.Trim()}%";

            query = query.Where(x =>

                EF.Functions.ILike(
                    x.NumeroIngreso,
                    patron
                )

                ||

                EF.Functions.ILike(
                    x.Cliente,
                    patron
                )

                ||

                EF.Functions.ILike(
                    x.Cedula,
                    patron
                )

                ||

                EF.Functions.ILike(
                    x.Telefono,
                    patron
                )

                ||

                EF.Functions.ILike(
                    x.TipoEquipo,
                    patron
                )

                ||

                EF.Functions.ILike(
                    x.Marca,
                    patron
                )

                ||

                EF.Functions.ILike(
                    x.Modelo,
                    patron
                )

                ||

                EF.Functions.ILike(
                    x.ImeiSerie,
                    patron
                )

                ||

                (
                    x.Correo != null &&
                    EF.Functions.ILike(
                        x.Correo,
                        patron
                    )
                )
            );
        }

        var resultado = await query
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(resultado);
    }


    // =========================================================
    // BUSCAR POR NÚMERO DE INGRESO
    // =========================================================

    [HttpGet("buscar/{numeroIngreso}")]
    public async Task<IActionResult> Buscar(
        string numeroIngreso)
    {
        var x = await db.IngresosEquipos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e =>
                    e.NumeroIngreso ==
                    numeroIngreso.Trim()
            );

        return x is null

            ? NotFound(new
            {
                message =
                    "No existe un equipo registrado con ese número de ingreso."
            })

            : Ok(x);
    }


    // =========================================================
    // DESCARGAR COMPROBANTE PDF
    // =========================================================

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(
        int id)
    {
        var ingreso = await db.IngresosEquipos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id
            );

        if (ingreso is null)
        {
            return NotFound(new
            {
                message =
                    "No se encontró el ingreso solicitado."
            });
        }

        var archivo =
            ingresoPdf.Generar(ingreso);

        return File(
            archivo,
            "application/pdf",
            $"{ingreso.NumeroIngreso}.pdf"
        );
    }


    // =========================================================
    // CREAR INGRESO + GUARDAR EVIDENCIA + GENERAR PDF
    // =========================================================

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Post(
        [FromForm] CrearIngresoDto d)
    {
        var numeroIngreso =
            d.NumeroIngreso.Trim();

        // -----------------------------------------
        // VALIDAR NÚMERO DUPLICADO
        // -----------------------------------------

        var existe =
            await db.IngresosEquipos
                .AnyAsync(
                    x =>
                        x.NumeroIngreso ==
                        numeroIngreso
                );

        if (existe)
        {
            return Conflict(new
            {
                message =
                    "Número de ingreso duplicado."
            });
        }


        // -----------------------------------------
        // VALIDAR CÉDULA
        // -----------------------------------------

        if (string.IsNullOrWhiteSpace(
            d.Cedula))
        {
            return BadRequest(new
            {
                message =
                    "Debe ingresar la cédula del cliente."
            });
        }


        // -----------------------------------------
        // EVIDENCIA
        // -----------------------------------------

        byte[]? evidenciaBytes = null;

        string? evidenciaTipoContenido =
            null;

        if (d.Evidencia is not null)
        {
            string[] tiposPermitidos =
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!tiposPermitidos.Contains(
                d.Evidencia.ContentType))
            {
                return BadRequest(new
                {
                    message =
                        "La evidencia debe ser una imagen JPG, PNG o WEBP."
                });
            }


            // Máximo 8 MB

            if (d.Evidencia.Length >
                8_000_000)
            {
                return BadRequest(new
                {
                    message =
                        "La evidencia no puede superar los 8 MB."
                });
            }


            using var memoria =
                new MemoryStream();

            await d.Evidencia
                .CopyToAsync(memoria);

            evidenciaBytes =
                memoria.ToArray();

            evidenciaTipoContenido =
                d.Evidencia.ContentType;
        }


        // -----------------------------------------
        // CREAR OBJETO INGRESO
        // -----------------------------------------

        var x = new IngresoEquipo
        {
            NumeroIngreso =
                numeroIngreso,

            FechaIngreso =
                d.FechaIngreso,

            Cliente =
                d.Cliente.Trim(),

            // NUEVO
            Cedula =
                d.Cedula.Trim(),

            Telefono =
                d.Telefono.Trim(),

            Correo =
                d.Correo?.Trim(),

            TipoEquipo =
                d.TipoEquipo.Trim(),

            Marca =
                d.Marca.Trim(),

            Modelo =
                d.Modelo.Trim(),

            ImeiSerie =
                d.ImeiSerie?.Trim()
                ?? string.Empty,

            Accesorios =
                d.Accesorios?.Trim()
                ?? string.Empty,

            EstadoFisico =
                d.EstadoFisico?.Trim()
                ?? string.Empty,

            FallaReportada =
                d.FallaReportada?.Trim()
                ?? string.Empty,

            Observaciones =
                d.Observaciones?.Trim(),

            // NUEVO
            Evidencia =
                evidenciaBytes,

            // NUEVO
            EvidenciaTipoContenido =
                evidenciaTipoContenido
        };


        // -----------------------------------------
        // GUARDAR EN POSTGRESQL
        // -----------------------------------------

        db.IngresosEquipos.Add(x);

        await db.SaveChangesAsync();


        // -----------------------------------------
        // GENERAR COMPROBANTE PDF
        // -----------------------------------------

        var archivo =
            ingresoPdf.Generar(x);


        return File(
            archivo,
            "application/pdf",
            $"{x.NumeroIngreso}.pdf"
        );
    }
}