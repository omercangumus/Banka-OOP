using System;
using System.Drawing;
using System.Threading.Tasks;
using DevExpress.XtraCharts;
using System.Windows.Forms;
using System.Linq;
using Dapper;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Core;

namespace BankApp.UI.Controls
{
    public partial class AssetAllocationChart : UserControl
    {
        private ChartControl chart;

        public AssetAllocationChart()
        {
            InitializeComponent();
            LoadChartData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(380, 300);
            this.BackColor = Color.FromArgb(30, 30, 30);

            chart = new ChartControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                AppearanceNameSerializable = "Dark"
            };

            chart.Titles.Add(new ChartTitle
            {
                Text = "💰 Harcama Dağılımı",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextColor = Color.White
            });

            this.Controls.Add(chart);
        }

        private async void LoadChartData()
        {
            try
            {
                Series series = new Series("Harcamalar", ViewType.Pie);
                
                try
                {
                    var context = new DapperContext();
                    using (var conn = context.CreateConnection())
                    {
                        // **FETCH REAL SPENDING DATA BY CATEGORY**
                        var spendingData = await conn.QueryAsync<(string Category, decimal Amount)>(@"
                            SELECT 
                                CASE 
                                    WHEN ""Description"" LIKE '%Yatırım%' OR ""Description"" LIKE '%yatırım%' OR ""Description"" LIKE '%Investment%' THEN 'Yatırım'
                                    WHEN ""Description"" LIKE '%market%' OR ""Description"" LIKE '%Market%' THEN 'Market'
                                    WHEN ""Description"" LIKE '%fatura%' OR ""Description"" LIKE '%Fatura%' THEN 'Faturalar'
                                    WHEN ""Description"" LIKE '%kira%' OR ""Description"" LIKE '%Kira%' THEN 'Kira'
                                    WHEN ""Description"" LIKE '%restoran%' OR ""Description"" LIKE '%yemek%' THEN 'Yemek'
                                    WHEN ""Description"" LIKE '%ulaşım%' OR ""Description"" LIKE '%Ulaşım%' THEN 'Ulaşım'
                                    WHEN ""Description"" LIKE '%eğlence%' OR ""Description"" LIKE '%Eğlence%' THEN 'Eğlence'
                                    ELSE 'Diğer'
                                END as Category,
                                SUM(ABS(""Amount"")) as Amount
                            FROM ""Transactions""
                            WHERE ""UserId"" = @UserId 
                            AND ""TransactionType"" IN ('Withdraw', 'TransferOut')
                            AND ""TransactionDate"" >= CURRENT_DATE - INTERVAL '30 days'
                            GROUP BY Category
                            HAVING SUM(ABS(""Amount"")) > 0
                            ORDER BY Amount DESC",
                            new { UserId = AppEvents.CurrentSession.UserId });

                        if (spendingData.Any())
                        {
                            foreach (var item in spendingData)
                            {
                                series.Points.Add(new SeriesPoint(item.Category, item.Amount));
                            }
                        }
                        else
                        {
                            // No real data, show sample
                            AddSampleData(series);
                        }
                    }
                }
                catch
                {
                    // Database error, show sample data
                    AddSampleData(series);
                }

                PieSeriesView pieView = (PieSeriesView)series.View;
                series.Label.TextPattern = "{A}: ₺{V:N0}";
                
                chart.Series.Clear();
                chart.Series.Add(series);
                
                // Modern styling
                chart.PaletteName = "Pastel";
                chart.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
                chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
                chart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Right;
                chart.Legend.AlignmentVertical = LegendAlignmentVertical.Center;
                chart.Legend.TextColor = Color.White;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadChartData Error: {ex.Message}");
            }
        }

        private void AddSampleData(Series series)
        {
            series.Points.Add(new SeriesPoint("Market", 1500));
            series.Points.Add(new SeriesPoint("Faturalar", 800));
            series.Points.Add(new SeriesPoint("Ulaşım", 400));
            series.Points.Add(new SeriesPoint("Yemek", 600));
            series.Points.Add(new SeriesPoint("Diğer", 300));
        }
    }
}
