using EasyData.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyData.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MarcasController(
    AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var marcas = await db.Marcas
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new
            {
                x.Id,
                x.Nombre
            })
            .ToListAsync();

        return Ok(marcas);
    }
}