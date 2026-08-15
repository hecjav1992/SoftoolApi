using EasyData.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EasyData.Api.Services;

public class IngresoPdfService
{
    public byte[] Generar(IngresoEquipo ingreso) => Document.Create(container =>
    {
          var logoPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "Images",
            "logo.png"
        );
        container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(35);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Column(c =>
            {
                c.Item().Height(7).Background("#18A7C9");

                c.Item().PaddingTop(15).Row(r =>
                {
                    r.RelativeItem().Column(x =>
                    {
                        x.Item().Text("COMPROBANTE DE INGRESO").Bold();
                        x.Item().Text(ingreso.NumeroIngreso)
                            .FontColor("#0783A0")
                            .Bold();
                    });

                    r.ConstantItem(200).AlignRight().Column(x =>
                    {
                      x.Item()
                      .Width(100)
                      .Height(50)
                      .Padding(0)
                      .AlignCenter()
                      .Image(logoPath)
                      .FitArea();

                         x.Item().Text("EASY DATA")
                            .Bold()
                            .FontSize(20)
                            .FontColor("#102A43");

                        x.Item().Text("TECNOLOGÍA Y SERVICIO TÉCNICO")
                            .FontSize(8);

                        x.Item().Text("easydata10@gmail.com | +507 6884-4342")
                            .FontSize(8);
                   
                    });
                });
            });

            page.Content().PaddingVertical(20).Column(c =>
            {
                c.Spacing(14);

                c.Item()
                    .Text($"INGRESO DE {ingreso.TipoEquipo.Trim().ToUpperInvariant()}")
                    .FontColor("#0783A0")
                    .Bold();

                c.Item()
                    .Text($"{ingreso.Marca} {ingreso.Modelo}")
                    .FontSize(22)
                    .Bold()
                    .FontColor("#102A43");

                SeccionTitulo(c, "01", "Datos del cliente");

                c.Item().Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                    });

                    Celda(t, "CLIENTE", ingreso.Cliente);
                    Celda(t, "TELÉFONO", ingreso.Telefono);
                    Celda(t, "CORREO", ingreso.Correo);
                    Celda(t, "FECHA DE INGRESO", ingreso.FechaIngreso.ToString("dd/MM/yyyy"));
                });

                SeccionTitulo(c, "02", "Datos del equipo");

                c.Item().Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                    });

                    Celda(t, "TIPO DE EQUIPO", ingreso.TipoEquipo);
                    Celda(t, "ESTADO", ingreso.Estado);
                    Celda(t, "MARCA", ingreso.Marca);
                    Celda(t, "MODELO", ingreso.Modelo);
                    Celda(t, "IMEI / SERIE", ingreso.ImeiSerie);
                    Celda(t, "ACCESORIOS", ingreso.Accesorios);
                });

                Seccion(c, "03", "Estado físico", ingreso.EstadoFisico);
                Seccion(c, "04", "Falla reportada", ingreso.FallaReportada);

                if (!string.IsNullOrWhiteSpace(ingreso.Observaciones))
                    Seccion(c, "05", "Observaciones", ingreso.Observaciones);

                c.Item()
                    .PaddingTop(6)
                    .Text("Constancia de recepción")
                    .Bold()
                    .FontColor("#102A43");

                c.Item()
                    .PaddingLeft(20)
                    .Text("El presente documento certifica la recepción del equipo descrito por EASY DATA para evaluación y servicio técnico. Conserve este comprobante para futuras consultas.")
                    .LineHeight(1.35f);

                c.Item().PaddingTop(34).Row(row =>
                {
                    row.RelativeItem().Column(x =>
                    {
                        x.Item().LineHorizontal(1).LineColor("#64748B");
                        x.Item().AlignCenter().PaddingTop(5)
                            .Text("Firma del responsable")
                            .FontSize(8)
                            .FontColor("#64748B");
                    });

                    row.ConstantItem(55);

                    row.RelativeItem().Column(x =>
                    {
                        x.Item().LineHorizontal(1).LineColor("#64748B");
                        x.Item().AlignCenter().PaddingTop(5)
                            .Text("Firma del cliente")
                            .FontSize(8)
                            .FontColor("#64748B");
                    });
                });
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("EASY DATA · Capira, Panamá Oeste · Página ");
                x.CurrentPageNumber();
            });
        });
    }).GeneratePdf();

    private static void Celda(TableDescriptor t, string titulo, string? valor) =>
        t.Cell()
            .Border(1)
            .BorderColor("#DBE4EC")
            .Padding(8)
            .Column(c =>
            {
                c.Item().Text(titulo).FontSize(7).FontColor("#64748B");
                c.Item().Text(string.IsNullOrWhiteSpace(valor) ? "—" : valor).Bold();
            });

    private static void SeccionTitulo(ColumnDescriptor c, string numero, string titulo) =>
        c.Item().Text($"{numero}  {titulo}").Bold().FontColor("#102A43");

    private static void Seccion(ColumnDescriptor c, string numero, string titulo, string? texto)
    {
        SeccionTitulo(c, numero, titulo);
        c.Item()
            .PaddingLeft(20)
            .Text(string.IsNullOrWhiteSpace(texto) ? "—" : texto)
            .LineHeight(1.35f);
    }
}
