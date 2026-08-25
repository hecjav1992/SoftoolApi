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
[Route("api/[controller]")]
public class DiagnosticosController(AppDbContext db, PdfService pdf) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult> Listar() => Ok(await db.Diagnosticos
    .AsNoTracking()
    .OrderByDescending(x => x.Id)
    .Select(x => new
    {
      x.Id, x.NumeroInforme, x.Fecha, x.Cliente, x.Telefono, x.Marca,
      x.Modelo, x.ImeiSerie, x.DiagnosticoTecnico, x.Recomendacion, x.CreadoEnUtc
    })
    .ToListAsync());

  [HttpPost("generar-pdf")]
  [RequestSizeLimit(10_000_000)]
  public async Task<IActionResult> CrearYGenerarPdf([FromForm] CrearDiagnosticoDto dto)
  {
    var numeroIngreso = dto.NumeroIngreso.Trim();
    var ingreso = await db.IngresosEquipos
      .FirstOrDefaultAsync(x => x.NumeroIngreso == numeroIngreso);

    if (ingreso is null)
      return NotFound(new
      {
        message = "El equipo no está registrado en Ingreso de equipos. Regístrelo antes de generar el diagnóstico."
      });

    if (await db.Diagnosticos.AnyAsync(x => x.NumeroInforme == dto.NumeroInforme.Trim()))
      return Conflict(new { message = "Ya existe un diagnóstico con ese número de informe." });

    byte[]? bytes = null;
    string? tipo = null;

    if (dto.Evidencia is not null)
    {
      if (!new[] { "image/jpeg", "image/png", "image/webp" }.Contains(dto.Evidencia.ContentType))
        return BadRequest(new { message = "La evidencia debe ser JPG, PNG o WEBP." });

      using var ms = new MemoryStream();
      await dto.Evidencia.CopyToAsync(ms);
      bytes = ms.ToArray();
      tipo = dto.Evidencia.ContentType;
    }

    // Los datos del equipo se toman del ingreso registrado, no del navegador.
    var diagnostico = new Diagnostico
    {
     
      NumeroInforme = dto.NumeroInforme.Trim(),
      Fecha = dto.Fecha,
      Cliente = ingreso.Cliente.Trim(),
      Telefono = ingreso.Telefono.Trim(),
      Marca = ingreso.Marca.Trim(),
      Modelo = ingreso.Modelo.Trim(),
      ImeiSerie = ingreso.ImeiSerie.Trim(),
      DiagnosticoTecnico = dto.DiagnosticoTecnico.Trim(),
      Recomendacion = dto.Recomendacion.Trim(),
      Evidencia = bytes,
      EvidenciaTipoContenido = tipo,
      IngresoEquipoId = ingreso.Id,
      IngresoEquipo = ingreso,
      PrecioReparacion = dto.PrecioReparacion,
      FechaVigenciaGarantia = dto.FechaVigenciaGarantia,
    };

    db.Diagnosticos.Add(diagnostico);

    var ingresoActual = await db.IngresosEquipos.FirstAsync(x => x.Id == ingreso.Id);
    ingresoActual.Estado = "Diagnosticado";

    await db.SaveChangesAsync();

    return File(pdf.Generar(diagnostico), "application/pdf", $"{diagnostico.NumeroInforme}.pdf");
  }

   [HttpGet("{id:long}/pdf")]
  public async Task<IActionResult> DescargarPdf(long id)
  {
    var d = await db.Diagnosticos.FindAsync(id);
    return d is null
      ? NotFound()
      : File(pdf.Generar(d), "application/pdf", $"{d.NumeroInforme}.pdf");
  }

}
