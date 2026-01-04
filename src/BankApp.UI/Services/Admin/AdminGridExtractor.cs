using System;
using System.Data;
using System.Windows.Forms;

namespace BankApp.UI.Services.Admin
{
    /// <summary>
    /// Extracts visible grid data to DataTable for export operations.
    /// Maximum stability - never crashes, always returns valid DataTable.
    /// </summary>
    public static class AdminGridExtractor
    {
        private const int MaxFieldLength = 300;

        /// <summary>
        /// Extract visible data from DataGridView to DataTable.
        /// Uses FormattedValue for display text, handles nulls and checkboxes.
        /// </summary>
        public static DataTable ExtractVisibleData(DataGridView grid)
        {
            var dt = new DataTable();

            try
            {
                if (grid == null)
                {
                    System.Diagnostics.Debug.WriteLine("[AdminGridExtractor] Grid is null");
                    return dt;
                }

                if (grid.Columns.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[AdminGridExtractor] Grid has no columns");
                    return dt;
                }

                // Add only visible columns
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (col.Visible)
                    {
                        dt.Columns.Add(col.HeaderText ?? col.Name, typeof(string));
                    }
                }

                if (dt.Columns.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[AdminGridExtractor] No visible columns");
                    return dt;
                }

                // Add rows (skip new row placeholder)
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;

                    var dataRow = dt.NewRow();
                    int colIndex = 0;

                    foreach (DataGridViewColumn col in grid.Columns)
                    {
                        if (!col.Visible) continue;

                        try
                        {
                            var cell = row.Cells[col.Index];
                            string value = ExtractCellValue(cell);
                            dataRow[colIndex] = value;
                        }
                        catch
                        {
                            dataRow[colIndex] = "";
                        }
                        colIndex++;
                    }

                    dt.Rows.Add(dataRow);
                }

                System.Diagnostics.Debug.WriteLine($"[AdminGridExtractor] Extracted {dt.Rows.Count} rows, {dt.Columns.Count} columns");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminGridExtractor] Error: {ex.Message}");
                // Return empty DataTable on error
            }

            return dt;
        }

        private static string ExtractCellValue(DataGridViewCell cell)
        {
            if (cell == null) return "";

            // Handle checkbox columns
            if (cell is DataGridViewCheckBoxCell checkCell)
            {
                var val = checkCell.Value;
                if (val is bool b) return b ? "Evet" : "Hayır";
                if (val != null && bool.TryParse(val.ToString(), out bool parsed))
                    return parsed ? "Evet" : "Hayır";
                return "";
            }

            // Use FormattedValue (displayed text) if available
            string result = "";
            try
            {
                if (cell.FormattedValue != null)
                    result = cell.FormattedValue.ToString() ?? "";
                else if (cell.Value != null)
                    result = cell.Value.ToString() ?? "";
            }
            catch
            {
                result = cell.Value?.ToString() ?? "";
            }

            // Cap extremely long strings
            if (result.Length > MaxFieldLength)
            {
                result = result.Substring(0, MaxFieldLength - 3) + "...";
            }

            return result;
        }
    }
}
