using EasyData.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyData.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/inteligencia-negocio")]
public class InteligenciaNegocioController(AppDbContext db) : ControllerBase
{
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen()
    {
        var totalIngresos = await db.IngresosEquipos
            .AsNoTracking()
            .CountAsync();

        var totalDiagnosticos = await db.Diagnosticos
            .AsNoTracking()
            .CountAsync();

        var diagnosticados = await db.IngresosEquipos
            .AsNoTracking()
            .CountAsync(x => x.Estado == "Diagnosticado");

        var pendientes = await db.IngresosEquipos
            .AsNoTracking()
            .CountAsync(x => x.Estado != "Diagnosticado");

        var porTipoEquipo = await db.IngresosEquipos
            .AsNoTracking()
            .Where(x => x.TipoEquipo != null && x.TipoEquipo != "")
            .GroupBy(x => x.TipoEquipo)
            .Select(g => new
            {
                nombre = g.Key,
                cantidad = g.Count()
            })
            .OrderByDescending(x => x.cantidad)
            .ToListAsync();

        var porMarca = await db.IngresosEquipos
            .AsNoTracking()
            .Where(x => x.Marca != null && x.Marca != "")
            .GroupBy(x => x.Marca)
            .Select(g => new
            {
                nombre = g.Key,
                cantidad = g.Count()
            })
            .OrderByDescending(x => x.cantidad)
            .Take(8)
            .ToListAsync();

        var porEstado = await db.IngresosEquipos
            .AsNoTracking()
            .Where(x => x.Estado != null && x.Estado != "")
            .GroupBy(x => x.Estado)
            .Select(g => new
            {
                nombre = g.Key,
                cantidad = g.Count()
            })
            .OrderByDescending(x => x.cantidad)
            .ToListAsync();

        // Solo trae las fechas necesarias y agrupa en memoria para evitar
        // problemas de traducción SQL entre proveedores.
        var fechasDiagnosticos = await db.Diagnosticos
            .AsNoTracking()
            .Select(x => x.CreadoEnUtc)
            .ToListAsync();

        var inicio = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
            .AddMonths(-5);

        var diagnosticosPorMes = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var mes = inicio.AddMonths(i);
                var cantidad = fechasDiagnosticos.Count(f =>
                    f.Year == mes.Year && f.Month == mes.Month);

                return new
                {
                    periodo = mes.ToString("yyyy-MM"),
                    etiqueta = mes.ToString("MMM yyyy"),
                    cantidad
                };
            })
            .ToList();

        var tasaDiagnostico = totalIngresos == 0
            ? 0
            : Math.Round((double)diagnosticados / totalIngresos * 100, 1);

        return Ok(new
        {
            generadoEnUtc = DateTime.UtcNow,
            kpis = new
            {
                totalIngresos,
                totalDiagnosticos,
                diagnosticados,
                pendientes,
                tasaDiagnostico
            },
            porTipoEquipo,
            porMarca,
            porEstado,
            diagnosticosPorMes
        });
    }
}
