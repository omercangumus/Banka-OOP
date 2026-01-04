using System;
using System.Data;
using System.Windows.Forms;

namespace BankApp.UI.Services.Admin
{
    public static class AdminGridExtractor
    {
        public static DataTable ExtractVisibleData(DataGridView grid)
        {
            if (grid == null || grid.Columns.Count == 0)
                return new DataTable();

            var dt = new DataTable();

            try
            {
                // Sütunları Oluştur
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (col.Visible) // Sadece görünenleri al
                    {
                        dt.Columns.Add(col.HeaderText ?? col.Name);
                    }
                }

                // Satırları Doldur
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue; // Yeni ekleme satırını atla

                    var dr = dt.NewRow();
                    int colIndex = 0;

                    foreach (DataGridViewColumn col in grid.Columns)
                    {
                        if (col.Visible)
                        {
                            var value = row.Cells[col.Index].Value;
                            // NULL KONTROLÜ (Çökme sebebi genelde burasıdır)
                            dr[colIndex] = value != null ? value.ToString() : "";
                            colIndex++;
                        }
                    }
                    dt.Rows.Add(dr);
                }
                
                System.Diagnostics.Debug.WriteLine($"[AdminGridExtractor] Extracted {dt.Rows.Count} rows.");
            }
            catch (Exception ex)
            {
                // Hata olursa boş tablo dön, çökmesin
                System.Diagnostics.Debug.WriteLine($"Extractor Hatası: {ex.Message}");
            }

            return dt;
        }
    }
}
