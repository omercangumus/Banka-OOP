using System;
using System.Data;
using System.IO;
using System.Text;

namespace BankApp.UI.Services.Admin
{
    /// <summary>
    /// CSV Exporter - No external libraries, UTF-8 BOM for Excel compatibility.
    /// Maximum stability - detailed exceptions for debugging.
    /// </summary>
    public static class AdminCsvExporter
    {
        // Using semicolon for Turkish Excel locale compatibility
        private const char Separator = ';';

        /// <summary>
        /// Export DataTable to CSV file with UTF-8 BOM encoding.
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

            var sb = new StringBuilder();

            try
            {
                // Write header row
                var headers = new string[dataTable.Columns.Count];
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    headers[i] = EscapeField(dataTable.Columns[i].ColumnName);
                }
                sb.AppendLine(string.Join(Separator.ToString(), headers));

                // Write data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    var values = new string[dataTable.Columns.Count];
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        values[i] = EscapeField(row[i]?.ToString() ?? "");
                    }
                    sb.AppendLine(string.Join(Separator.ToString(), values));
                }

                // Write with UTF-8 BOM for Excel compatibility
                var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                File.WriteAllText(filePath, sb.ToString(), utf8WithBom);

                System.Diagnostics.Debug.WriteLine($"[AdminCsvExporter] Exported {dataTable.Rows.Count} rows to {filePath}");
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

        /// <summary>
        /// Escape CSV field according to RFC 4180.
        /// If field contains separator, quote, or newline -> wrap in quotes and double internal quotes.
        /// </summary>
        private static string EscapeField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            bool needsQuotes = field.Contains(Separator) || 
                              field.Contains('"') || 
                              field.Contains('\n') || 
                              field.Contains('\r');

            if (needsQuotes)
            {
                // Double any existing quotes
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }
    }
}
