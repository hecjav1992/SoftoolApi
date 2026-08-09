using EasyData.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace EasyData.Api.Services;
public class PdfService {
  public byte[] Generar(Diagnostico d) => Document.Create(container => {
    container.Page(page => {
      page.Size(PageSizes.Letter); page.Margin(35); page.DefaultTextStyle(x => x.FontSize(10));
      page.Header().Column(c => {
        c.Item().Height(7).Background("#18A7C9");
        c.Item().PaddingTop(15).Row(r => {
          r.RelativeItem().Column(x => { x.Item().Text("EASY DATA").Bold().FontSize(20).FontColor("#102A43"); x.Item().Text("TECNOLOGÍA Y SERVICIO TÉCNICO").FontSize(8); });
          r.ConstantItem(180).AlignRight().Column(x => { x.Item().Text("INFORME TÉCNICO").Bold(); x.Item().Text(d.NumeroInforme); });
        });
      });
      page.Content().PaddingVertical(20).Column(c => {
        c.Spacing(14);
        c.Item().Text($"DIAGNÓSTICO DE {d.IngresoEquipo?.TipoEquipo.ToUpper() ?? ""}").FontColor("#0783A0").Bold();
        c.Item().Text($"{d.Marca} {d.Modelo}").FontSize(22).Bold().FontColor("#102A43");
        c.Item().Table(t => { t.ColumnsDefinition(cols => { cols.RelativeColumn(); cols.RelativeColumn(); cols.RelativeColumn(); cols.RelativeColumn(); });
          Celda(t,"CLIENTE",d.Cliente); Celda(t,"TELÉFONO",d.Telefono); Celda(t,"FECHA",d.Fecha.ToString("dd/MM/yyyy")); Celda(t,"IMEI / SERIE",d.ImeiSerie ?? "—"); });
        Seccion(c,"01","Diagnóstico técnico",d.DiagnosticoTecnico);
        Seccion(c,"02","Recomendación",d.Recomendacion);
        c.Item().Text("03  Evidencia fotográfica").Bold().FontColor("#102A43");
        if (d.Evidencia is { Length: > 0 }) c.Item().AlignCenter().MaxHeight(260).Image(d.Evidencia).FitArea(); else c.Item().Text("No se adjuntó evidencia fotográfica.").Italic();
      });
      page.Footer().AlignCenter().Text(x => { x.Span("Mgtr. Héctor J. Degracia · EASY DATA · Capira, Panamá Oeste · Página "); x.CurrentPageNumber(); });
    });
  }).GeneratePdf();

  static void Celda(TableDescriptor t,string titulo,string valor) => t.Cell().Border(1).BorderColor("#DBE4EC").Padding(8).Column(c => { c.Item().Text(titulo).FontSize(7).FontColor("#64748B"); c.Item().Text(valor).Bold(); });
  static void Seccion(ColumnDescriptor c,string n,string titulo,string texto) { c.Item().Text($"{n}  {titulo}").Bold().FontColor("#102A43"); c.Item().PaddingLeft(20).Text(texto).LineHeight(1.4f); }
}
