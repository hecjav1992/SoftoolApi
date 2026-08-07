using System.ComponentModel.DataAnnotations;
namespace EasyData.Api.DTOs;

public class CrearDiagnosticoDto
{
  [Required] public string NumeroIngreso { get; set; } = string.Empty;
  [Required] public string NumeroInforme { get; set; } = string.Empty;
  [Required] public DateOnly Fecha { get; set; }
  [Required] public string DiagnosticoTecnico { get; set; } = string.Empty;
  [Required] public string Recomendacion { get; set; } = string.Empty;
  public IFormFile? Evidencia { get; set; }
}
