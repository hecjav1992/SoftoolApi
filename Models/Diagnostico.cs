namespace EasyData.Api.Models;
public class Diagnostico {
  public long Id { get; set; }
  public string NumeroInforme { get; set; } = string.Empty;
  public DateOnly Fecha { get; set; }
  public string Cliente { get; set; } = string.Empty;
  public string Telefono { get; set; } = string.Empty;
  public string Marca { get; set; } = string.Empty;
  public string Modelo { get; set; } = string.Empty;
  public string? TipoEquipo { get; set; } 

  public string? ImeiSerie { get; set; }
  public string DiagnosticoTecnico { get; set; } = string.Empty;
  public string Recomendacion { get; set; } = string.Empty;
  public byte[]? Evidencia { get; set; }
  public string? EvidenciaTipoContenido { get; set; }
  public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
}
