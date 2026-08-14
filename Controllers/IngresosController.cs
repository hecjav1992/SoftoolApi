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
public class IngresosController(AppDbContext db, IngresoPdfService ingresoPdf) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? buscar)
    {
        var query = db.IngresosEquipos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var patron = $"%{buscar.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.NumeroIngreso, patron) ||
                EF.Functions.ILike(x.Cliente, patron) ||
                EF.Functions.ILike(x.Telefono, patron) ||
                EF.Functions.ILike(x.TipoEquipo, patron) ||
                EF.Functions.ILike(x.Marca, patron) ||
                EF.Functions.ILike(x.Modelo, patron) ||
                EF.Functions.ILike(x.ImeiSerie, patron) ||
                (x.Correo != null && EF.Functions.ILike(x.Correo, patron))
            );
        }

        var resultado = await query
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(resultado);
    }

    [HttpGet("buscar/{numeroIngreso}")]
    public async Task<IActionResult> Buscar(string numeroIngreso)
    {
        var x = await db.IngresosEquipos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.NumeroIngreso == numeroIngreso.Trim());

        return x is null
            ? NotFound(new { message = "No existe un equipo registrado con ese número de ingreso." })
            : Ok(x);
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var ingreso = await db.IngresosEquipos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ingreso is null)
            return NotFound(new { message = "No se encontró el ingreso solicitado." });

        var archivo = ingresoPdf.Generar(ingreso);

        return File(
            archivo,
            "application/pdf",
            $"{ingreso.NumeroIngreso}.pdf"
        );
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CrearIngresoDto d)
    {
        var numeroIngreso = d.NumeroIngreso.Trim();

        if (await db.IngresosEquipos.AnyAsync(x => x.NumeroIngreso == numeroIngreso))
            return Conflict(new { message = "Número de ingreso duplicado." });

        var x = new IngresoEquipo
        {
            NumeroIngreso = numeroIngreso,
            FechaIngreso = d.FechaIngreso,
            Cliente = d.Cliente.Trim(),
            Telefono = d.Telefono.Trim(),
            Correo = d.Correo?.Trim(),
            TipoEquipo = d.TipoEquipo.Trim(),
            Marca = d.Marca.Trim(),
            Modelo = d.Modelo.Trim(),
            ImeiSerie = d.ImeiSerie.Trim(),
            Accesorios = d.Accesorios.Trim(),
            EstadoFisico = d.EstadoFisico.Trim(),
            FallaReportada = d.FallaReportada.Trim(),
            Observaciones = d.Observaciones?.Trim()
        };

        db.IngresosEquipos.Add(x);
        await db.SaveChangesAsync();

        var archivo = ingresoPdf.Generar(x);

        return File(
            archivo,
            "application/pdf",
            $"{x.NumeroIngreso}.pdf"
        );
    }
}
