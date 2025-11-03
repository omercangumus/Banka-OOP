using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.Utils;
using DevExpress.LookAndFeel;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Controls
{
    public class InvestmentDashboard : XtraUserControl
    {
        private PanelControl pnlMain;
        private LabelControl lblTitle;
        private CheckButton btnProMode;
        private LabelControl lblSummary;
        private SimpleButton btnRefresh;
        private ChartControl chartStocks, chartGold, chartEuro, chartOil;
        private GroupControl grpNews;
        private LabelControl lblNewsText;
        private SimpleButton btnBack;
        
        private ChartControl expandedChart = null;
        private Point[] originalLocations = new Point[4];
        private Size[] originalSizes = new Size[4];
        
        // New Components for "Full" bottom section
        private DevExpress.XtraGrid.GridControl gridMarketData;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewMarketData;
        private GroupControl grpCalendar;
        private LabelControl lblCalendarData;

        private readonly FinnhubService _finnhubService;

        public InvestmentDashboard()
        {
            _finnhubService = new FinnhubService();
            InitializeUI();
            _ = LoadRealData();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            // Professional Dark Theme Fix
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            this.Appearance.BackColor = Color.FromArgb(15, 23, 42); 
            this.Appearance.Options.UseBackColor = true;

            pnlMain = new PanelControl();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMain.Appearance.BackColor = Color.Transparent;
            pnlMain.Appearance.Options.UseBackColor = true;
            this.Controls.Add(pnlMain);

            // Header
            lblTitle = new LabelControl();
            lblTitle.Text = "YATIRIM & EMTİA PİYASALARI";
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 16F);
            lblTitle.Appearance.ForeColor = Color.FromArgb(226, 232, 240);
            lblTitle.Location = new System.Drawing.Point(20, 10);
            pnlMain.Controls.Add(lblTitle);
            
            btnProMode = new CheckButton();
            btnProMode.Text = "PROFESYONEL MOD";
            btnProMode.Location = new System.Drawing.Point(880, 10);
            btnProMode.Size = new Size(150, 40);
            btnProMode.CheckedChanged += BtnProMode_CheckedChanged;
            pnlMain.Controls.Add(btnProMode);
            
            btnRefresh = new SimpleButton();
            btnRefresh.Text = "🔄 YENİLE";
            btnRefresh.Location = new System.Drawing.Point(1050, 10);
            btnRefresh.Size = new Size(150, 40);
            btnRefresh.Appearance.BackColor = Color.FromArgb(16, 185, 129);
            btnRefresh.Appearance.ForeColor = Color.White;
            btnRefresh.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.Click += BtnRefresh_Click;
            pnlMain.Controls.Add(btnRefresh);
            
            lblSummary = new LabelControl();
            lblSummary.Text = "Piyasa verileri yükleniyor...";
            lblSummary.Appearance.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblSummary.Appearance.ForeColor = Color.FromArgb(16, 185, 129);
            lblSummary.Location = new System.Drawing.Point(20, 50);
            pnlMain.Controls.Add(lblSummary);

            // Charts
            chartStocks = CreateChart("BIST 100", new System.Drawing.Point(20, 80));
            pnlMain.Controls.Add(chartStocks);

            chartGold = CreateChart("ALTIN (ONS)", new System.Drawing.Point(620, 80));
            pnlMain.Controls.Add(chartGold);

            chartEuro = CreateChart("EURO/TL", new System.Drawing.Point(20, 290));
            pnlMain.Controls.Add(chartEuro);

            chartOil = CreateChart("BRENT PETROL", new System.Drawing.Point(620, 290));
            pnlMain.Controls.Add(chartOil);
            
            // Store original positions
            originalLocations[0] = chartStocks.Location;
            originalLocations[1] = chartGold.Location;
            originalLocations[2] = chartEuro.Location;
            originalLocations[3] = chartOil.Location;
            
            originalSizes[0] = chartStocks.Size;
            originalSizes[1] = chartGold.Size;
            originalSizes[2] = chartEuro.Size;
            originalSizes[3] = chartOil.Size;
            
            // Events
            chartStocks.Click += (s, e) => ExpandChart(chartStocks);
            chartGold.Click += (s, e) => ExpandChart(chartGold);
            chartEuro.Click += (s, e) => ExpandChart(chartEuro);
            chartOil.Click += (s, e) => ExpandChart(chartOil);
            
            // Back button
            btnBack = new SimpleButton();
            btnBack.Text = "◀ GERİ DÖN";
            btnBack.Location = new System.Drawing.Point(1050, 50);
            btnBack.Size = new Size(150, 40);
            btnBack.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            btnBack.Appearance.ForeColor = Color.White;
            btnBack.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnBack.Click += BtnBack_Click;
            btnBack.Visible = false;
            pnlMain.Controls.Add(btnBack);
            btnBack.BringToFront();

            // News
            grpNews = new GroupControl();
            grpNews.Text = "Piyasa Haberleri & Analizler";
            grpNews.Location = new System.Drawing.Point(20, 510);
            grpNews.Size = new Size(1180, 130); 
            grpNews.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            
            lblNewsText = new LabelControl();
            lblNewsText.Dock = DockStyle.Fill;
            lblNewsText.AutoSizeMode = LabelAutoSizeMode.None;
            lblNewsText.Padding = new Padding(10);
            lblNewsText.Appearance.Font = new Font("Segoe UI", 9F);
            lblNewsText.Appearance.ForeColor = Color.LightGray;
            lblNewsText.Text = "• Piyasalar güne pozitif başladı.\n• BIST 100 endeksi rekor tazeledi.\n• Altın fiyatlarında yükseliş ivmesi korunuyor.\n• Merkez Bankası faiz kararını açıkladı.";
            
            grpNews.Controls.Add(lblNewsText);
            pnlMain.Controls.Add(grpNews);

            // ============================================
            // NEW: Fill the Bottom Space (Localized & Enhanced)
            // ============================================
            
            // 1. Canlı Piyasa (Live Market Watch)
            gridMarketData = new DevExpress.XtraGrid.GridControl();
            gridMarketData.Location = new Point(20, 650);
            gridMarketData.Size = new Size(750, 200);
            gridMarketData.LookAndFeel.UseDefaultLookAndFeel = false;
            gridMarketData.LookAndFeel.SkinName = "Office 2019 Black";
            
            gridViewMarketData = new DevExpress.XtraGrid.Views.Grid.GridView();
            gridMarketData.MainView = gridViewMarketData;
            gridViewMarketData.OptionsView.ShowGroupPanel = false;
            gridViewMarketData.OptionsView.ShowIndicator = false;
            gridViewMarketData.Appearance.HeaderPanel.BackColor = Color.FromArgb(30, 41, 59);
            gridViewMarketData.Appearance.HeaderPanel.ForeColor = Color.White;
            gridViewMarketData.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gridViewMarketData.Appearance.Row.BackColor = Color.FromArgb(15, 23, 42);
            gridViewMarketData.Appearance.Row.ForeColor = Color.White;
            gridViewMarketData.Appearance.Row.Font = new Font("Segoe UI", 9F);
            
            // Populate Dummy Data for Grid
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Sembol");
            dt.Columns.Add("Varlık Adı");
            dt.Columns.Add("Son Fiyat");
            dt.Columns.Add("Değişim %");
            
            dt.Rows.Add("BTC-USD", "Bitcoin", "$45,230.50", "+2.5%");
            dt.Rows.Add("ETH-USD", "Ethereum", "$2,340.10", "+1.2%");
            dt.Rows.Add("XRP-USD", "Ripple", "$0.55", "-0.8%");
            dt.Rows.Add("SOL-USD", "Solana", "$98.40", "+5.4%");
            dt.Rows.Add("ADA-USD", "Cardano", "$0.52", "-1.1%");
            dt.Rows.Add("DOGE-USD", "Dogecoin", "$0.08", "+0.5%");
            dt.Rows.Add("AVAX-USD", "Avalanche", "$35.20", "+3.1%");
            dt.Rows.Add("LINK-USD", "Chainlink", "$14.50", "-0.2%");
            
            gridMarketData.DataSource = dt;
            pnlMain.Controls.Add(gridMarketData);
            
            // 2. Bekleyen Emirlerim (My Pending Orders)
            grpCalendar = new GroupControl();
            grpCalendar.Text = "📋 BEKLEYEN EMİRLERİM";
            grpCalendar.Location = new Point(790, 650);
            grpCalendar.Size = new Size(410, 200);
            grpCalendar.AppearanceCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            
            lblCalendarData = new LabelControl();
            lblCalendarData.Dock = DockStyle.Fill;
            lblCalendarData.AutoSizeMode = LabelAutoSizeMode.None;
            lblCalendarData.Padding = new Padding(10);
            lblCalendarData.Appearance.Font = new Font("Consolas", 10F);
            lblCalendarData.Appearance.ForeColor = Color.White; // Make it pop
            lblCalendarData.Text = "• AL  : THYAO  @ 275.50 TL [100 LOT]\n" +
                                   "• SAT : AKBNK  @ 42.10 TL  [500 LOT]\n" +
                                   "• AL  : BTC    @ $42,000   [0.05 BTC]\n" +
                                   "• SAT : GLD    @ 2,050 TL  [10 GR]\n" +
                                   "------------------------------------\n" +
                                   "Toplam Bekleyen Hacim: 145,250 TL";
                                   
            grpCalendar.Controls.Add(lblCalendarData);
            pnlMain.Controls.Add(grpCalendar); // Add to panel
        }
        
        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            btnRefresh.Enabled = false;
            btnRefresh.Text = "🔄 Loading...";
            await LoadRealData();
            btnRefresh.Enabled = true;
            btnRefresh.Text = "🔄 REFRESH";
        }

        private ChartControl CreateChart(string title, System.Drawing.Point loc)
        {
            ChartControl chart = new ChartControl();
            chart.Location = loc;
            chart.Size = new Size(580, 200);
            chart.BorderOptions.Visibility = DefaultBoolean.False;
            chart.Legend.Visibility = DefaultBoolean.False;
            chart.BackColor = Color.FromArgb(30, 41, 59);  // Slightly lighter than background
            
            // Fix: Do not assign chart.Diagram directly. 
            // Add a dummy series to force XYDiagram creation, then configure it.
            Series dummy = new Series("Dummy", ViewType.Line);
            chart.Series.Add(dummy);

            if (chart.Diagram is XYDiagram diag)
            {
                diag.AxisX.Label.Visible = false;
                diag.AxisY.Label.Font = new Font("Tahoma", 7);
                diag.AxisY.Label.TextColor = Color.FromArgb(148, 163, 184);  // #94a3b8
                diag.DefaultPane.BackColor = Color.FromArgb(30, 41, 59);  // #1e293b
                diag.DefaultPane.BorderVisible = false;
                
                diag.AxisX.GridLines.Visible = false;
                diag.AxisY.GridLines.Visible = false; 
            }
            
            chart.Series.Clear(); // Remove dummy
            
            chart.Titles.Add(new ChartTitle {
                Text = title,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                TextColor = Color.FromArgb(226, 232, 240)  // #e2e8f0
            });
            
            return chart;
        }

        private void BtnProMode_CheckedChanged(object sender, EventArgs e)
        {
             bool pro = btnProMode.Checked;
             UpdateChartPro(chartStocks, pro);
             UpdateChartPro(chartGold, pro);
             UpdateChartPro(chartEuro, pro);
             UpdateChartPro(chartOil, pro);
        }
        
        private void UpdateChartPro(ChartControl chart, bool enabled)
        {
            if(chart.Diagram is XYDiagram diag)
            {
                diag.AxisY.GridLines.Visible = enabled;
                diag.AxisX.GridLines.Visible = enabled;
                diag.AxisY.GridLines.Color = Color.Gray;
                diag.AxisX.GridLines.Color = Color.Gray;
                
                chart.CrosshairEnabled = enabled ? DefaultBoolean.True : DefaultBoolean.False;
                chart.CrosshairOptions.ShowArgumentLine = enabled;
                chart.CrosshairOptions.ShowValueLine = enabled;
            }
        }

        public async Task LoadRealData()
        {
            try 
            {
               // Load real market data
                var appleQuote = await _finnhubService.GetQuoteAsync("AAPL");
                var goldQuote = await _finnhubService.GetQuoteAsync("GC=F");  // Gold futures
                var eurQuote = await _finnhubService.GetQuoteAsync("EURUSD=X");
                
                // Update summary with real data
                lblSummary.Text = $"AAPL: ${appleQuote.C:N2} | GOLD: ${goldQuote.C:N2} | EUR/USD: {eurQuote.C:N4}";
                
                // Update charts with real candle data
                await FillChartWithRealData(chartStocks, "AAPL", Color.FromArgb(59, 130, 246));  // Blue
                await FillChartWithRealData(chartGold, "GC=F", Color.FromArgb(250, 204, 21));    // Gold
                await FillChartWithRealData(chartEuro, "EURUSD=X", Color.FromArgb(34, 197, 94)); // Green
                await FillChartWithRealData(chartOil, "CL=F", Color.FromArgb(239, 68, 68));      // Red
                
                // Load news
                var news = await _finnhubService.GetMarketNewsAsync();
                if (news != null && news.Count > 0)
                {
                    var newsText = string.Join("\n\n", news.Take(4).Select(n => 
                        $"• {n.Headline}\n  {n.Source} - {DateTimeOffset.FromUnixTimeSeconds(n.Datetime):MMM dd, HH:mm}"));
                    lblNewsText.Text = newsText;
                }
            } 
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRealData Error: {ex.Message}");
                lblSummary.Text = "Error loading data. Using cached/simulated data.";
                LoadDummyData();  // Fallback
            }
        }
        
        private async Task FillChartWithRealData(ChartControl chart, string symbol, Color color)
        {
            try
            {
                var candles = await _finnhubService.GetCandlesAsync(symbol, "D", 30);
                
                chart.Series.Clear();
                Series s = new Series(symbol, ViewType.Line);
                
                if (candles != null && candles.C != null && candles.C.Count > 0)
                {
                    for (int i = 0; i < candles.C.Count; i++)
                    {
                        s.Points.Add(new SeriesPoint(i, candles.C[i]));
                    }
                }
                else
                {
                    // Fallback
                    var r = new Random(symbol.GetHashCode());
                    double p = 100;
                    for(int i=0; i<30; i++) 
                    {
                        p += (r.NextDouble() * 4) - 2;
                        s.Points.Add(new SeriesPoint(i, p));
                    }
                }
                
                chart.Series.Add(s);
                s.View.Color = color;
                ((LineSeriesView)s.View).LineStyle.Thickness = 2;
                if(chart.Diagram is XYDiagram d) d.AxisY.WholeRange.AlwaysShowZeroLevel = false;
            }
            catch
            {
                FillChart(chart, symbol, color, 100, 2);  // Fallback to dummy
            }
        }

        public void LoadDummyData()
        {
            try {
                FillChart(chartStocks, "BIST", Color.FromArgb(33, 150, 243), 9000, 150);
                FillChart(chartGold, "GOLD", Color.Gold, 2150, 20);
                FillChart(chartEuro, "EUR", Color.LightBlue, 35.8, 0.5);
                FillChart(chartOil, "OIL", Color.OrangeRed, 85, 2);
            } catch {}
        }
        
        private void FillChart(ChartControl chart, string name, Color color, double start, double vol)
        {
            chart.Series.Clear();
            Series s = new Series(name, ViewType.Line);
            var r = new Random(name.GetHashCode()); 
            double p = start;
            for(int i=0; i<40; i++) {
                p += (r.NextDouble() * vol * 2) - vol;
                s.Points.Add(new SeriesPoint(i, p));
            }
            chart.Series.Add(s);
            s.View.Color = color;
            ((LineSeriesView)s.View).LineStyle.Thickness = 2;
            if(chart.Diagram is XYDiagram d) d.AxisY.WholeRange.AlwaysShowZeroLevel = false;
        }
        
        private void ExpandChart(ChartControl chart)
        {
            if (expandedChart != null) return; // Already expanded
            
            expandedChart = chart;
            
            // Hide other charts and news
            if (chart != chartStocks) chartStocks.Visible = false;
            if (chart != chartGold) chartGold.Visible = false;
            if (chart != chartEuro) chartEuro.Visible = false;
            if (chart != chartOil) chartOil.Visible = false;
            grpNews.Visible = false;
            
            // Expand the selected chart
            chart.Location = new System.Drawing.Point(20, 80);
            chart.Size = new Size(1180, 560);
            chart.BringToFront();
            
            // Show back button
            btnBack.Visible = true;
            btnBack.BringToFront();
        }
        
        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (expandedChart == null) return;
            
            // Restore all charts to original positions
            chartStocks.Location = originalLocations[0];
            chartStocks.Size = originalSizes[0];
            chartStocks.Visible = true;
            
            chartGold.Location = originalLocations[1];
            chartGold.Size = originalSizes[1];
            chartGold.Visible = true;
            
            chartEuro.Location = originalLocations[2];
            chartEuro.Size = originalSizes[2];
            chartEuro.Visible = true;
            
            chartOil.Location = originalLocations[3];
            chartOil.Size = originalSizes[3];
            chartOil.Visible = true;
            
            grpNews.Visible = true;
            
            // Hide back button
            btnBack.Visible = false;
            
            expandedChart = null;
        }
    }
}
