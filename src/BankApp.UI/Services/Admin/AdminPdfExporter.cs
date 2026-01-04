using System;
using System.Data;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankApp.UI.Services.Admin
{
    /// <summary>
    /// PDF Exporter using QuestPDF for maximum stability and Turkish character support.
    /// </summary>
    public static class AdminPdfExporter
    {
        static AdminPdfExporter()
        {
            // Configure QuestPDF license (Community license for free usage)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Export DataTable to PDF file.
        /// </summary>
        /// <param name="dataTable">Data to export</param>
        /// <param name="filePath">Target file path</param>
        /// <exception cref="InvalidOperationException">If no data to export</exception>
        /// <exception cref="IOException">If file cannot be written</exception>
        public static void Export(DataTable dataTable, string filePath)
        {
            // Validation
            if (dataTable == null)
                throw new InvalidOperationException("DataTable null olamaz.");

            if (dataTable.Rows.Count == 0)
                throw new InvalidOperationException("Dışa aktarılacak veri bulunamadı.");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "Dosya yolu belirtilmedi.");

            // Check if directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Check if file is locked
            if (File.Exists(filePath))
            {
                try
                {
                    using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
                    {
                        // File is accessible
                    }
                }
                catch (IOException)
                {
                    throw new IOException($"Dosya başka bir program tarafından kullanılıyor: {filePath}");
                }
            }

            try
            {
                // Determine orientation based on column count
                bool useLandscape = dataTable.Columns.Count > 6;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Page setup
                        page.Size(useLandscape ? PageSizes.A4.Landscape() : PageSizes.A4);
                        page.Margin(1.5f, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                        // Header
                        page.Header().Column(col =>
                        {
                            col.Item().Text("NovaBank - Admin Raporu")
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            col.Item().Text($"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);

                            col.Item().PaddingBottom(10);
                        });

                        // Content - Table
                        page.Content().Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < dataTable.Columns.Count; i++)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            // Header row
                            table.Header(header =>
                            {
                                foreach (DataColumn col in dataTable.Columns)
                                {
                                    header.Cell()
                                        .Background(Colors.Blue.Darken2)
                                        .Padding(5)
                                        .Text(col.ColumnName)
                                        .FontColor(Colors.White)
                                        .Bold()
                                        .FontSize(9);
                                }
                            });

                            // Data rows
                            int rowIndex = 0;
                            foreach (DataRow row in dataTable.Rows)
                            {
                                var bgColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                foreach (DataColumn col in dataTable.Columns)
                                {
                                    var value = row[col]?.ToString() ?? "";
                                    // Truncate very long text
                                    if (value.Length > 100)
                                        value = value.Substring(0, 97) + "...";

                                    table.Cell()
                                        .Background(bgColor)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text(value)
                                        .FontSize(8);
                                }
                                rowIndex++;
                            }
                        });

                        // Footer
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Sayfa ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                });

                // Generate PDF
                document.GeneratePdf(filePath);

                System.Diagnostics.Debug.WriteLine($"[AdminPdfExporter] Exported {dataTable.Rows.Count} rows to {filePath}");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"Bu konuma yazma izniniz yok: {filePath}");
            }
            catch (IOException ex)
            {
                throw new IOException($"Dosya yazma hatası: {ex.Message}", ex);
            }
        }
    }
}
