using EasyData.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyData.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HistorialController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? buscar)
    {
        var query = db.IngresosEquipos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim().ToLower();

            query = query.Where(x =>
                x.NumeroIngreso.ToLower().Contains(texto) ||
                x.Cliente.ToLower().Contains(texto) ||
                x.Telefono.ToLower().Contains(texto) ||
                x.Marca.ToLower().Contains(texto) ||
                x.Modelo.ToLower().Contains(texto) ||
                x.ImeiSerie.ToLower().Contains(texto) ||
                x.TipoEquipo.ToLower().Contains(texto)
            );
        }

        var ingresos = await query
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        var resultado = new List<object>();

        foreach (var ingreso in ingresos)
        {
            var diagnostico = await db.Diagnosticos
                .AsNoTracking()
                .Where(d => d.IngresoEquipoId == ingreso.Id)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            resultado.Add(new
            {
                ingresoId = ingreso.Id,
                ingreso.NumeroIngreso,
                ingreso.TipoEquipo,
                ingreso.Cliente,
                ingreso.Telefono,
                ingreso.Marca,
                ingreso.Modelo,
                ingreso.ImeiSerie,
                ingreso.Estado,

                diagnosticoId = diagnostico?.Id,
                numeroInforme = diagnostico?.NumeroInforme,

                tieneDiagnostico = diagnostico != null,
                tienePdf = diagnostico != null
            });
        }

        return Ok(resultado);
    }
}