namespace EasyData.Api.DTOs; 
public class CrearIngresoDto{
    public string NumeroIngreso{get;set;}="";
    public DateOnly FechaIngreso{get;set;}
    public string Cliente{get;set;}="";
    public string Telefono{get;set;}="";
    public string? Correo{get;set;}
    public string TipoEquipo{get;set;}="";
    public string Marca{get;set;}="";
    public string Modelo{get;set;}="";
    public string ImeiSerie{get;set;}="";
    public string Accesorios{get;set;}="";
    public string EstadoFisico{get;set;}="";
    public string FallaReportada{get;set;}="";
    public string? Observaciones{get;set;}}