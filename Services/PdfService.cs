using EasyData.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EasyData.Api.Services;

public class PdfService
{
    public byte[] Generar(Diagnostico d) => Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(35);

            page.DefaultTextStyle(x =>
                x.FontSize(10)
            );

            // =========================
            // ENCABEZADO
            // =========================

            page.Header().Column(c =>
            {
                c.Item()
                    .Height(7)
                    .Background("#18A7C9");

                c.Item()
                    .PaddingTop(15)
                    .Row(r =>
                    {
                        r.RelativeItem()
                            .Column(x =>
                            {
                                x.Item()
                                    .Text("EASY DATA")
                                    .Bold()
                                    .FontSize(20)
                                    .FontColor("#102A43");

                                x.Item()
                                    .Text("TECNOLOGÍA Y SERVICIO TÉCNICO")
                                    .FontSize(8);

                                x.Item()
                                    .Text("easydata10@gmail.com | +507 6884-4342")
                                    .FontSize(8);
                            });

                        r.ConstantItem(180)
                            .AlignRight()
                            .Column(x =>
                            {
                                x.Item()
                                    .Text("INFORME TÉCNICO")
                                    .Bold();

                                x.Item()
                                    .Text(
                                        string.IsNullOrWhiteSpace(d.NumeroInforme)
                                            ? "—"
                                            : d.NumeroInforme
                                    )
                                    .FontColor("#0783A0")
                                    .Bold();
                            });
                    });
            });

            // =========================
            // CONTENIDO
            // =========================

            page.Content()
                .PaddingVertical(20)
                .Column(c =>
                {
                    c.Spacing(14);

                    // TIPO DE EQUIPO

                    var tipoEquipo =
                        d.IngresoEquipo?.TipoEquipo;

                    c.Item()
                        .Text(
                            string.IsNullOrWhiteSpace(tipoEquipo)
                                ? "DIAGNÓSTICO TÉCNICO"
                                : $"DIAGNÓSTICO DE {tipoEquipo.Trim().ToUpperInvariant()}"
                        )
                        .FontColor("#0783A0")
                        .Bold();

                    // MARCA Y MODELO

                    c.Item()
                        .Text($"{d.Marca} {d.Modelo}")
                        .FontSize(22)
                        .Bold()
                        .FontColor("#102A43");

                    // =========================
                    // DATOS GENERALES
                    // =========================

                    c.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        Celda(
                            t,
                            "CLIENTE",
                            d.Cliente
                        );

                        Celda(
                            t,
                            "TELÉFONO",
                            d.Telefono
                        );

                        Celda(
                            t,
                            "FECHA",
                            d.Fecha.ToString("dd/MM/yyyy")
                        );

                        Celda(
                            t,
                            "IMEI / SERIE",
                            d.ImeiSerie
                        );
                    });

                    // =========================
                    // 01 DIAGNÓSTICO
                    // =========================

                    Seccion(
                        c,
                        "01",
                        "Diagnóstico técnico",
                        d.DiagnosticoTecnico
                    );

                    // =========================
                    // 02 RECOMENDACIÓN
                    // =========================

                    Seccion(
                        c,
                        "02",
                        "Recomendación",
                        d.Recomendacion
                    );

                    // =========================
                    // 03 DATOS DEL SERVICIO
                    // =========================

                    c.Item()
                        .Text("03  Datos del servicio")
                        .Bold()
                        .FontColor("#102A43");

                    c.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        Celda(
                            t,
                            "PRECIO DE REPARACIÓN",
                            d.PrecioReparacion.HasValue
                                ? $"B/. {d.PrecioReparacion.Value:N2}"
                                : "—"
                        );

                        Celda(
                            t,
                            "GARANTÍA VÁLIDA HASTA",
                            d.FechaVigenciaGarantia.HasValue
                                ? d.FechaVigenciaGarantia.Value
                                    .ToString("dd/MM/yyyy")
                                : "—"
                        );
                    });

                    // =========================
                    // 04 EVIDENCIA
                    // =========================

                    c.Item()
                        .Text("04  Evidencia fotográfica")
                        .Bold()
                        .FontColor("#102A43");

                    if (d.Evidencia is { Length: > 0 })
                    {
                        c.Item()
                            .AlignCenter()
                            .MaxHeight(240)
                            .Image(d.Evidencia)
                            .FitArea();
                    }
                    else
                    {
                        c.Item()
                            .PaddingLeft(20)
                            .Text(
                                "No se adjuntó evidencia fotográfica."
                            )
                            .Italic()
                            .FontColor("#64748B");
                    }

                    // =========================
                    // INFORMACIÓN DE GARANTÍA
                    // =========================

                    if (d.FechaVigenciaGarantia.HasValue)
                    {
                        c.Item()
                            .PaddingTop(5)
                            .Text(
                                $"Garantía del servicio válida hasta el {d.FechaVigenciaGarantia.Value:dd/MM/yyyy}."
                            )
                            .FontSize(9)
                            .FontColor("#64748B");
                    }

                    // =========================
                    // FIRMAS
                    // =========================

                    c.Item()
                        .PaddingTop(35)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(x =>
                                {
                                    x.Item()
                                        .LineHorizontal(1)
                                        .LineColor("#64748B");

                                    x.Item()
                                        .AlignCenter()
                                        .PaddingTop(5)
                                        .Text(
                                            "Firma del responsable"
                                        )
                                        .FontSize(8)
                                        .FontColor("#64748B");
                                });

                            row.ConstantItem(55);

                            row.RelativeItem()
                                .Column(x =>
                                {
                                    x.Item()
                                        .LineHorizontal(1)
                                        .LineColor("#64748B");

                                    x.Item()
                                        .AlignCenter()
                                        .PaddingTop(5)
                                        .Text(
                                            "Firma del cliente"
                                        )
                                        .FontSize(8)
                                        .FontColor("#64748B");
                                });
                        });
                });

            // =========================
            // PIE DE PÁGINA
            // =========================

            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.Span(
                        "Mgtr. Héctor J. Degracia · EASY DATA · Capira, Panamá Oeste · Página "
                    );

                    x.CurrentPageNumber();
                });
        });
    }).GeneratePdf();


    // =========================
    // CELDA
    // =========================

    private static void Celda(
        TableDescriptor t,
        string titulo,
        string? valor)
    {
        t.Cell()
            .Border(1)
            .BorderColor("#DBE4EC")
            .Padding(8)
            .Column(c =>
            {
                c.Item()
                    .Text(titulo)
                    .FontSize(7)
                    .FontColor("#64748B");

                c.Item()
                    .Text(
                        string.IsNullOrWhiteSpace(valor)
                            ? "—"
                            : valor
                    )
                    .Bold();
            });
    }


    // =========================
    // SECCIÓN
    // =========================

    private static void Seccion(
        ColumnDescriptor c,
        string numero,
        string titulo,
        string? texto)
    {
        c.Item()
            .Text($"{numero}  {titulo}")
            .Bold()
            .FontColor("#102A43");

        c.Item()
            .PaddingLeft(20)
            .Text(
                string.IsNullOrWhiteSpace(texto)
                    ? "—"
                    : texto
            )
            .LineHeight(1.4f);
    }
}