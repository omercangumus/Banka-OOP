using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using BankApp.Infrastructure.Services;
using BankApp.UI.Forms;

namespace BankApp.UI.Controls
{
    public class InstrumentDetailView : XtraUserControl
    {
        public event EventHandler BackRequested;
        public event EventHandler<string> TradeTerminalRequested;
        
        private string _currentSymbol;
        private string _currentTimeframe = "1D";
        
        private PanelControl pnlHeader;
        private PanelControl pnlChart;
        private PanelControl pnlRight;
        private PanelControl pnlBottom;
        
        private SimpleButton btnBack;
        private LabelControl lblSymbol;
        private SimpleButton btnTf1m, btnTf5m, btnTf15m, btnTf1H, btnTf4H, btnTf1D, btnTf1W;
        private SimpleButton btnRefresh, btnAnalysis;
        private CheckEdit chkMA20, chkEMA12, chkBollinger;
        
        // Order Panel
        private ComboBoxEdit cmbOrderType;
        private TextEdit txtPrice, txtAmount, txtTotal;
        private SimpleButton btnPct25, btnPct50, btnPct75, btnPct100;
        private SimpleButton btnBuy, btnSell;
        private LabelControl lblCurrentPrice, lblFee, lblAvailBalance;
        
        // Bottom tabs
        private XtraTabControl tabBottom;
        private GridControl gridOrders, gridHistory;
        
        private ChartControl chartMain;
        private LabelControl lblChartLoading;
        private bool _isLoadingChart = false;
        private bool _showMA20, _showEMA12, _showBollinger;
        
        private readonly IMarketDataProvider _dataProvider;
        
        public InstrumentDetailView(IMarketDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            InitializeComponents();
        }
        
        public void LoadSymbol(string symbol)
        {
            _currentSymbol = symbol;
            lblSymbol.Text = symbol;
            UpdatePriceDisplay();
            LoadChartAsync();
        }
        
        private async void UpdatePriceDisplay()
        {
            try
            {
                if (_dataProvider != null && !string.IsNullOrEmpty(_currentSymbol))
                {
                    var quote = await _dataProvider.GetQuoteAsync(_currentSymbol);
                    if (quote != null)
                    {
                        lblCurrentPrice.Text = "$" + quote.Current.ToString("N2");
                        lblCurrentPrice.ForeColor = quote.Change >= 0 ? Color.FromArgb(38, 166, 91) : Color.FromArgb(232, 65, 66);
                    }
                }
            }
            catch { lblCurrentPrice.Text = "--"; }
        }
        
        private async void LoadChartAsync()
        {
            if (_isLoadingChart || string.IsNullOrEmpty(_currentSymbol)) return;
            _isLoadingChart = true;
            lblChartLoading.Visible = true;
            lblChartLoading.BringToFront();
            
            try
            {
                var tfMap = _currentTimeframe switch { "1m" => "1", "5m" => "5", "15m" => "15", "1H" => "60", "4H" => "240", "1D" => "D", "1W" => "W", _ => "D" };
                var candles = await _dataProvider.GetCandlesAsync(_currentSymbol, tfMap, 150);
                if (this.InvokeRequired) this.BeginInvoke(new Action(() => RenderChart(candles)));
                else RenderChart(candles);
            }
            catch (Exception ex)
            {
                if (this.InvokeRequired) this.BeginInvoke(new Action(() => ShowChartError(ex.Message)));
                else ShowChartError(ex.Message);
            }
            finally
            {
                _isLoadingChart = false;
                if (this.InvokeRequired) this.BeginInvoke(new Action(() => lblChartLoading.Visible = false));
                else lblChartLoading.Visible = false;
            }
        }
        
        private void RenderChart(List<MarketCandle> candles)
        {
            chartMain.Series.Clear();
            if (candles == null || candles.Count == 0) { ShowChartError("Veri yok"); return; }
            
            var series = new Series(_currentSymbol, ViewType.CandleStick);
            series.ArgumentScaleType = ScaleType.DateTime;
            foreach (var c in candles) series.Points.Add(new SeriesPoint(c.Time, c.Low, c.High, c.Open, c.Close));
            
            var view = (CandleStickSeriesView)series.View;
            view.Color = Color.FromArgb(38, 166, 91);
            view.ReductionOptions.Color = Color.FromArgb(232, 65, 66);
            view.ReductionOptions.Level = StockLevel.Close;
            view.ReductionOptions.Visible = true;
            view.LineThickness = 2;
            view.LevelLineLength = 0.6;
            
            chartMain.Series.Add(series);
            
            if (_showMA20) AddMA(candles, 20, Color.FromArgb(255, 193, 7));
            if (_showEMA12) AddEMA(candles, 12, Color.FromArgb(0, 188, 212));
            if (_showBollinger) AddBollingerBands(candles, 20, 2);
            
            var diagram = chartMain.Diagram as XYDiagram;
            if (diagram != null)
            {
                diagram.EnableAxisXZooming = true;
                diagram.EnableAxisYZooming = true;
                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisYScrolling = true;
                diagram.ZoomingOptions.UseMouseWheel = true;
                
                diagram.AxisX.Label.TextPattern = "{A:MM/dd}";
                diagram.AxisX.Color = Color.FromArgb(40, 40, 40);
                diagram.AxisY.Color = Color.FromArgb(40, 40, 40);
                diagram.AxisX.GridLines.Color = Color.FromArgb(30, 30, 30);
                diagram.AxisY.GridLines.Color = Color.FromArgb(30, 30, 30);
                diagram.AxisX.GridLines.Visible = true;
                diagram.AxisY.GridLines.Visible = true;
                diagram.AxisX.Label.Font = new Font("Segoe UI", 8F);
                diagram.AxisY.Label.Font = new Font("Segoe UI", 8F);
                diagram.AxisX.Label.TextColor = Color.FromArgb(130, 130, 130);
                diagram.AxisY.Label.TextColor = Color.FromArgb(130, 130, 130);
                diagram.DefaultPane.BackColor = Color.FromArgb(14, 14, 14);
            }
            
            chartMain.CrosshairEnabled = DevExpress.Utils.DefaultBoolean.True;
            chartMain.CrosshairOptions.ShowArgumentLabels = true;
            chartMain.CrosshairOptions.ShowValueLabels = true;
            chartMain.CrosshairOptions.ArgumentLineColor = Color.FromArgb(80, 80, 80);
            chartMain.CrosshairOptions.ValueLineColor = Color.FromArgb(80, 80, 80);
            chartMain.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chartMain.Visible = true;
            chartMain.BringToFront();
        }
        
        private void AddMA(List<MarketCandle> candles, int period, Color color)
        {
            if (candles.Count < period) return;
            var s = new Series("MA" + period, ViewType.Line);
            s.ArgumentScaleType = ScaleType.DateTime;
            for (int i = period - 1; i < candles.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < period; j++) sum += candles[i - j].Close;
                s.Points.Add(new SeriesPoint(candles[i].Time, sum / period));
            }
            ((LineSeriesView)s.View).Color = color;
            ((LineSeriesView)s.View).LineStyle.Thickness = 2;
            chartMain.Series.Add(s);
        }
        
        private void AddEMA(List<MarketCandle> candles, int period, Color color)
        {
            if (candles.Count < period) return;
            var s = new Series("EMA" + period, ViewType.Line);
            s.ArgumentScaleType = ScaleType.DateTime;
            double mult = 2.0 / (period + 1);
            double ema = candles[0].Close;
            for (int i = 0; i < candles.Count; i++)
            {
                ema = (candles[i].Close - ema) * mult + ema;
                if (i >= period - 1) s.Points.Add(new SeriesPoint(candles[i].Time, ema));
            }
            ((LineSeriesView)s.View).Color = color;
            ((LineSeriesView)s.View).LineStyle.Thickness = 2;
            chartMain.Series.Add(s);
        }
        
        private void AddBollingerBands(List<MarketCandle> candles, int period, double stdDevMult)
        {
            if (candles.Count < period) return;
            
            var upperSeries = new Series("BB Upper", ViewType.Line);
            var lowerSeries = new Series("BB Lower", ViewType.Line);
            var middleSeries = new Series("BB Middle", ViewType.Line);
            upperSeries.ArgumentScaleType = ScaleType.DateTime;
            lowerSeries.ArgumentScaleType = ScaleType.DateTime;
            middleSeries.ArgumentScaleType = ScaleType.DateTime;
            
            for (int i = period - 1; i < candles.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < period; j++) sum += candles[i - j].Close;
                double sma = sum / period;
                
                double variance = 0;
                for (int j = 0; j < period; j++) variance += Math.Pow(candles[i - j].Close - sma, 2);
                double stdDev = Math.Sqrt(variance / period);
                
                middleSeries.Points.Add(new SeriesPoint(candles[i].Time, sma));
                upperSeries.Points.Add(new SeriesPoint(candles[i].Time, sma + stdDevMult * stdDev));
                lowerSeries.Points.Add(new SeriesPoint(candles[i].Time, sma - stdDevMult * stdDev));
            }
            
            ((LineSeriesView)middleSeries.View).Color = Color.FromArgb(100, 149, 237);
            ((LineSeriesView)upperSeries.View).Color = Color.FromArgb(100, 149, 237);
            ((LineSeriesView)lowerSeries.View).Color = Color.FromArgb(100, 149, 237);
            ((LineSeriesView)middleSeries.View).LineStyle.Thickness = 1;
            ((LineSeriesView)upperSeries.View).LineStyle.Thickness = 1;
            ((LineSeriesView)lowerSeries.View).LineStyle.Thickness = 1;
            ((LineSeriesView)upperSeries.View).LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dash;
            ((LineSeriesView)lowerSeries.View).LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dash;
            
            chartMain.Series.Add(middleSeries);
            chartMain.Series.Add(upperSeries);
            chartMain.Series.Add(lowerSeries);
        }
        
        private void ShowChartError(string msg)
        {
            lblChartLoading.Text = " " + msg;
            lblChartLoading.Visible = true;
            lblChartLoading.BringToFront();
        }
        
        private void InitializeComponents()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(14, 14, 14);
            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
            
            // HEADER
            pnlHeader = new PanelControl();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 46;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Appearance.BackColor = Color.FromArgb(20, 20, 20);
            pnlHeader.Padding = new Padding(0);
            
            btnBack = new SimpleButton();
            btnBack.Text = " Geri";
            btnBack.Size = new Size(70, 30);
            btnBack.Location = new Point(8, 8);
            btnBack.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnBack.Appearance.ForeColor = Color.White;
            btnBack.Appearance.Options.UseBackColor = true;
            btnBack.Appearance.Options.UseForeColor = true;
            btnBack.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);
            pnlHeader.Controls.Add(btnBack);
            
            lblSymbol = new LabelControl();
            lblSymbol.Text = "AAPL";
            lblSymbol.Appearance.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblSymbol.Appearance.ForeColor = Color.White;
            lblSymbol.Location = new Point(90, 12);
            pnlHeader.Controls.Add(lblSymbol);
            
            int tfX = 200;
            btnTf1m = CreateTfBtn("1m", tfX); tfX += 42;
            btnTf5m = CreateTfBtn("5m", tfX); tfX += 42;
            btnTf15m = CreateTfBtn("15m", tfX); tfX += 48;
            btnTf1H = CreateTfBtn("1H", tfX); tfX += 42;
            btnTf4H = CreateTfBtn("4H", tfX); tfX += 42;
            btnTf1D = CreateTfBtn("1D", tfX); tfX += 42;
            btnTf1W = CreateTfBtn("1W", tfX);
            pnlHeader.Controls.Add(btnTf1m);
            pnlHeader.Controls.Add(btnTf5m);
            pnlHeader.Controls.Add(btnTf15m);
            pnlHeader.Controls.Add(btnTf1H);
            pnlHeader.Controls.Add(btnTf4H);
            pnlHeader.Controls.Add(btnTf1D);
            pnlHeader.Controls.Add(btnTf1W);
            btnTf1D.Appearance.BackColor = Color.FromArgb(33, 150, 243);
            
            chkMA20 = new CheckEdit();
            chkMA20.Text = "MA20";
            chkMA20.Size = new Size(60, 26);
            chkMA20.Location = new Point(tfX + 55, 10);
            chkMA20.Properties.Appearance.ForeColor = Color.FromArgb(255, 193, 7);
            chkMA20.CheckedChanged += (s, e) => { _showMA20 = chkMA20.Checked; LoadChartAsync(); };
            pnlHeader.Controls.Add(chkMA20);
            
            chkEMA12 = new CheckEdit();
            chkEMA12.Text = "EMA12";
            chkEMA12.Size = new Size(70, 26);
            chkEMA12.Location = new Point(tfX + 120, 10);
            chkEMA12.Properties.Appearance.ForeColor = Color.FromArgb(0, 188, 212);
            chkEMA12.CheckedChanged += (s, e) => { _showEMA12 = chkEMA12.Checked; LoadChartAsync(); };
            pnlHeader.Controls.Add(chkEMA12);
            
            chkBollinger = new CheckEdit();
            chkBollinger.Text = "BB";
            chkBollinger.Size = new Size(50, 26);
            chkBollinger.Location = new Point(tfX + 190, 10);
            chkBollinger.Properties.Appearance.ForeColor = Color.FromArgb(100, 149, 237);
            chkBollinger.CheckedChanged += (s, e) => { _showBollinger = chkBollinger.Checked; LoadChartAsync(); };
            pnlHeader.Controls.Add(chkBollinger);
            
            btnRefresh = new SimpleButton();
            btnRefresh.Text = "🔄";
            btnRefresh.Size = new Size(36, 30);
            btnRefresh.Location = new Point(tfX + 250, 8);
            btnRefresh.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnRefresh.Appearance.ForeColor = Color.White;
            btnRefresh.Appearance.Options.UseBackColor = true;
            btnRefresh.Click += (s, e) => { UpdatePriceDisplay(); LoadChartAsync(); };
            pnlHeader.Controls.Add(btnRefresh);
            
            btnAnalysis = new SimpleButton();
            btnAnalysis.Text = "⛶ Analiz";
            btnAnalysis.Size = new Size(80, 30);
            btnAnalysis.Location = new Point(tfX + 295, 8);
            btnAnalysis.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            btnAnalysis.Appearance.ForeColor = Color.White;
            btnAnalysis.Appearance.Options.UseBackColor = true;
            btnAnalysis.Appearance.Options.UseForeColor = true;
            btnAnalysis.Click += BtnAnalysis_Click;
            pnlHeader.Controls.Add(btnAnalysis);
            
            // RIGHT PANEL - Binance-like Order Entry
            pnlRight = new PanelControl();
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Width = 300;
            pnlRight.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlRight.Appearance.BackColor = Color.FromArgb(18, 18, 18);
            CreateOrderPanel();
            
            // BOTTOM - Open Orders / History
            pnlBottom = new PanelControl();
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Height = 180;
            pnlBottom.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlBottom.Appearance.BackColor = Color.FromArgb(18, 18, 18);
            CreateBottomTabs();
            
            // CHART
            pnlChart = new PanelControl();
            pnlChart.Dock = DockStyle.Fill;
            pnlChart.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlChart.Appearance.BackColor = Color.FromArgb(14, 14, 14);
            pnlChart.Padding = new Padding(0);
            
            lblChartLoading = new LabelControl();
            lblChartLoading.Text = " Grafik yükleniyor...";
            lblChartLoading.Appearance.Font = new Font("Segoe UI", 13F);
            lblChartLoading.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            lblChartLoading.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblChartLoading.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblChartLoading.AutoSizeMode = LabelAutoSizeMode.None;
            lblChartLoading.Dock = DockStyle.Fill;
            lblChartLoading.Visible = false;
            
            chartMain = new ChartControl();
            chartMain.Dock = DockStyle.Fill;
            chartMain.BackColor = Color.FromArgb(14, 14, 14);
            chartMain.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            
            pnlChart.Controls.Add(lblChartLoading);
            pnlChart.Controls.Add(chartMain);
            chartMain.BringToFront();
            
            this.Controls.Add(pnlChart);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlRight);
            this.Controls.Add(pnlHeader);
            this.ResumeLayout(true);
        }
        
        private SimpleButton CreateTfBtn(string text, int x)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Size = new Size(38, 30);
            btn.Location = new Point(x, 8);
            btn.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Click += TfBtn_Click;
            return btn;
        }
        
        private void TfBtn_Click(object sender, EventArgs e)
        {
            btnTf1m.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf5m.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf15m.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf1H.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf4H.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf1D.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf1W.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            var btn = sender as SimpleButton;
            if (btn != null) { btn.Appearance.BackColor = Color.FromArgb(33, 150, 243); _currentTimeframe = btn.Text; LoadChartAsync(); }
        }
        
        private void BtnAnalysis_Click(object sender, EventArgs e)
        {
            var form = new FullscreenChartForm(_dataProvider, _currentSymbol, _currentTimeframe);
            form.ShowDialog();
        }
        
        private void CreateOrderPanel()
        {
            int y = 12;
            
            var lblTitle = new LabelControl();
            lblTitle.Text = "Spot İşlem";
            lblTitle.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;
            lblTitle.Location = new Point(15, y);
            pnlRight.Controls.Add(lblTitle);
            y += 35;
            
            lblCurrentPrice = new LabelControl();
            lblCurrentPrice.Text = "--";
            lblCurrentPrice.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCurrentPrice.Appearance.ForeColor = Color.FromArgb(38, 166, 91);
            lblCurrentPrice.Location = new Point(15, y);
            pnlRight.Controls.Add(lblCurrentPrice);
            y += 40;
            
            var lblType = new LabelControl();
            lblType.Text = "Emir Tipi";
            lblType.Appearance.Font = new Font("Segoe UI", 9F);
            lblType.Appearance.ForeColor = Color.FromArgb(130, 130, 130);
            lblType.Location = new Point(15, y);
            pnlRight.Controls.Add(lblType);
            y += 22;
            
            cmbOrderType = new ComboBoxEdit();
            cmbOrderType.Properties.Items.AddRange(new[] { "Market", "Limit", "Stop-Limit" });
            cmbOrderType.EditValue = "Market";
            cmbOrderType.Size = new Size(270, 28);
            cmbOrderType.Location = new Point(15, y);
            cmbOrderType.Properties.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            cmbOrderType.Properties.Appearance.ForeColor = Color.White;
            cmbOrderType.EditValueChanged += (s, e) => txtPrice.Enabled = cmbOrderType.EditValue.ToString() != "Market";
            pnlRight.Controls.Add(cmbOrderType);
            y += 38;
            
            var lblPriceLbl = new LabelControl();
            lblPriceLbl.Text = "Fiyat (USDT)";
            lblPriceLbl.Appearance.Font = new Font("Segoe UI", 9F);
            lblPriceLbl.Appearance.ForeColor = Color.FromArgb(130, 130, 130);
            lblPriceLbl.Location = new Point(15, y);
            pnlRight.Controls.Add(lblPriceLbl);
            y += 22;
            
            txtPrice = new TextEdit();
            txtPrice.Size = new Size(270, 28);
            txtPrice.Location = new Point(15, y);
            txtPrice.Properties.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            txtPrice.Properties.Appearance.ForeColor = Color.White;
            txtPrice.Properties.NullText = "Market Fiyatı";
            txtPrice.Enabled = false;
            pnlRight.Controls.Add(txtPrice);
            y += 38;
            
            var lblAmtLbl = new LabelControl();
            lblAmtLbl.Text = "Miktar";
            lblAmtLbl.Appearance.Font = new Font("Segoe UI", 9F);
            lblAmtLbl.Appearance.ForeColor = Color.FromArgb(130, 130, 130);
            lblAmtLbl.Location = new Point(15, y);
            pnlRight.Controls.Add(lblAmtLbl);
            y += 22;
            
            txtAmount = new TextEdit();
            txtAmount.Size = new Size(270, 28);
            txtAmount.Location = new Point(15, y);
            txtAmount.Properties.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            txtAmount.Properties.Appearance.ForeColor = Color.White;
            txtAmount.EditValue = "1";
            txtAmount.EditValueChanged += (s, e) => UpdateTotal();
            pnlRight.Controls.Add(txtAmount);
            y += 38;
            
            // Quick % buttons
            int bx = 15;
            btnPct25 = CreatePctBtn("25%", bx); bx += 68;
            btnPct50 = CreatePctBtn("50%", bx); bx += 68;
            btnPct75 = CreatePctBtn("75%", bx); bx += 68;
            btnPct100 = CreatePctBtn("100%", bx);
            btnPct25.Location = new Point(15, y);
            btnPct50.Location = new Point(83, y);
            btnPct75.Location = new Point(151, y);
            btnPct100.Location = new Point(219, y);
            pnlRight.Controls.Add(btnPct25);
            pnlRight.Controls.Add(btnPct50);
            pnlRight.Controls.Add(btnPct75);
            pnlRight.Controls.Add(btnPct100);
            y += 38;
            
            var lblTotalLbl = new LabelControl();
            lblTotalLbl.Text = "Toplam (USDT)";
            lblTotalLbl.Appearance.Font = new Font("Segoe UI", 9F);
            lblTotalLbl.Appearance.ForeColor = Color.FromArgb(130, 130, 130);
            lblTotalLbl.Location = new Point(15, y);
            pnlRight.Controls.Add(lblTotalLbl);
            y += 22;
            
            txtTotal = new TextEdit();
            txtTotal.Size = new Size(270, 28);
            txtTotal.Location = new Point(15, y);
            txtTotal.Properties.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            txtTotal.Properties.Appearance.ForeColor = Color.White;
            txtTotal.Properties.ReadOnly = true;
            pnlRight.Controls.Add(txtTotal);
            y += 38;
            
            lblAvailBalance = new LabelControl();
            lblAvailBalance.Text = "Bakiye: ,000.00";
            lblAvailBalance.Appearance.Font = new Font("Segoe UI", 9F);
            lblAvailBalance.Appearance.ForeColor = Color.FromArgb(100, 100, 100);
            lblAvailBalance.Location = new Point(15, y);
            pnlRight.Controls.Add(lblAvailBalance);
            y += 22;
            
            lblFee = new LabelControl();
            lblFee.Text = "Tahmini Komisyon: ~.10";
            lblFee.Appearance.Font = new Font("Segoe UI", 9F);
            lblFee.Appearance.ForeColor = Color.FromArgb(100, 100, 100);
            lblFee.Location = new Point(15, y);
            pnlRight.Controls.Add(lblFee);
            y += 32;
            
            btnBuy = new SimpleButton();
            btnBuy.Text = "AL (BUY)";
            btnBuy.Size = new Size(130, 42);
            btnBuy.Location = new Point(15, y);
            btnBuy.Appearance.BackColor = Color.FromArgb(38, 166, 91);
            btnBuy.Appearance.ForeColor = Color.White;
            btnBuy.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnBuy.Appearance.Options.UseBackColor = true;
            btnBuy.Appearance.Options.UseForeColor = true;
            btnBuy.Appearance.Options.UseFont = true;
            btnBuy.Click += BtnBuy_Click;
            pnlRight.Controls.Add(btnBuy);
            
            btnSell = new SimpleButton();
            btnSell.Text = "SAT (SELL)";
            btnSell.Size = new Size(130, 42);
            btnSell.Location = new Point(155, y);
            btnSell.Appearance.BackColor = Color.FromArgb(232, 65, 66);
            btnSell.Appearance.ForeColor = Color.White;
            btnSell.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSell.Appearance.Options.UseBackColor = true;
            btnSell.Appearance.Options.UseForeColor = true;
            btnSell.Appearance.Options.UseFont = true;
            btnSell.Click += BtnSell_Click;
            pnlRight.Controls.Add(btnSell);
        }
        
        private SimpleButton CreatePctBtn(string text, int x)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Size = new Size(64, 26);
            btn.Appearance.BackColor = Color.FromArgb(35, 35, 35);
            btn.Appearance.ForeColor = Color.FromArgb(150, 150, 150);
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Click += (s, e) => {
                var pct = int.Parse(text.Replace("%", "")) / 100.0;
                txtAmount.EditValue = (10 * pct).ToString("N4");
            };
            return btn;
        }
        
        private void UpdateTotal()
        {
            try
            {
                var amt = double.Parse(txtAmount.Text ?? "0");
                var price = 100.0; // Placeholder
                txtTotal.EditValue = (amt * price).ToString("N2");
                lblFee.Text = "Tahmini Komisyon: ~$" + (amt * price * 0.001).ToString("N2");
            }
            catch { }
        }
        
        private void CreateBottomTabs()
        {
            tabBottom = new XtraTabControl();
            tabBottom.Dock = DockStyle.Fill;
            tabBottom.LookAndFeel.SkinName = "Office 2019 Black";
            tabBottom.LookAndFeel.UseDefaultLookAndFeel = false;
            
            var tabOrders = new XtraTabPage();
            tabOrders.Text = "Açık Emirler";
            gridOrders = new GridControl();
            gridOrders.Dock = DockStyle.Fill;
            gridOrders.LookAndFeel.SkinName = "Office 2019 Black";
            var viewOrders = new GridView(gridOrders);
            gridOrders.MainView = viewOrders;
            viewOrders.OptionsView.ShowGroupPanel = false;
            viewOrders.OptionsView.ShowIndicator = false;
            tabOrders.Controls.Add(gridOrders);
            tabBottom.TabPages.Add(tabOrders);
            
            var tabHistory = new XtraTabPage();
            tabHistory.Text = "İşlem Geçmişi";
            gridHistory = new GridControl();
            gridHistory.Dock = DockStyle.Fill;
            gridHistory.LookAndFeel.SkinName = "Office 2019 Black";
            var viewHistory = new GridView(gridHistory);
            gridHistory.MainView = viewHistory;
            viewHistory.OptionsView.ShowGroupPanel = false;
            viewHistory.OptionsView.ShowIndicator = false;
            tabHistory.Controls.Add(gridHistory);
            tabBottom.TabPages.Add(tabHistory);
            
            pnlBottom.Controls.Add(tabBottom);
        }
        
        private void BtnBuy_Click(object sender, EventArgs e)
        {
            var orderType = cmbOrderType.EditValue?.ToString() ?? "Market";
            var amount = txtAmount.Text;
            MessageBox.Show($"BUY Emri\nSembol: {_currentSymbol}\nTip: {orderType}\nMiktar: {amount}", "Emir Onayı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void BtnSell_Click(object sender, EventArgs e)
        {
            var orderType = cmbOrderType.EditValue?.ToString() ?? "Market";
            var amount = txtAmount.Text;
            MessageBox.Show($"SELL Emri\nSembol: {_currentSymbol}\nTip: {orderType}\nMiktar: {amount}", "Emir Onayı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) { BackRequested?.Invoke(this, EventArgs.Empty); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
