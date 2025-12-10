using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services.Dashboard;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Core;

namespace BankApp.UI.Controls
{
    public partial class AssetAllocationChart : UserControl
    {
        private ChartControl chart;
        private LabelControl lblEmpty;
        private readonly DashboardSummaryService _summaryService;

        public AssetAllocationChart()
        {
            var context = new DapperContext();
            _summaryService = new DashboardSummaryService(context);
            
            InitializeComponent();
            LoadChartData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(380, 300);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Padding = new Padding(10);

            chart = new ChartControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                AppearanceNameSerializable = "Dark"
            };

            chart.Titles.Add(new ChartTitle
            {
                Text = "📊 Varlık Dağılımı",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextColor = Color.White,
                Alignment = StringAlignment.Near
            });
            
            // Empty state label
            lblEmpty = new LabelControl
            {
                Text = "📭 Henüz varlık bulunmuyor",
                Appearance = { 
                    Font = new Font("Segoe UI", 11, FontStyle.Regular),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center }
                },
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Fill,
                Visible = false
            };

            this.Controls.Add(chart);
            this.Controls.Add(lblEmpty);
        }

        public async void RefreshData()
        {
            await LoadChartDataAsync();
        }

        private async void LoadChartData()
        {
            await LoadChartDataAsync();
        }

        private async System.Threading.Tasks.Task LoadChartDataAsync()
        {
            try
            {
                // Use Asset Allocation (Nakit / Yatırım / Borç)
                var allocationData = await _summaryService.GetAssetAllocationAsync(AppEvents.CurrentSession.UserId);
                
                if (allocationData != null && allocationData.Any() && allocationData[0].Category != "Veri yok")
                {
                    lblEmpty.Visible = false;
                    chart.Visible = true;
                    
                    Series series = new Series("Varlıklar", ViewType.Doughnut);
                    
                    foreach (var item in allocationData)
                    {
                        var point = new SeriesPoint(item.Category, (double)item.Amount);
                        point.Color = ColorTranslator.FromHtml(item.Color);
                        series.Points.Add(point);
                    }
                    
                    // Doughnut styling
                    if (series.View is DoughnutSeriesView doughnutView)
                    {
                        doughnutView.HoleRadiusPercent = 45;
                        doughnutView.ExplodedDistancePercentage = 5;
                    }
                    
                    // Premium label format: "Kategori - ₺X (Y%)"
                    series.Label.TextPattern = "{A}\n₺{V:N0} ({VP:P0})";
                    series.Label.ResolveOverlappingMode = ResolveOverlappingMode.Default;
                    series.Label.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                    series.Label.TextColor = Color.White;
                    series.LegendTextPattern = "{A} - ₺{V:N0} ({VP:P0})";
                    
                    chart.Series.Clear();
                    chart.Series.Add(series);
                    
                    // Modern dark theme styling
                    chart.PaletteName = "Mixed";
                    chart.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
                    chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
                    chart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
                    chart.Legend.AlignmentVertical = LegendAlignmentVertical.BottomOutside;
                    chart.Legend.Direction = LegendDirection.LeftToRight;
                    chart.Legend.TextColor = Color.White;
                    chart.Legend.BackColor = Color.Transparent;
                    chart.Legend.Font = new Font("Segoe UI", 9);
                }
                else
                {
                    // Show empty state
                    chart.Visible = false;
                    lblEmpty.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadChartData Error: {ex.Message}");
                chart.Visible = false;
                lblEmpty.Visible = true;
                lblEmpty.Text = "⚠️ Veri yüklenemedi";
            }
        }
    }
}
