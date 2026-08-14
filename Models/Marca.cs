namespace EasyData.Api.Models;

public class Marca
{
    public long Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}