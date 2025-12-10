using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using BankApp.Infrastructure.Services.Dashboard;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Core;
using Dapper;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// Gerçek portföy görünümü - varlıklar, dağılım, performans
    /// </summary>
    public class PortfolioView : UserControl
    {
        private readonly IDashboardService _dashboardService;
        private TableLayoutPanel mainLayout;
        private PanelControl summaryPanel;
        private PanelControl chartPanel;
        private PanelControl assetsPanel;
        private ChartControl pieChart;
        private GridControl gridAssets;
        private GridView viewAssets;
        
        // Summary labels
        private LabelControl lblTotalValue;
        private LabelControl lblTotalValueAmount;
        private LabelControl lblProfitLoss;
        private LabelControl lblProfitLossAmount;

        public PortfolioView()
        {
            var context = new DapperContext();
            _dashboardService = new DashboardService(context);
            
            InitializeComponents();
            LoadPortfolioData();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.Dock = DockStyle.Fill;

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                Padding = new Padding(20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // Summary Panel (Top, Full Width)
            CreateSummaryPanel();
            mainLayout.Controls.Add(summaryPanel, 0, 0);
            mainLayout.SetColumnSpan(summaryPanel, 2);

            // Assets Grid (Bottom Left)
            CreateAssetsGrid();
            mainLayout.Controls.Add(assetsPanel, 0, 1);

            // Pie Chart (Bottom Right)
            CreatePieChart();
            mainLayout.Controls.Add(chartPanel, 1, 1);

            this.Controls.Add(mainLayout);
        }

        private void CreateSummaryPanel()
        {
            summaryPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = Color.FromArgb(30, 30, 30) }
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(20)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // Total Value Card
            var totalCard = CreateSummaryCard("💰 Toplam Portföy Değeri", "₺0.00", Color.FromArgb(33, 150, 243));
            lblTotalValueAmount = totalCard.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(totalCard, 0, 0);

            // Profit/Loss Card
            var profitCard = CreateSummaryCard("📈 Kar/Zarar", "₺0.00", Color.FromArgb(76, 175, 80));
            lblProfitLossAmount = profitCard.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(profitCard, 1, 0);

            // Asset Count Card
            var countCard = CreateSummaryCard("📊 Varlık Sayısı", "0", Color.FromArgb(255, 152, 0));
            layout.Controls.Add(countCard, 2, 0);

            // Performance Card
            var perfCard = CreateSummaryCard("⚡ Performans", "0%", Color.FromArgb(156, 39, 176));
            layout.Controls.Add(perfCard, 3, 0);

            summaryPanel.Controls.Add(layout);
        }

        private PanelControl CreateSummaryCard(string title, string value, Color accentColor)
        {
            var card = new PanelControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = Color.FromArgb(40, 40, 40) },
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            var lblTitle = new LabelControl
            {
                Text = title,
                Location = new Point(15, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 20),
                Appearance = { 
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(200, 200, 200)
                }
            };

            var lblValue = new LabelControl
            {
                Text = value,
                Location = new Point(15, 45),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(250, 40),
                Appearance = { 
                    Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                    ForeColor = accentColor
                }
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);

            return card;
        }

        private void CreateAssetsGrid()
        {
            assetsPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = Color.FromArgb(30, 30, 30) }
            };

            var lblTitle = new LabelControl
            {
                Text = "💼 Varlıklarım",
                Location = new Point(15, 10),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 25),
                Appearance = { 
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White
                }
            };
            assetsPanel.Controls.Add(lblTitle);

            gridAssets = new GridControl
            {
                Location = new Point(10, 45),
                Size = new Size(assetsPanel.Width - 20, assetsPanel.Height - 55),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            viewAssets = new GridView(gridAssets)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false }
            };

            gridAssets.MainView = viewAssets;

            // Columns
            viewAssets.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn 
            { 
                Caption = "Varlık", 
                FieldName = "AssetName",
                Visible = true,
                Width = 120
            });
            viewAssets.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn 
            { 
                Caption = "Kategori", 
                FieldName = "Category",
                Visible = true,
                Width = 80
            });
            viewAssets.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn 
            { 
                Caption = "Miktar", 
                FieldName = "Quantity",
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "{0:N2}" },
                Visible = true,
                Width = 80
            });
            viewAssets.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn 
            { 
                Caption = "Birim Fiyat", 
                FieldName = "UnitPrice",
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺{0:N2}" },
                Visible = true,
                Width = 90
            });
            viewAssets.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn 
            { 
                Caption = "Toplam Değer", 
                FieldName = "TotalValue",
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺{0:N2}" },
                Visible = true,
                Width = 100
            });
            viewAssets.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn 
            { 
                Caption = "Kar/Zarar", 
                FieldName = "ProfitLoss",
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺{0:N2}" },
                Visible = true,
                Width = 90
            });
            viewAssets.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn 
            { 
                Caption = "%", 
                FieldName = "ProfitLossPercent",
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "{0:F1}%" },
                Visible = true,
                Width = 60
            });

            assetsPanel.Controls.Add(gridAssets);
        }

        private void CreatePieChart()
        {
            chartPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = Color.FromArgb(30, 30, 30) }
            };

            var lblTitle = new LabelControl
            {
                Text = "📊 Varlık Dağılımı",
                Location = new Point(15, 10),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 25),
                Appearance = { 
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White
                }
            };
            chartPanel.Controls.Add(lblTitle);

            pieChart = new ChartControl
            {
                Location = new Point(10, 45),
                Size = new Size(chartPanel.Width - 20, chartPanel.Height - 55),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(30, 30, 30),
                AppearanceNameSerializable = "Dark"
            };

            pieChart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            pieChart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Right;
            pieChart.Legend.AlignmentVertical = LegendAlignmentVertical.Center;
            pieChart.Legend.TextColor = Color.White;

            chartPanel.Controls.Add(pieChart);
        }

        private async void LoadPortfolioData()
        {
            try
            {
                var userId = AppEvents.CurrentSession.UserId;
                
                // Get detailed portfolio data
                var portfolioData = await GetDetailedPortfolioAsync(userId);
                
                if (portfolioData.Any())
                {
                    // Update grid with detailed data
                    gridAssets.DataSource = portfolioData;
                    
                    // Update pie chart from detailed data
                    var distribution = portfolioData
                        .GroupBy(x => x.Category)
                        .Select(g => new { Category = g.Key, Amount = g.Sum(x => x.TotalValue) })
                        .ToList();
                    
                    var series = new Series("Varlıklar", ViewType.Doughnut);
                    foreach (var item in distribution)
                    {
                        series.Points.Add(new SeriesPoint(item.Category, (double)item.Amount));
                    }
                    series.Label.TextPattern = "{A}: {VP:P0}";
                    
                    pieChart.Series.Clear();
                    pieChart.Series.Add(series);
                    pieChart.PaletteName = "Pastel";
                    
                    // Update summary cards
                    var totalValue = portfolioData.Sum(x => x.TotalValue);
                    var totalCost = portfolioData.Sum(x => x.TotalCost);
                    var profitLoss = totalValue - totalCost;
                    var profitLossPercent = totalCost > 0 ? (profitLoss / totalCost * 100) : 0;
                    
                    if (lblTotalValueAmount != null)
                        lblTotalValueAmount.Text = $"₺{totalValue:N2}";
                    
                    if (lblProfitLossAmount != null)
                    {
                        var color = profitLoss >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 68, 68);
                        lblProfitLossAmount.Text = $"{(profitLoss >= 0 ? "+" : "")}₺{profitLoss:N2} ({profitLossPercent:F1}%)";
                        lblProfitLossAmount.Appearance.ForeColor = color;
                    }
                }
                else
                {
                    // Show empty state
                    gridAssets.DataSource = null;
                    
                    var series = new Series("Varlıklar", ViewType.Doughnut);
                    series.Points.Add(new SeriesPoint("Veri Yok", 1));
                    pieChart.Series.Clear();
                    pieChart.Series.Add(series);
                    
                    if (lblTotalValueAmount != null)
                        lblTotalValueAmount.Text = "₺0.00";
                    
                    if (lblProfitLossAmount != null)
                        lblProfitLossAmount.Text = "₺0.00";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PortfolioView] LoadPortfolioData error: {ex.Message}");
            }
        }
        
        private async Task<List<PortfolioItemDto>> GetDetailedPortfolioAsync(int userId)
        {
            var items = new List<PortfolioItemDto>();
            
            try
            {
                var context = new DapperContext();
                using var conn = context.CreateConnection();
                
                // Get customer ID
                var customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });
                    
                if (!customerId.HasValue) return items;
                
                // 1. Cash/Bank Accounts
                var accounts = await conn.QueryAsync<dynamic>(@"
                    SELECT 
                        'Nakit' as Category,
                        IBAN as AssetName,
                        Balance as Quantity,
                        Balance as UnitPrice,
                        Balance as TotalValue,
                        Balance as TotalCost,
                        0 as ProfitLoss
                    FROM ""Accounts""
                    WHERE ""CustomerId"" = @CustomerId AND ""Balance"" > 0");
                
                foreach (var acc in accounts)
                {
                    items.Add(new PortfolioItemDto
                    {
                        Category = acc.Category,
                        AssetName = acc.AssetName,
                        Quantity = acc.Quantity,
                        UnitPrice = acc.UnitPrice,
                        TotalValue = acc.TotalValue,
                        TotalCost = acc.TotalCost,
                        ProfitLoss = acc.ProfitLoss,
                        ProfitLossPercent = 0
                    });
                }
                
                // 2. Stocks (if exists)
                try
                {
                    var stocks = await conn.QueryAsync<dynamic>(@"
                        SELECT 
                            'Hisse' as Category,
                            Symbol as AssetName,
                            Quantity,
                            CurrentPrice as UnitPrice,
                            (Quantity * CurrentPrice) as TotalValue,
                            (Quantity * PurchasePrice) as TotalCost,
                            (Quantity * (CurrentPrice - PurchasePrice)) as ProfitLoss
                        FROM ""UserStocks""
                        WHERE ""UserId"" = @UserId AND ""Quantity"" > 0");
                    
                    foreach (var stock in stocks)
                    {
                        var plPercent = stock.TotalCost > 0 ? (stock.ProfitLoss / stock.TotalCost * 100) : 0;
                        items.Add(new PortfolioItemDto
                        {
                            Category = stock.Category,
                            AssetName = stock.AssetName,
                            Quantity = stock.Quantity,
                            UnitPrice = stock.UnitPrice,
                            TotalValue = stock.TotalValue,
                            TotalCost = stock.TotalCost,
                            ProfitLoss = stock.ProfitLoss,
                            ProfitLossPercent = plPercent
                        });
                    }
                }
                catch
                {
                    // Stock table might not exist, skip
                }
                
                // 3. Other assets (placeholder for future)
                // Could add crypto, real estate, etc.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDetailedPortfolioAsync Error: {ex.Message}");
            }
            
            return items;
        }
        
        public class PortfolioItemDto
        {
            public string Category { get; set; }
            public string AssetName { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalValue { get; set; }
            public decimal TotalCost { get; set; }
            public decimal ProfitLoss { get; set; }
            public decimal ProfitLossPercent { get; set; }
        }
    }
}
