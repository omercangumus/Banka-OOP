using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;
using DevExpress.XtraTab;
using BankApp.UI.Controls; // For BESCalculatorControl
using AssetType = BankApp.Infrastructure.Services.AssetType; // Alias for clarity

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Investment Dashboard - Passive portfolio management view
    /// Shows net worth, asset allocation, and retirement planning
    /// </summary>
    public partial class InvestmentDashboardForm : XtraForm
    {
        private readonly PortfolioService _portfolioService;
        
        // UI Components
        // UI Components
        private PanelControl pnlNetWorth;
        private LabelControl lblNetWorthTitle;
        private LabelControl lblNetWorthValue;
        private LabelControl lblProfitLoss;
        private SimpleButton btnRefresh;

        private XtraTabControl tabControl;
        private XtraTabPage tabOverview;
        private XtraTabPage tabPortfolio;
        private XtraTabPage tabBES;


        // Overview Widgets
        private PanelControl pnlTicker;
        private LabelControl lblTickerText;
        private ListBoxControl lstNews;
        private PanelControl pnlQuickActions;
        private ChartControl chartMiniHistory;
        
        // Portfolio Components
        private ChartControl chartAssetAllocation;
        private GridControl gridHoldings;
        private GridView gridViewHoldings;

        public InvestmentDashboardForm()
        {
            _portfolioService = new PortfolioService();
            InitializeComponent();
            _ = LoadDashboardDataAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main Layout
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.Text = "💼 Investment Dashboard - Portföy Yönetimi";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 1. TOP PANEL: Net Worth
            pnlNetWorth = new PanelControl();
            pnlNetWorth.Dock = DockStyle.Top;
            pnlNetWorth.Height = 120;
            pnlNetWorth.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlNetWorth.Appearance.BackColor = Color.FromArgb(30, 41, 59);

            lblNetWorthTitle = new LabelControl { Text = "TOPLAM NET DEĞER", Location = new Point(30, 20) };
            lblNetWorthTitle.Appearance.Font = new Font("Segoe UI", 12F);
            lblNetWorthTitle.Appearance.ForeColor = Color.FromArgb(148, 163, 184);

            lblNetWorthValue = new LabelControl { Text = "₺0", Location = new Point(30, 45), Size = new Size(500, 60), AutoSizeMode = LabelAutoSizeMode.None };
            lblNetWorthValue.Appearance.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            lblNetWorthValue.Appearance.ForeColor = Color.White;

            lblProfitLoss = new LabelControl { Text = "Kar/Zarar: ₺0 (0%)", Location = new Point(600, 60), Size = new Size(400, 30), AutoSizeMode = LabelAutoSizeMode.None };
            lblProfitLoss.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblProfitLoss.Appearance.ForeColor = Color.FromArgb(34, 197, 94);

            btnRefresh = new SimpleButton { Text = "🔄 Yenile", Location = new Point(1250, 40), Size = new Size(120, 40) };
            btnRefresh.Click += async (s, e) => await LoadDashboardDataAsync();

            pnlNetWorth.Controls.AddRange(new Control[] { lblNetWorthTitle, lblNetWorthValue, lblProfitLoss, btnRefresh });

            // 2. TAB CONTROL
            tabControl = new XtraTabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.AppearancePage.Header.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            
            tabOverview = new XtraTabPage { Text = "Genel Bakış" };
            tabPortfolio = new XtraTabPage { Text = "Portföyüm" };
            tabBES = new XtraTabPage { Text = "BES Emeklilik" };
            
            SetupOverviewTab();
            SetupPortfolioTab();
            SetupBESTab();

            tabControl.TabPages.AddRange(new XtraTabPage[] { tabOverview, tabPortfolio, tabBES });

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlNetWorth); // Dock Top added last to be on top? No, Dock order matters.
            
            // Re-ordering controls for Dock to work: Add Top first, then Fill
            this.Controls.Clear();
            this.Controls.Add(tabControl); // Fill
            this.Controls.Add(pnlNetWorth); // Top

            this.ResumeLayout(false);
        }

        private void SetupOverviewTab()
        {
            tabOverview.AutoScroll = true;
            tabOverview.Appearance.Header.BackColor = Color.FromArgb(30, 41, 59);

            // A. Market Ticker
            pnlTicker = new PanelControl();
            pnlTicker.Location = new Point(20, 20);
            pnlTicker.Size = new Size(1340, 50);
            pnlTicker.Appearance.BackColor = Color.Black;
            pnlTicker.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            
            lblTickerText = new LabelControl { Text = "USD/TRY: 35.40 ⬆  |  EUR/TRY: 38.10 ⬆  |  BIST100: 9,200 ⬆  |  BTC/USD: $98,500 🚀  |  ETH/USD: $3,800 ⬆", Dock = DockStyle.Fill };
            lblTickerText.Appearance.Font = new Font("Consolas", 14F, FontStyle.Bold);
            lblTickerText.Appearance.ForeColor = Color.LawnGreen;
            lblTickerText.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            pnlTicker.Controls.Add(lblTickerText);

            // B. Mini Chart (7 Day History) & News
            chartMiniHistory = new ChartControl();
            chartMiniHistory.Location = new Point(20, 90);
            chartMiniHistory.Size = new Size(800, 350);
            chartMiniHistory.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
            
            // C. News Feed
            lstNews = new ListBoxControl();
            lstNews.Location = new Point(840, 90);
            lstNews.Size = new Size(520, 500);
            lstNews.Appearance.BackColor = Color.FromArgb(20, 20, 20);
            lstNews.Appearance.ForeColor = Color.White;
            lstNews.Appearance.Font = new Font("Segoe UI", 10F);

            // D. Quick Actions Grid (4 Buttons)
            pnlQuickActions = new PanelControl();
            pnlQuickActions.Location = new Point(20, 460);
            pnlQuickActions.Size = new Size(800, 130);
            pnlQuickActions.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlQuickActions.Appearance.BackColor = Color.Transparent;
            
            var btnGold = CreateActionButton("Altın Al", Color.FromArgb(234, 179, 8), new Point(0, 0));
            var btnTransfer = CreateActionButton("Para Gönder", Color.FromArgb(168, 85, 247), new Point(210, 0));
            var btnBills = CreateActionButton("Fatura Öde", Color.FromArgb(59, 130, 246), new Point(420, 0));
            var btnQR = CreateActionButton("QR Ödeme", Color.FromArgb(239, 68, 68), new Point(630, 0));
            
            // Wire up Transfer button to open new form
            btnTransfer.Click += (s, e) => { new TransferForm().ShowDialog(); };
            btnGold.Click += (s, e) => XtraMessageBox.Show("Altın Alım ekranı yakında!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            pnlQuickActions.Controls.AddRange(new Control[] { btnGold, btnTransfer, btnBills, btnQR });

            tabOverview.Controls.AddRange(new Control[] { pnlTicker, chartMiniHistory, lstNews, pnlQuickActions });
        }

        private SimpleButton CreateActionButton(string text, Color color, Point loc)
        {
            var btn = new SimpleButton { Text = text, Location = loc, Size = new Size(200, 100) };
            btn.Appearance.BackColor = color;
            btn.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btn.Appearance.ForeColor = Color.White;
            btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            return btn;
        }

        private void SetupPortfolioTab()
        {
            tabPortfolio.Appearance.Header.BackColor = Color.FromArgb(30, 41, 59);

            // Re-use existing components logic
            chartAssetAllocation = new ChartControl();
            chartAssetAllocation.Location = new Point(20, 20);
            chartAssetAllocation.Size = new Size(550, 400);
            chartAssetAllocation.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chartAssetAllocation.Legend.Direction = LegendDirection.LeftToRight;
            chartAssetAllocation.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
            chartAssetAllocation.Legend.AlignmentVertical = LegendAlignmentVertical.Bottom;
            chartAssetAllocation.Titles.Add(new ChartTitle { Text = "VARLIK DAĞILIMI", Font = new Font("Segoe UI", 14F, FontStyle.Bold), TextColor = Color.White });

            gridHoldings = new GridControl();
            gridHoldings.Location = new Point(590, 20);
            gridHoldings.Size = new Size(760, 500);
            gridHoldings.MainView = gridViewHoldings = new GridView();
            
            gridViewHoldings.OptionsView.ShowGroupPanel = false;
            gridViewHoldings.OptionsView.ShowIndicator = false;
            gridViewHoldings.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridViewHoldings.Appearance.HeaderPanel.BackColor = Color.FromArgb(51, 65, 85);
            gridViewHoldings.Appearance.HeaderPanel.ForeColor = Color.White;
            gridViewHoldings.Appearance.Row.BackColor = Color.FromArgb(30, 41, 59);
            gridViewHoldings.Appearance.Row.ForeColor = Color.White;

            tabPortfolio.Controls.Add(chartAssetAllocation);
            tabPortfolio.Controls.Add(gridHoldings);
        }

        private void SetupBESTab()
        {
            tabBES.Appearance.Header.BackColor = Color.FromArgb(30, 41, 59);
            
            // Center the control
            BESCalculatorControl besControl = new BESCalculatorControl();
            besControl.Location = new Point((this.Width - besControl.Width) / 2 - 50, 50);
            besControl.Anchor = AnchorStyles.Top;
            
            tabBES.Controls.Add(besControl);
        }

        /// <summary>
        /// Load all dashboard data asynchronously
        /// </summary>
        /// <summary>
        /// Load all dashboard data asynchronously
        /// </summary>
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                btnRefresh.Text = "⏳ Yükleniyor...";

                // 1. Load Live Market Data
                var liveService = new LiveFinancialService();
                var marketData = await liveService.GetMarketDataAsync();

                // Update Ticker
                if (marketData != null)
                {
                    lblTickerText.Text = $"USD/TRY: {marketData.UsdTry:N2} | EUR/TRY: {marketData.EurTry:N2} | ALTIN (Gr): {marketData.GoldGramTry:N0} TL | BTC: ${marketData.Bitcoin.PriceUSD:N0} ({marketData.Bitcoin.Change24h:+0.00}%) | ETH: ${marketData.Ethereum.PriceUSD:N0} ({marketData.Ethereum.Change24h:+0.00}%)";
                    
                    // Colorize based on general market trend (Mock logic for ticker color, or use BTC change)
                    lblTickerText.Appearance.ForeColor = marketData.Bitcoin.Change24h >= 0 ? Color.LawnGreen : Color.FromArgb(239, 68, 68);
                }

                // Load net worth (TODO: Use live rates for precision calculation in PortfolioService)
                decimal netWorth = await _portfolioService.GetNetWorthAsync();
                lblNetWorthValue.Text = $"₺{netWorth:N0}";

                // Load profit/loss
                var (plAmount, plPercent) = await _portfolioService.GetTotalProfitLossAsync();
                lblProfitLoss.Text = $"Kar/Zarar: ₺{plAmount:N0} ({plPercent:+0.00;-0.00}%)";
                lblProfitLoss.Appearance.ForeColor = plAmount >= 0 
                    ? Color.FromArgb(34, 197, 94)  // Green
                    : Color.FromArgb(239, 68, 68);  // Red

                // Parallel Loading of other widgets
                var t1 = LoadAssetAllocationChartAsync();
                var t2 = LoadHoldingsGridAsync();
                var t3 = LoadNewsAsync();
                var t4 = LoadMiniChartAsync();

                await Task.WhenAll(t1, t2, t3, t4);

                btnRefresh.Text = "✅ Güncellendi";
                await Task.Delay(1000);
                btnRefresh.Text = "🔄 Yenile";
                btnRefresh.Enabled = true;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Veri yüklenirken hata: {ex.Message}", "Hata", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnRefresh.Enabled = true;
                btnRefresh.Text = "🔄 Yenile";
            }
        }

        private async Task LoadNewsAsync()
        {
            // Load news from Finnhub (mock)
            var finnhub = new FinnhubService();
            var news = await finnhub.GetMarketNewsAsync();
            
            lstNews.Items.Clear();
            foreach (var item in news)
            {
                lstNews.Items.Add($"📰 {item.Source} - {item.Headline}");
            }
        }

        private async Task LoadMiniChartAsync()
        {
            // Simple Line Chart for History (Mock)
            chartMiniHistory.Series.Clear();
            Series series = new Series("7 Day History", ViewType.Line);
            
            var r = new Random();
            double balance = (double)await _portfolioService.GetNetWorthAsync() * 0.95; // Start lower
            
            for(int i=6; i>=0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                series.Points.Add(new SeriesPoint(date.ToString("dd MMM"), balance));
                balance *= (1 + (r.NextDouble() * 0.04 - 0.01)); // Random daily change
            }
            
            chartMiniHistory.Series.Add(series);
            
            if (chartMiniHistory.Diagram is XYDiagram diag)
            {
                diag.DefaultPane.BackColor = Color.FromArgb(30, 41, 59);
                diag.AxisX.Label.TextColor = Color.White;
                diag.AxisY.Label.TextColor = Color.White;
                diag.AxisX.GridLines.Visible = false;
                diag.AxisY.GridLines.Color = Color.FromArgb(60, 60, 60);
            }
        }

        /// <summary>
        /// Load asset allocation pie chart
        /// </summary>
        private async Task LoadAssetAllocationChartAsync()
        {
            chartAssetAllocation.Series.Clear();

            var allocation = await _portfolioService.GetAssetAllocationAsync();
            
            Series series = new Series("Asset Allocation", ViewType.Doughnut);
            
            // Color scheme for different asset types
            var colorMap = new Dictionary<AssetType, Color>
            {
                { AssetType.Stock, Color.FromArgb(59, 130, 246) },      // Blue
                { AssetType.Crypto, Color.FromArgb(249, 115, 22) },     // Orange
                { AssetType.Gold, Color.FromArgb(251, 191, 36) },       // Amber/Gold
                { AssetType.Forex, Color.FromArgb(34, 197, 94) },       // Green
                { AssetType.Cash, Color.FromArgb(148, 163, 184) },      // Gray
                { AssetType.Pension, Color.FromArgb(168, 85, 247) },    // Purple
                { AssetType.Index, Color.FromArgb(14, 165, 233) }       // Sky Blue
            };

            foreach (var item in allocation.OrderByDescending(x => x.Value))
            {
                string label = GetAssetTypeLabel(item.Key);
                SeriesPoint point = new SeriesPoint(label, item.Value);
                
                if (colorMap.ContainsKey(item.Key))
                {
                    point.Color = colorMap[item.Key];
                }
                
                series.Points.Add(point);
            }

            if (series.View is DoughnutSeriesView doughnutView)
            {
                doughnutView.RuntimeExploding = true;
                doughnutView.Border.Visibility = DevExpress.Utils.DefaultBoolean.False;
                doughnutView.HoleRadiusPercent = 60; // Make center hole bigger for text
            }

            series.Label.TextPattern = "{A}: {VP:P1}";
            series.LegendTextPattern = "{A}: {VP:P1}";

            chartAssetAllocation.Series.Add(series);
        }

        /// <summary>
        /// Load holdings grid with portfolio data
        /// </summary>
        private async Task LoadHoldingsGridAsync()
        {
            var holdings = await _portfolioService.GetEnrichedHoldingsAsync();

            var gridData = holdings.Select(h => new
            {
                Varlık = h.AssetName,
                Tip = GetAssetTypeLabel(h.AssetType),
                Miktar = h.Quantity,
                Maliyet = h.AverageCost,
                Güncel = h.CurrentPrice,
                Toplam = h.CurrentValue,
                KarZarar = h.ProfitLoss,
                Yüzde = h.ProfitLossPercent
            }).ToList();

            gridHoldings.DataSource = gridData;

            // Configure columns
            gridViewHoldings.Columns["Varlık"].Width = 150;
            gridViewHoldings.Columns["Tip"].Width = 80;
            gridViewHoldings.Columns["Miktar"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridViewHoldings.Columns["Miktar"].DisplayFormat.FormatString = "N2";
            gridViewHoldings.Columns["Maliyet"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            gridViewHoldings.Columns["Maliyet"].DisplayFormat.FormatString = "₺{0:N2}";
            gridViewHoldings.Columns["Güncel"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            gridViewHoldings.Columns["Güncel"].DisplayFormat.FormatString = "₺{0:N2}";
            gridViewHoldings.Columns["Toplam"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            gridViewHoldings.Columns["Toplam"].DisplayFormat.FormatString = "₺{0:N0}";
            gridViewHoldings.Columns["KarZarar"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            gridViewHoldings.Columns["KarZarar"].DisplayFormat.FormatString = "₺{0:N0}";
            gridViewHoldings.Columns["Yüzde"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            gridViewHoldings.Columns["Yüzde"].DisplayFormat.FormatString = "{0:+0.00;-0.00}%";

            // Color-code profit/loss rows
            gridViewHoldings.RowStyle += (s, e) =>
            {
                if (e.RowHandle >= 0)
                {
                    var pl = (decimal)gridViewHoldings.GetRowCellValue(e.RowHandle, "KarZarar");
                    if (pl > 0)
                        e.Appearance.ForeColor = Color.FromArgb(34, 197, 94); // Green
                    else if (pl < 0)
                        e.Appearance.ForeColor = Color.FromArgb(239, 68, 68); // Red
                }
            };

            gridViewHoldings.BestFitColumns();
        }



        /// <summary>
        /// Get Turkish label for asset type
        /// </summary>
        private string GetAssetTypeLabel(AssetType type)
        {
            return type switch
            {
                AssetType.Stock => "Hisse",
                AssetType.Index => "Endeks",
                AssetType.Crypto => "Kripto",
                AssetType.Gold => "Altın",
                AssetType.Forex => "Döviz",
                AssetType.Cash => "Nakit",
                AssetType.Pension => "BES",
                _ => type.ToString()
            };
        }
    }
}
