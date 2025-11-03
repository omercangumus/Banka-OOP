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
        // private PanelControl pnlQuickActions; // Removed to make room for charts
        private ChartControl chartStock; // Renamed from MiniHistory
        private ChartControl chartCrypto; // New Crypto Chart
        
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

            // B. Stock Chart (Top Left)
            chartStock = new ChartControl();
            chartStock.Location = new Point(20, 90);
            chartStock.Size = new Size(800, 300);
            chartStock.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
            
            // C. Crypto Chart (Bottom Left)
            chartCrypto = new ChartControl();
            chartCrypto.Location = new Point(20, 410);
            chartCrypto.Size = new Size(800, 300);
            chartCrypto.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;

            // D. News Feed (Right Side - Taller)
            lstNews = new ListBoxControl();
            lstNews.Location = new Point(840, 90);
            lstNews.Size = new Size(520, 620);
            lstNews.Appearance.BackColor = Color.FromArgb(20, 20, 20);
            lstNews.Appearance.ForeColor = Color.White;
            lstNews.Appearance.Font = new Font("Segoe UI", 10F);

            // Add Controls
            tabOverview.Controls.AddRange(new Control[] { pnlTicker, chartStock, chartCrypto, lstNews });
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
            
            BESCalculatorControl besControl = new BESCalculatorControl();
            // Dynamic centering
            besControl.Location = new Point((tabBES.Width - besControl.Width) / 2, 50);
            besControl.Anchor = AnchorStyles.Top;
            
            tabBES.Controls.Add(besControl);
            
            // Handle resizing to keep it centered
            this.Resize += (s, e) => {
                besControl.Location = new Point((tabBES.Width - besControl.Width) / 2, 50);
            };
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
                var t4 = LoadStockChartAsync();
                var t5 = LoadCryptoChartAsync();

                await Task.WhenAll(t1, t2, t3, t4, t5);

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

        private async Task LoadStockChartAsync()
        {
            try
            {
                var finnhub = new FinnhubService();
                var candles = await finnhub.GetCandlesAsync("AAPL", "D", 60); // 60 Days
                var data = CandlestickData.FromFinnhubCandles(candles);

                if (chartStock.InvokeRequired)
                {
                    await Task.Run(() => ConfigureChart(chartStock, "AAPL Stock (Daily)", data, Color.FromArgb(59, 130, 246)));
                }
                else
                {
                    ConfigureChart(chartStock, "AAPL Stock (Daily)", data, Color.FromArgb(59, 130, 246));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading stock chart: {ex.Message}");
            }
        }

        private async Task LoadCryptoChartAsync()
        {
            try
            {
                var finnhub = new FinnhubService();
                var candles = await finnhub.GetCandlesAsync("BINANCE:BTCUSDT", "D", 60); // Crypto requires specific exchange prefix usually, or simple symbol
                if (candles == null || candles.S == "no_data")
                    candles = await finnhub.GetCandlesAsync("COINBASE:BTC-USD", "D", 60); // Try another

                // Fallback to stock-like behavior if crypto fails specific call (Finnhub often treats crypto as normal symbols like 'BTC-USD')
                if (candles == null || candles.S == "no_data")
                    candles = await finnhub.GetCandlesAsync("BTC-USD", "D", 60);

                var data = CandlestickData.FromFinnhubCandles(candles);

                if (chartCrypto.InvokeRequired)
                {
                    await Task.Run(() => ConfigureChart(chartCrypto, "Bitcoin (BTC-USD)", data, Color.FromArgb(249, 115, 22)));
                }
                else
                {
                    ConfigureChart(chartCrypto, "Bitcoin (BTC-USD)", data, Color.FromArgb(249, 115, 22));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading crypto chart: {ex.Message}");
            }
        }

        private void ConfigureChart(ChartControl chart, string title, CandlestickData[] data, Color color)
        {
            chart.Series.Clear();
            chart.Titles.Clear();
            
            // Ensure UI update on main thread
            if (chart.InvokeRequired)
            {
                chart.Invoke(new Action(() => ConfigureChart(chart, title, data, color)));
                return;
            }

            Series series = new Series(title, ViewType.CandleStick);
            
            foreach(var item in data)
            {
                // Candlestick point: Low, High, Open, Close
                // DevExpress expects specific order logic often or just object binding.
                // For FinancialSeries, points are strictly ordered: Low, High, Open, Close.
                
                // Using SeriesPoint with 4 values: Low, High, Open, Close
                var point = new SeriesPoint(item.Time, item.Low, item.High, item.Open, item.Close);
                series.Points.Add(point);
            }

            chart.Series.Add(series);
            
            // Aesthetic Styling
            if (chart.Diagram is XYDiagram diag)
            {
                diag.DefaultPane.BackColor = Color.FromArgb(30, 41, 59);
                diag.AxisX.Label.TextColor = Color.White;
                diag.AxisY.Label.TextColor = Color.White;
                diag.AxisX.GridLines.Visible = false;
                diag.AxisY.GridLines.Color = Color.FromArgb(60, 60, 60);
                diag.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Day;
                diag.AxisX.DateTimeScaleOptions.GridAlignment = DateTimeGridAlignment.Week;
                
                // Set Candle Colors
                CandleStickSeriesView view = (CandleStickSeriesView)series.View;
                view.Color = color; // Up Color
                view.ReductionOptions.Color = Color.Red; // Down Color
                view.ReductionOptions.Visible = true;
                view.LineThickness = 2;
                view.LevelLineLength = 0.25;
            }

            chart.Titles.Add(new ChartTitle() { 
                Text = title, 
                TextColor = Color.White, 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Alignment = StringAlignment.Near
            });
            
            chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chart.BackColor = Color.Transparent;
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
