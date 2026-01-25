using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.Utils;
using BankApp.Infrastructure.Services;
using BankApp.UI.Services.Pdf;

namespace BankApp.UI.Forms
{
    public class ChartAnalysisForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly IMarketDataProvider _dataProvider;
        private string _symbol;
        private string _timeframe = "1D";
        private List<MarketCandle> _candles = new List<MarketCandle>();
        
        // Layout
        private Panel pnlToolbar;
        private Panel pnlTopBar;
        private ChartControl chartMain;
        private ChartControl chartRSI;
        private ChartControl chartMACD;
        private XYDiagram _diagram;
        private XYDiagram _diagramRSI;
        private XYDiagram _diagramMACD;
        
        // Top bar controls
        private LabelControl lblSymbol;
        private SimpleButton[] _tfButtons;
        private CheckButton chkMA20, chkMA50, chkEMA12, chkEMA26, chkBB, chkVolume, chkRSI, chkMACD;
        private SimpleButton btnResetZoom, btnClose;
        
        // Drawing state
        private string _currentTool = "select";
        private List<ChartDrawingObject> _drawings = new List<ChartDrawingObject>();
        private ChartDrawingObject _selectedDrawing = null;
        private ChartDrawingObject _activeDrawing = null;
        private Point? _drawStart = null;
        private bool _isDrawing = false;
        private bool _isDragging = false;
        private bool _snapToOHLC = true;
        
        // Tool buttons
        private Dictionary<string, SimpleButton> _toolButtons = new Dictionary<string, SimpleButton>();
        
        // Indicators state
        private bool _showMA20, _showMA50, _showEMA12, _showEMA26, _showBB, _showVolume, _showRSI, _showMACD;
        
        // Undo/Redo
        private Stack<List<ChartDrawingObject>> _undoStack = new Stack<List<ChartDrawingObject>>();
        private Stack<List<ChartDrawingObject>> _redoStack = new Stack<List<ChartDrawingObject>>();
        
        public ChartAnalysisForm(IMarketDataProvider dataProvider, string symbol, string timeframe)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] OPENED: {GetType().FullName}, Symbol={symbol}, Timeframe={timeframe}");
            
            _dataProvider = dataProvider;
            _symbol = symbol;
            _timeframe = timeframe;
            
            InitializeForm();
            InitializeTopBar();
            InitializeToolPanel();
            InitializeChart();
            
            this.KeyPreview = true;
            this.KeyDown += OnKeyDown;
            this.Shown += async (s, e) => await LoadChartDataAsync();
        }
        
        private void InitializeForm()
        {
            this.Text = $"Grafik Analiz - {_symbol}";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(14, 14, 14);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 700);
        }
        
        private void InitializeTopBar()
        {
            pnlTopBar = new Panel();
            pnlTopBar.Dock = DockStyle.Top;
            pnlTopBar.Height = 44;
            pnlTopBar.BackColor = Color.FromArgb(22, 22, 22);
            pnlTopBar.Padding = new Padding(0);
            
            // Symbol label
            lblSymbol = new LabelControl();
            lblSymbol.Text = $"📊 {_symbol}";
            lblSymbol.Appearance.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblSymbol.Appearance.ForeColor = Color.White;
            lblSymbol.Location = new Point(60, 10);
            pnlTopBar.Controls.Add(lblSymbol);
            
            // Timeframe buttons
            string[] tfs = { "1m", "5m", "15m", "1H", "4H", "1D", "1W" };
            _tfButtons = new SimpleButton[tfs.Length];
            int tfX = 220;
            for (int i = 0; i < tfs.Length; i++)
            {
                var btn = CreateTopButton(tfs[i], tfX, 44);
                btn.Tag = tfs[i];
                btn.Click += TfButton_Click;
                if (tfs[i] == _timeframe) btn.Appearance.BackColor = Color.FromArgb(33, 150, 243);
                _tfButtons[i] = btn;
                pnlTopBar.Controls.Add(btn);
                tfX += 48;
            }
            
            // Indicator toggles
            int indX = tfX + 30;
            chkMA20 = CreateIndicatorToggle("MA20", indX, Color.FromArgb(255, 193, 7)); indX += 65;
            chkMA50 = CreateIndicatorToggle("MA50", indX, Color.FromArgb(156, 39, 176)); indX += 65;
            chkEMA12 = CreateIndicatorToggle("EMA12", indX, Color.FromArgb(0, 188, 212)); indX += 70;
            chkEMA26 = CreateIndicatorToggle("EMA26", indX, Color.FromArgb(255, 152, 0)); indX += 70;
            chkBB = CreateIndicatorToggle("BB", indX, Color.FromArgb(100, 149, 237)); indX += 50;
            chkVolume = CreateIndicatorToggle("Vol", indX, Color.FromArgb(128, 128, 128)); indX += 50;
            chkRSI = CreateIndicatorToggle("RSI", indX, Color.FromArgb(233, 30, 99)); indX += 50;
            chkMACD = CreateIndicatorToggle("MACD", indX, Color.FromArgb(76, 175, 80)); indX += 65;
            
            pnlTopBar.Controls.Add(chkMA20);
            pnlTopBar.Controls.Add(chkMA50);
            pnlTopBar.Controls.Add(chkEMA12);
            pnlTopBar.Controls.Add(chkEMA26);
            pnlTopBar.Controls.Add(chkBB);
            pnlTopBar.Controls.Add(chkVolume);
            pnlTopBar.Controls.Add(chkRSI);
            pnlTopBar.Controls.Add(chkMACD);
            
            // Reset zoom
            btnResetZoom = new SimpleButton();
            btnResetZoom.Text = "⟲ Reset";
            btnResetZoom.Size = new Size(70, 30);
            btnResetZoom.Location = new Point(indX + 20, 7);
            btnResetZoom.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnResetZoom.Appearance.ForeColor = Color.White;
            btnResetZoom.Appearance.Options.UseBackColor = true;
            btnResetZoom.Click += (s, e) => {
                System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] HANDLER: btnResetZoom clicked, this={GetType().FullName}");
                ResetZoom();
            };
            pnlTopBar.Controls.Add(btnResetZoom);
            
            // Close button
            btnClose = new SimpleButton();
            btnClose.Text = "✕ Kapat";
            btnClose.Size = new Size(80, 30);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(this.ClientSize.Width - 100, 7);
            btnClose.Appearance.BackColor = Color.FromArgb(180, 50, 50);
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Click += (s, e) => this.Close();
            pnlTopBar.Controls.Add(btnClose);
            
            this.Controls.Add(pnlTopBar);
        }
        
        private void InitializeToolPanel()
        {
            pnlToolbar = new Panel();
            pnlToolbar.Dock = DockStyle.Left;
            pnlToolbar.Width = 50;
            pnlToolbar.BackColor = Color.FromArgb(22, 22, 22);
            
            var tools = new (string id, string icon, string tooltip)[]
            {
                ("select", "⊕", "Seç / Taşı (V)"),
                ("crosshair", "╋", "Crosshair (C)"),
                ("trendline", "╱", "Trend Çizgisi (T)"),
                ("hline", "━", "Yatay Çizgi (H)"),
                ("ray", "→", "Işın (R)"),
                ("channel", "▭", "Paralel Kanal"),
                ("fibretr", "ƒ", "Fib Geri Çekilme"),
                ("fibext", "ƒ+", "Fib Uzatma"),
                ("rectangle", "▢", "Dikdörtgen / Bölge"),
                ("text", "T", "Metin / Not"),
                ("pricelabel", "💲", "Fiyat Etiketi"),
                ("measure", "📏", "Ölçüm Aracı"),
                ("snap", "🧲", "OHLC Snap (S)"),
                ("delete", "🗑", "Seçiliyi Sil (Del)"),
                ("clearall", "🧹", "Tümünü Temizle"),
                ("undo", "↩", "Geri Al (Ctrl+Z)"),
                ("redo", "↪", "İleri Al (Ctrl+Y)"),
                ("exportpdf", "📄", "PDF Export")
            };
            
            int y = 8;
            foreach (var (id, icon, tooltip) in tools)
            {
                var btn = CreateToolButton(id, icon, tooltip, y);
                _toolButtons[id] = btn;
                pnlToolbar.Controls.Add(btn);
                y += 38;
            }
            
            // Set select as default
            _toolButtons["select"].Appearance.BackColor = Color.FromArgb(33, 150, 243);
            
            this.Controls.Add(pnlToolbar);
        }
        
        private void InitializeChart()
        {
            // Main chart (Candle + Volume)
            chartMain = new ChartControl();
            chartMain.Dock = DockStyle.Fill;
            chartMain.BackColor = Color.FromArgb(14, 14, 14);
            chartMain.AppearanceName = "Dark";
            chartMain.Legend.Visibility = DefaultBoolean.False;
            chartMain.RuntimeHitTesting = true;
            chartMain.BorderOptions.Visibility = DefaultBoolean.False;
            chartMain.ContextMenuStrip = null;
            
            chartMain.MouseDown += Chart_MouseDown;
            chartMain.MouseMove += Chart_MouseMove;
            chartMain.MouseUp += Chart_MouseUp;
            chartMain.MouseDoubleClick += Chart_DoubleClick;
            chartMain.CustomPaint += Chart_CustomPaint;
            chartMain.CustomDrawCrosshair += Chart_CustomDrawCrosshair;
            
            this.Controls.Add(chartMain);
            
            // RSI chart (bottom panel)
            chartRSI = new ChartControl();
            chartRSI.Dock = DockStyle.Bottom;
            chartRSI.Height = 150;
            chartRSI.BackColor = Color.FromArgb(14, 14, 14);
            chartRSI.AppearanceName = "Dark";
            chartRSI.Legend.Visibility = DefaultBoolean.False;
            chartRSI.BorderOptions.Visibility = DefaultBoolean.False;
            chartRSI.ContextMenuStrip = null;
            chartRSI.Visible = false;
            this.Controls.Add(chartRSI);
            
            // MACD chart (bottom panel)
            chartMACD = new ChartControl();
            chartMACD.Dock = DockStyle.Bottom;
            chartMACD.Height = 150;
            chartMACD.BackColor = Color.FromArgb(14, 14, 14);
            chartMACD.AppearanceName = "Dark";
            chartMACD.Legend.Visibility = DefaultBoolean.False;
            chartMACD.BorderOptions.Visibility = DefaultBoolean.False;
            chartMACD.ContextMenuStrip = null;
            chartMACD.Visible = false;
            this.Controls.Add(chartMACD);
            
            chartMain.SendToBack();
        }
        
        private SimpleButton CreateTopButton(string text, int x, int width)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Size = new Size(width, 30);
            btn.Location = new Point(x, 7);
            btn.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            return btn;
        }
        
        private CheckButton CreateIndicatorToggle(string text, int x, Color color)
        {
            var chk = new CheckButton();
            chk.Text = text;
            chk.Size = new Size(60, 28);
            chk.Location = new Point(x, 8);
            chk.Appearance.BackColor = Color.FromArgb(35, 35, 35);
            chk.Appearance.ForeColor = color;
            chk.Appearance.Font = new Font("Segoe UI", 8F);
            chk.Appearance.Options.UseBackColor = true;
            chk.Appearance.Options.UseForeColor = true;
            chk.Appearance.Options.UseFont = true;
            chk.CheckedChanged += Indicator_CheckedChanged;
            return chk;
        }
        
        private SimpleButton CreateToolButton(string id, string icon, string tooltip, int y)
        {
            var btn = new SimpleButton();
            btn.Text = icon;
            btn.Tag = id;
            btn.Size = new Size(40, 34);
            btn.Location = new Point(5, y);
            btn.Appearance.BackColor = Color.FromArgb(35, 35, 35);
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.Font = new Font("Segoe UI", 12F);
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Appearance.Options.UseFont = true;
            
            var tt = new ToolTipController();
            btn.SuperTip = new SuperToolTip();
            btn.SuperTip.Items.Add(tooltip);
            
            btn.Click += ToolButton_Click;
            return btn;
        }
        
        private void ToolButton_Click(object sender, EventArgs e)
        {
            var btn = sender as SimpleButton;
            if (btn == null) return;
            
            var toolId = btn.Tag?.ToString();
            
            // Handle action buttons separately
            switch (toolId)
            {
                case "snap":
                    _snapToOHLC = !_snapToOHLC;
                    btn.Appearance.BackColor = _snapToOHLC ? Color.FromArgb(33, 150, 243) : Color.FromArgb(35, 35, 35);
                    return;
                case "delete":
                    DeleteSelected();
                    return;
                case "clearall":
                    ClearAllDrawings();
                    return;
                case "exportpdf":
                    ExportPdf();
                    return;
                case "undo":
                    Undo();
                    return;
                case "redo":
                    Redo();
                    return;
            }
            
            // Set current tool
            _currentTool = toolId;
            CancelActiveDrawing();
            
            // Update button states
            foreach (var kvp in _toolButtons)
            {
                if (kvp.Key == "snap" || kvp.Key == "delete" || kvp.Key == "clearall" || kvp.Key == "undo" || kvp.Key == "redo")
                    continue;
                kvp.Value.Appearance.BackColor = kvp.Key == toolId ? Color.FromArgb(33, 150, 243) : Color.FromArgb(35, 35, 35);
            }
            
            // Enable crosshair for crosshair tool
            if (_diagram != null)
            {
                chartMain.CrosshairEnabled = toolId == "crosshair" ? DefaultBoolean.True : DefaultBoolean.False;
            }
        }
        
        private void TfButton_Click(object sender, EventArgs e)
        {
            var btn = sender as SimpleButton;
            if (btn == null) return;
            
            _timeframe = btn.Tag?.ToString() ?? "1D";
            
            foreach (var b in _tfButtons)
                b.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btn.Appearance.BackColor = Color.FromArgb(33, 150, 243);
            
            _ = LoadChartDataAsync();
        }
        
        private void Indicator_CheckedChanged(object sender, EventArgs e)
        {
            _showMA20 = chkMA20.Checked;
            _showMA50 = chkMA50.Checked;
            _showEMA12 = chkEMA12.Checked;
            _showEMA26 = chkEMA26.Checked;
            _showBB = chkBB.Checked;
            _showVolume = chkVolume.Checked;
            _showRSI = chkRSI.Checked;
            _showMACD = chkMACD.Checked;
            
            RenderChart();
        }
        
        private async System.Threading.Tasks.Task LoadChartDataAsync()
        {
            try
            {
                var tfMap = _timeframe switch
                {
                    "1m" => "1", "5m" => "5", "15m" => "15",
                    "1H" => "60", "4H" => "240", "1D" => "D", "1W" => "W",
                    _ => "D"
                };
                
                _candles = await _dataProvider.GetCandlesAsync(_symbol, tfMap, 300);
                RenderChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void RenderChart()
        {
            if (_candles == null || _candles.Count == 0) return;
            
            chartMain.Series.Clear();
            
            // Main candlestick series
            var series = new Series(_symbol, ViewType.CandleStick);
            series.ArgumentScaleType = ScaleType.DateTime;
            
            foreach (var c in _candles)
                series.Points.Add(new SeriesPoint(c.Time, c.Low, c.High, c.Open, c.Close));
            
            var view = (CandleStickSeriesView)series.View;
            view.Color = Color.FromArgb(38, 166, 91);
            view.ReductionOptions.Color = Color.FromArgb(232, 65, 66);
            view.ReductionOptions.Level = StockLevel.Close;
            view.ReductionOptions.Visible = true;
            view.LineThickness = 2;
            view.LevelLineLength = 0.6;
            
            chartMain.Series.Add(series);
            
            // Add indicators to main chart
            if (_showMA20) AddMA(20, Color.FromArgb(255, 193, 7));
            if (_showMA50) AddMA(50, Color.FromArgb(156, 39, 176));
            if (_showEMA12) AddEMA(12, Color.FromArgb(0, 188, 212));
            if (_showEMA26) AddEMA(26, Color.FromArgb(255, 152, 0));
            if (_showBB) AddBollingerBands(20, 2);
            if (_showVolume) AddVolumeIndicator();
            
            // RSI and MACD in separate panels
            chartRSI.Visible = _showRSI;
            chartMACD.Visible = _showMACD;
            if (_showRSI) RenderRSIChart();
            if (_showMACD) RenderMACDChart();
            
            // Configure diagram
            _diagram = chartMain.Diagram as XYDiagram;
            if (_diagram != null)
            {
                _diagram.EnableAxisXZooming = true;
                _diagram.EnableAxisYZooming = true;
                _diagram.EnableAxisXScrolling = true;
                _diagram.EnableAxisYScrolling = true;
                _diagram.ZoomingOptions.UseMouseWheel = true;
                _diagram.ZoomingOptions.UseKeyboard = true;
                
                _diagram.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Day;
                _diagram.AxisX.Label.TextPattern = "{A:MM/dd HH:mm}";
                _diagram.AxisX.Color = Color.FromArgb(50, 50, 50);
                _diagram.AxisY.Color = Color.FromArgb(50, 50, 50);
                _diagram.AxisX.GridLines.Color = Color.FromArgb(30, 30, 30);
                _diagram.AxisY.GridLines.Color = Color.FromArgb(30, 30, 30);
                _diagram.AxisX.GridLines.Visible = true;
                _diagram.AxisY.GridLines.Visible = true;
                _diagram.AxisX.Label.Font = new Font("Segoe UI", 9F);
                _diagram.AxisY.Label.Font = new Font("Segoe UI", 9F);
                _diagram.AxisX.Label.TextColor = Color.FromArgb(150, 150, 150);
                _diagram.AxisY.Label.TextColor = Color.FromArgb(150, 150, 150);
                _diagram.DefaultPane.BackColor = Color.FromArgb(14, 14, 14);
            }
            
            // Crosshair
            chartMain.CrosshairEnabled = _currentTool == "crosshair" ? DefaultBoolean.True : DefaultBoolean.False;
            chartMain.CrosshairOptions.ShowArgumentLabels = true;
            chartMain.CrosshairOptions.ShowValueLabels = true;
            chartMain.CrosshairOptions.ShowArgumentLine = true;
            chartMain.CrosshairOptions.ShowValueLine = true;
            chartMain.CrosshairOptions.ArgumentLineColor = Color.FromArgb(100, 100, 100);
            chartMain.CrosshairOptions.ValueLineColor = Color.FromArgb(100, 100, 100);
        }
        
        private void AddMA(int period, Color color)
        {
            if (_candles.Count < period) return;
            var s = new Series($"MA{period}", ViewType.Line);
            s.ArgumentScaleType = ScaleType.DateTime;
            for (int i = period - 1; i < _candles.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < period; j++) sum += _candles[i - j].Close;
                s.Points.Add(new SeriesPoint(_candles[i].Time, sum / period));
            }
            ((LineSeriesView)s.View).Color = color;
            ((LineSeriesView)s.View).LineStyle.Thickness = 2;
            chartMain.Series.Add(s);
        }
        
        private void AddEMA(int period, Color color)
        {
            if (_candles.Count < period) return;
            var s = new Series($"EMA{period}", ViewType.Line);
            s.ArgumentScaleType = ScaleType.DateTime;
            double mult = 2.0 / (period + 1);
            double ema = _candles[0].Close;
            for (int i = 0; i < _candles.Count; i++)
            {
                ema = (_candles[i].Close - ema) * mult + ema;
                if (i >= period - 1) s.Points.Add(new SeriesPoint(_candles[i].Time, ema));
            }
            ((LineSeriesView)s.View).Color = color;
            ((LineSeriesView)s.View).LineStyle.Thickness = 2;
            chartMain.Series.Add(s);
        }
        
        private void AddBollingerBands(int period, double stdDevMult)
        {
            if (_candles.Count < period) return;
            var upper = new Series("BB Upper", ViewType.Line);
            var lower = new Series("BB Lower", ViewType.Line);
            var middle = new Series("BB Middle", ViewType.Line);
            upper.ArgumentScaleType = lower.ArgumentScaleType = middle.ArgumentScaleType = ScaleType.DateTime;
            
            for (int i = period - 1; i < _candles.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < period; j++) sum += _candles[i - j].Close;
                double sma = sum / period;
                double variance = 0;
                for (int j = 0; j < period; j++) variance += Math.Pow(_candles[i - j].Close - sma, 2);
                double stdDev = Math.Sqrt(variance / period);
                
                middle.Points.Add(new SeriesPoint(_candles[i].Time, sma));
                upper.Points.Add(new SeriesPoint(_candles[i].Time, sma + stdDevMult * stdDev));
                lower.Points.Add(new SeriesPoint(_candles[i].Time, sma - stdDevMult * stdDev));
            }
            
            var bbColor = Color.FromArgb(100, 149, 237);
            ((LineSeriesView)middle.View).Color = bbColor;
            ((LineSeriesView)upper.View).Color = bbColor;
            ((LineSeriesView)lower.View).Color = bbColor;
            ((LineSeriesView)upper.View).LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dash;
            ((LineSeriesView)lower.View).LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dash;
            
            chartMain.Series.Add(middle);
            chartMain.Series.Add(upper);
            chartMain.Series.Add(lower);
        }
        
        private void AddVolumeIndicator()
        {
            if (_candles.Count == 0) return;
            
            var volSeries = new Series("Volume", ViewType.Bar);
            volSeries.ArgumentScaleType = ScaleType.DateTime;
            
            foreach (var c in _candles)
            {
                volSeries.Points.Add(new SeriesPoint(c.Time, c.Volume));
            }
            
            var view = (BarSeriesView)volSeries.View;
            view.Color = Color.FromArgb(100, 100, 100);
            view.FillStyle.FillMode = DevExpress.XtraCharts.FillMode.Solid;
            
            chartMain.Series.Add(volSeries);
            
            // Volume should use secondary axis
            if (_diagram != null && volSeries.View is BarSeriesView barView)
            {
                var secondaryAxisY = new SecondaryAxisY("VolumeAxis");
                secondaryAxisY.Alignment = AxisAlignment.Far;
                secondaryAxisY.Label.TextColor = Color.FromArgb(150, 150, 150);
                secondaryAxisY.GridLines.Visible = false;
                _diagram.SecondaryAxesY.Add(secondaryAxisY);
                barView.AxisY = secondaryAxisY;
            }
        }
        
        private void RenderRSIChart(int period = 14)
        {
            if (_candles.Count < period + 1) return;
            
            chartRSI.Series.Clear();
            
            var rsiSeries = new Series("RSI(14)", ViewType.Line);
            rsiSeries.ArgumentScaleType = ScaleType.DateTime;
            
            // Calculate RSI
            var gains = new List<double>();
            var losses = new List<double>();
            
            for (int i = 1; i < _candles.Count; i++)
            {
                var change = _candles[i].Close - _candles[i - 1].Close;
                gains.Add(change > 0 ? change : 0);
                losses.Add(change < 0 ? Math.Abs(change) : 0);
            }
            
            for (int i = period; i < gains.Count; i++)
            {
                var avgGain = gains.Skip(i - period).Take(period).Average();
                var avgLoss = losses.Skip(i - period).Take(period).Average();
                
                var rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                var rsi = 100 - (100 / (1 + rs));
                
                rsiSeries.Points.Add(new SeriesPoint(_candles[i + 1].Time, rsi));
            }
            
            ((LineSeriesView)rsiSeries.View).Color = Color.FromArgb(233, 30, 99);
            ((LineSeriesView)rsiSeries.View).LineStyle.Thickness = 2;
            
            chartRSI.Series.Add(rsiSeries);
            
            // Add reference lines at 30 and 70
            var overbought = new Series("OB", ViewType.Line);
            var oversold = new Series("OS", ViewType.Line);
            overbought.ArgumentScaleType = oversold.ArgumentScaleType = ScaleType.DateTime;
            
            foreach (var c in _candles.Skip(period))
            {
                overbought.Points.Add(new SeriesPoint(c.Time, 70));
                oversold.Points.Add(new SeriesPoint(c.Time, 30));
            }
            
            ((LineSeriesView)overbought.View).Color = Color.FromArgb(100, 255, 82, 82);
            ((LineSeriesView)oversold.View).Color = Color.FromArgb(100, 105, 240, 174);
            ((LineSeriesView)overbought.View).LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dot;
            ((LineSeriesView)oversold.View).LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dot;
            
            chartRSI.Series.Add(overbought);
            chartRSI.Series.Add(oversold);
            
            // Configure RSI diagram
            _diagramRSI = chartRSI.Diagram as XYDiagram;
            if (_diagramRSI != null)
            {
                _diagramRSI.AxisX.Visibility = DefaultBoolean.False;
                _diagramRSI.AxisY.WholeRange.MinValue = 0;
                _diagramRSI.AxisY.WholeRange.MaxValue = 100;
                _diagramRSI.AxisY.WholeRange.Auto = false;
                _diagramRSI.AxisY.GridLines.Color = Color.FromArgb(30, 30, 30);
                _diagramRSI.AxisY.Label.TextColor = Color.FromArgb(150, 150, 150);
                _diagramRSI.DefaultPane.BackColor = Color.FromArgb(14, 14, 14);
            }
        }
        
        private void RenderMACDChart(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            if (_candles.Count < slowPeriod + signalPeriod) return;
            
            chartMACD.Series.Clear();
            
            // Calculate EMA
            var fastEMA = CalculateEMAList(fastPeriod);
            var slowEMA = CalculateEMAList(slowPeriod);
            
            if (fastEMA.Count == 0 || slowEMA.Count == 0) return;
            
            // MACD Line = Fast EMA - Slow EMA
            var macdLine = new Series("MACD", ViewType.Line);
            var signalLine = new Series("Signal", ViewType.Line);
            var histogram = new Series("Histogram", ViewType.Bar);
            
            macdLine.ArgumentScaleType = signalLine.ArgumentScaleType = histogram.ArgumentScaleType = ScaleType.DateTime;
            
            var macdValues = new List<(DateTime time, double value)>();
            
            int startIdx = slowPeriod - 1;
            for (int i = startIdx; i < _candles.Count; i++)
            {
                var macd = fastEMA[i - fastPeriod + 1] - slowEMA[i - slowPeriod + 1];
                macdValues.Add((_candles[i].Time, macd));
                macdLine.Points.Add(new SeriesPoint(_candles[i].Time, macd));
            }
            
            // Signal Line = EMA of MACD
            if (macdValues.Count >= signalPeriod)
            {
                double signalEMA = macdValues.Take(signalPeriod).Average(x => x.value);
                double multiplier = 2.0 / (signalPeriod + 1);
                
                for (int i = 0; i < macdValues.Count; i++)
                {
                    if (i > 0)
                        signalEMA = (macdValues[i].value - signalEMA) * multiplier + signalEMA;
                    
                    if (i >= signalPeriod - 1)
                    {
                        signalLine.Points.Add(new SeriesPoint(macdValues[i].time, signalEMA));
                        var histValue = macdValues[i].value - signalEMA;
                        histogram.Points.Add(new SeriesPoint(macdValues[i].time, histValue));
                    }
                }
            }
            
            ((LineSeriesView)macdLine.View).Color = Color.FromArgb(33, 150, 243);
            ((LineSeriesView)signalLine.View).Color = Color.FromArgb(255, 152, 0);
            ((BarSeriesView)histogram.View).Color = Color.FromArgb(76, 175, 80);
            ((LineSeriesView)macdLine.View).LineStyle.Thickness = 2;
            ((LineSeriesView)signalLine.View).LineStyle.Thickness = 2;
            
            chartMACD.Series.Add(histogram);
            chartMACD.Series.Add(macdLine);
            chartMACD.Series.Add(signalLine);
            
            // Configure MACD diagram
            _diagramMACD = chartMACD.Diagram as XYDiagram;
            if (_diagramMACD != null)
            {
                _diagramMACD.AxisX.Visibility = DefaultBoolean.False;
                _diagramMACD.AxisY.GridLines.Color = Color.FromArgb(30, 30, 30);
                _diagramMACD.AxisY.Label.TextColor = Color.FromArgb(150, 150, 150);
                _diagramMACD.DefaultPane.BackColor = Color.FromArgb(14, 14, 14);
            }
        }
        
        private List<double> CalculateEMAList(int period)
        {
            var result = new List<double>();
            if (_candles.Count < period) return result;
            
            double multiplier = 2.0 / (period + 1);
            double ema = _candles.Take(period).Average(c => c.Close);
            
            for (int i = 0; i < period; i++)
                result.Add(ema);
            
            for (int i = period; i < _candles.Count; i++)
            {
                ema = (_candles[i].Close - ema) * multiplier + ema;
                result.Add(ema);
            }
            
            return result;
        }
        
        #region Drawing Methods
        
        private void Chart_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (_currentTool == "select" || _currentTool == "crosshair") return;
            
            var coords = GetChartCoordinates(e.Location);
            if (coords == null) return;
            
            // Special handling for text tool - show input dialog
            if (_currentTool == "text")
            {
                System.Diagnostics.Debug.WriteLine("[CHART] Text tool clicked - showing input dialog");
                var text = ShowInputDialog("Not Metni:", "");
                if (!string.IsNullOrEmpty(text))
                {
                    SaveUndoState();
                    var newNote = new ChartDrawingObject
                    {
                        Type = "text",
                        StartTime = coords.Value.time,
                        StartPrice = coords.Value.price,
                        EndTime = coords.Value.time,
                        EndPrice = coords.Value.price,
                        Color = Color.White,
                        Text = text
                    };
                    _drawings.Add(newNote);
                    System.Diagnostics.Debug.WriteLine($"[CHART] Note added: text='{text}', totalDrawings={_drawings.Count}");
                    chartMain.Invalidate();
                }
                return;
            }
            
            // Special handling for price label - instant placement
            if (_currentTool == "pricelabel")
            {
                SaveUndoState();
                _drawings.Add(new ChartDrawingObject
                {
                    Type = "pricelabel",
                    StartTime = coords.Value.time,
                    StartPrice = coords.Value.price,
                    EndTime = coords.Value.time,
                    EndPrice = coords.Value.price,
                    Color = Color.FromArgb(33, 150, 243)
                });
                chartMain.Invalidate();
                return;
            }
            
            _isDrawing = true;
            _drawStart = e.Location;
            
            SaveUndoState();
            
            _activeDrawing = new ChartDrawingObject
            {
                Type = _currentTool,
                StartTime = coords.Value.time,
                StartPrice = coords.Value.price,
                EndTime = coords.Value.time,
                EndPrice = coords.Value.price,
                Color = GetToolColor(_currentTool)
            };
        }
        
        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing || _activeDrawing == null) return;
            
            var coords = GetChartCoordinates(e.Location);
            if (coords == null) return;
            
            _activeDrawing.EndTime = coords.Value.time;
            _activeDrawing.EndPrice = coords.Value.price;
            
            chartMain.Invalidate();
        }
        
        private void Chart_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isDrawing || _activeDrawing == null) return;
            
            _isDrawing = false;
            _drawings.Add(_activeDrawing);
            _activeDrawing = null;
            _drawStart = null;
            
            chartMain.Invalidate();
        }
        
        private void Chart_DoubleClick(object sender, MouseEventArgs e)
        {
            // Check if double-clicked on a text object for editing
            var coords = GetChartCoordinates(e.Location);
            if (coords != null)
            {
                foreach (var drawing in _drawings)
                {
                    if (drawing.Type == "text")
                    {
                        var screenPos = GetScreenCoordinates(drawing.StartTime, drawing.StartPrice);
                        if (screenPos != null)
                        {
                            var hitRect = new Rectangle(screenPos.Value.X - 10, screenPos.Value.Y - 10, 150, 30);
                            if (hitRect.Contains(e.Location))
                            {
                                // Edit text
                                var newText = ShowInputDialog("Not Düzenle:", drawing.Text ?? "");
                                if (!string.IsNullOrEmpty(newText))
                                {
                                    SaveUndoState();
                                    drawing.Text = newText;
                                    chartMain.Invalidate();
                                }
                                return;
                            }
                        }
                    }
                }
            }
            
            // Default: Reset zoom
            ResetZoom();
        }
        
        private string ShowInputDialog(string prompt, string defaultValue)
        {
            using var form = new Form();
            form.Text = "Giriş";
            form.Size = new Size(350, 150);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.BackColor = Color.FromArgb(30, 30, 30);
            
            var label = new Label { Text = prompt, Left = 20, Top = 20, Width = 300, ForeColor = Color.White };
            var textBox = new TextBox { Left = 20, Top = 45, Width = 290, Text = defaultValue, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            var btnOk = new Button { Text = "Tamam", Left = 140, Top = 80, Width = 80, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "İptal", Left = 230, Top = 80, Width = 80, DialogResult = DialogResult.Cancel };
            
            form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;
            
            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
        
        private void Chart_CustomPaint(object sender, CustomPaintEventArgs e)
        {
            // Draw all saved drawings
            foreach (var drawing in _drawings)
                DrawObject(e.Graphics, drawing, drawing == _selectedDrawing);
            
            // Draw active drawing
            if (_activeDrawing != null)
                DrawObject(e.Graphics, _activeDrawing, false);
        }
        
        private void DrawObject(Graphics g, ChartDrawingObject obj, bool selected)
        {
            var start = GetScreenCoordinates(obj.StartTime, obj.StartPrice);
            var end = GetScreenCoordinates(obj.EndTime, obj.EndPrice);
            
            if (start == null || end == null) return;
            
            using var pen = new Pen(selected ? Color.Yellow : obj.Color, selected ? 3 : 2);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            switch (obj.Type)
            {
                case "trendline":
                case "ray":
                    if (obj.Type == "ray")
                    {
                        var dx = end.Value.X - start.Value.X;
                        var dy = end.Value.Y - start.Value.Y;
                        end = new Point(chartMain.Width, start.Value.Y + (int)(dy * (chartMain.Width - start.Value.X) / (dx == 0 ? 1 : dx)));
                    }
                    g.DrawLine(pen, start.Value, end.Value);
                    break;
                    
                case "hline":
                    g.DrawLine(pen, new Point(0, start.Value.Y), new Point(chartMain.Width, start.Value.Y));
                    // Price label
                    using (var brush = new SolidBrush(obj.Color))
                    using (var font = new Font("Segoe UI", 8F))
                    {
                        g.FillRectangle(brush, chartMain.Width - 70, start.Value.Y - 10, 70, 20);
                        g.DrawString($"{obj.StartPrice:N2}", font, Brushes.White, chartMain.Width - 68, start.Value.Y - 8);
                    }
                    break;
                    
                case "rectangle":
                    var rect = new Rectangle(
                        Math.Min(start.Value.X, end.Value.X),
                        Math.Min(start.Value.Y, end.Value.Y),
                        Math.Abs(end.Value.X - start.Value.X),
                        Math.Abs(end.Value.Y - start.Value.Y));
                    using (var fill = new SolidBrush(Color.FromArgb(30, obj.Color)))
                    {
                        g.FillRectangle(fill, rect);
                        g.DrawRectangle(pen, rect);
                    }
                    break;
                    
                case "channel":
                    g.DrawLine(pen, start.Value, end.Value);
                    var offset = 30;
                    g.DrawLine(pen, new Point(start.Value.X, start.Value.Y + offset), new Point(end.Value.X, end.Value.Y + offset));
                    break;
                    
                case "fibretr":
                    DrawFibonacci(g, start.Value, end.Value, obj.Color, false);
                    break;
                    
                case "fibext":
                    DrawFibonacci(g, start.Value, end.Value, obj.Color, true);
                    break;
                    
                case "text":
                    using (var font = new Font("Segoe UI", 10F))
                    using (var brush = new SolidBrush(obj.Color))
                    {
                        g.DrawString(obj.Text ?? "Note", font, brush, start.Value);
                    }
                    break;
                    
                case "pricelabel":
                    using (var brush = new SolidBrush(obj.Color))
                    using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
                    {
                        var label = $"${obj.StartPrice:N2}";
                        g.FillRectangle(brush, start.Value.X - 5, start.Value.Y - 12, 80, 24);
                        g.DrawString(label, font, Brushes.White, start.Value.X, start.Value.Y - 10);
                    }
                    break;
                    
                case "measure":
                    g.DrawLine(pen, start.Value, end.Value);
                    var priceDiff = obj.EndPrice - obj.StartPrice;
                    var pctChange = obj.StartPrice != 0 ? (priceDiff / obj.StartPrice) * 100 : 0;
                    using (var font = new Font("Segoe UI", 9F))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var text = $"{priceDiff:+0.00;-0.00} ({pctChange:+0.00;-0.00}%)";
                        var midX = (start.Value.X + end.Value.X) / 2;
                        var midY = (start.Value.Y + end.Value.Y) / 2;
                        g.FillRectangle(new SolidBrush(Color.FromArgb(180, 30, 30, 30)), midX - 5, midY - 12, 120, 24);
                        g.DrawString(text, font, brush, midX, midY - 10);
                    }
                    break;
            }
        }
        
        private void DrawFibonacci(Graphics g, Point start, Point end, Color color, bool extension)
        {
            var levels = extension 
                ? new[] { 0, 0.618, 1, 1.272, 1.618, 2.0, 2.618 }
                : new[] { 0, 0.236, 0.382, 0.5, 0.618, 0.786, 1.0 };
            
            var height = end.Y - start.Y;
            
            using var pen = new Pen(color, 1);
            using var font = new Font("Segoe UI", 8F);
            using var brush = new SolidBrush(color);
            
            foreach (var level in levels)
            {
                var y = start.Y + (int)(height * level);
                pen.DashStyle = level == 0 || level == 1 ? System.Drawing.Drawing2D.DashStyle.Solid : System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(pen, new Point(Math.Min(start.X, end.X), y), new Point(chartMain.Width - 80, y));
                g.DrawString($"{level:P1}", font, brush, chartMain.Width - 75, y - 8);
            }
        }
        
        private (DateTime time, double price)? GetChartCoordinates(Point screenPoint)
        {
            if (_diagram == null || _candles.Count == 0) return null;
            
            try
            {
                var diagramCoords = _diagram.PointToDiagram(screenPoint);
                if (diagramCoords == null) return null;
                
                var axisValue = diagramCoords.GetAxisValue(_diagram.AxisX);
                var axisYValue = diagramCoords.GetAxisValue(_diagram.AxisY);
                
                if (axisValue == null || axisYValue == null) return null;
                
                DateTime time = axisValue.DateTimeValue;
                double price = axisYValue.NumericalValue;
                
                // Snap to OHLC if enabled
                if (_snapToOHLC && _candles.Count > 0)
                {
                    var nearestCandle = _candles.OrderBy(c => Math.Abs((c.Time - time).TotalMinutes)).FirstOrDefault();
                    if (nearestCandle != null)
                    {
                        var ohlc = new[] { nearestCandle.Open, nearestCandle.High, nearestCandle.Low, nearestCandle.Close };
                        var closest = ohlc.OrderBy(p => Math.Abs(p - price)).First();
                        var range = _candles.Max(c => c.High) - _candles.Min(c => c.Low);
                        if (range > 0 && Math.Abs(closest - price) < range * 0.02)
                            price = closest;
                    }
                }
                
                return (time, price);
            }
            catch { return null; }
        }
        
        private Point? GetScreenCoordinates(DateTime time, double price)
        {
            if (_diagram == null) return null;
            
            try
            {
                var coords = _diagram.DiagramToPoint(time, price);
                return coords.Point;
            }
            catch { return null; }
        }
        
        private Color GetToolColor(string tool)
        {
            return tool switch
            {
                "trendline" => Color.FromArgb(33, 150, 243),
                "hline" => Color.FromArgb(255, 193, 7),
                "ray" => Color.FromArgb(0, 188, 212),
                "channel" => Color.FromArgb(156, 39, 176),
                "fibretr" => Color.FromArgb(255, 152, 0),
                "fibext" => Color.FromArgb(233, 30, 99),
                "rectangle" => Color.FromArgb(76, 175, 80),
                "text" => Color.White,
                "pricelabel" => Color.FromArgb(33, 150, 243),
                "measure" => Color.FromArgb(255, 255, 255),
                _ => Color.FromArgb(33, 150, 243)
            };
        }
        
        #endregion
        
        #region Actions
        
        private void DeleteSelected()
        {
            if (_selectedDrawing != null)
            {
                SaveUndoState();
                _drawings.Remove(_selectedDrawing);
                _selectedDrawing = null;
                chartMain.Invalidate();
            }
        }
        
        private void ClearAllDrawings()
        {
            int prevCount = _drawings.Count;
            System.Diagnostics.Debug.WriteLine($"[CHART] ClearAllDrawings called, current count={prevCount}");
            if (_drawings.Count > 0)
            {
                SaveUndoState();
                _drawings.Clear();
                chartMain.Invalidate();
                System.Diagnostics.Debug.WriteLine($"[CHART] Drawings cleared: count was {prevCount}, now {_drawings.Count}");
            }
        }
        
        private void CancelActiveDrawing()
        {
            _activeDrawing = null;
            _isDrawing = false;
            _drawStart = null;
            chartMain.Invalidate();
        }
        
        private void ResetZoom()
        {
            // Clear all drawings
            _drawings.Clear();
            _selectedDrawing = null;
            _activeDrawing = null;
            _undoStack.Clear();
            _redoStack.Clear();
            
            // Reset zoom on all charts
            if (_diagram != null)
            {
                _diagram.AxisX.WholeRange.Auto = true;
                _diagram.AxisY.WholeRange.Auto = true;
            }
            if (_diagramRSI != null)
            {
                _diagramRSI.AxisX.WholeRange.Auto = true;
                _diagramRSI.AxisY.WholeRange.Auto = true;
            }
            if (_diagramMACD != null)
            {
                _diagramMACD.AxisX.WholeRange.Auto = true;
                _diagramMACD.AxisY.WholeRange.Auto = true;
            }
            
            // Force redraw
            chartMain?.Invalidate();
            chartRSI?.Invalidate();
            chartMACD?.Invalidate();
        }
        
        private void SaveUndoState()
        {
            var copy = _drawings.Select(d => d.Clone()).ToList();
            _undoStack.Push(copy);
            _redoStack.Clear();
            if (_undoStack.Count > 50)
            {
                var temp = _undoStack.ToArray().Take(50).ToList();
                _undoStack.Clear();
                foreach (var item in temp.AsEnumerable().Reverse()) _undoStack.Push(item);
            }
        }
        
        private void Undo()
        {
            if (_undoStack.Count == 0) return;
            var current = _drawings.Select(d => d.Clone()).ToList();
            _redoStack.Push(current);
            _drawings = _undoStack.Pop();
            chartMain.Invalidate();
        }
        
        private void Redo()
        {
            if (_redoStack.Count == 0) return;
            var current = _drawings.Select(d => d.Clone()).ToList();
            _undoStack.Push(current);
            _drawings = _redoStack.Pop();
            chartMain.Invalidate();
        }
        
        private void ExportPdf()
        {
            try
            {
                var lastPrice = _candles.Count > 0 ? _candles.Last().Close : 0;
                var prevPrice = _candles.Count > 1 ? _candles[^2].Close : lastPrice;
                var changePercent = prevPrice > 0 ? ((lastPrice - prevPrice) / prevPrice) * 100 : 0;
                
                var data = new BankApp.UI.Services.Pdf.InvestmentAnalysisData
                {
                    Symbol = _symbol ?? "UNKNOWN",
                    Name = _symbol ?? "UNKNOWN",
                    Timeframe = _timeframe ?? "1D",
                    LastPrice = lastPrice,
                    ChangePercent = changePercent,
                    ChangeAbsolute = lastPrice - prevPrice,
                    RSI = _showRSI ? "Enabled" : "N/A",
                    MACD = _showMACD ? "Enabled" : "N/A",
                    Signal = _showMACD ? "Enabled" : "N/A",
                    Volume = _showVolume ? "Enabled" : "N/A",
                    GeneratedAt = DateTime.Now
                };
                
                var path = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"{data.Symbol}_Chart_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                );
                
                using var report = new BankApp.UI.Reports.InvestmentAnalysisReport(data);
                report.ExportToPdf(path);
                
                DevExpress.XtraEditors.XtraMessageBox.Show($"PDF oluşturuldu:\n{path}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show($"PDF Hatası:\n{ex.GetType().Name}\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private double CalculateEMA(int period)
        {
            if (_candles.Count < period) return 0;
            double multiplier = 2.0 / (period + 1);
            double ema = _candles[0].Close;
            for (int i = 1; i < _candles.Count; i++)
                ema = (_candles[i].Close - ema) * multiplier + ema;
            return ema;
        }
        
        #endregion
        
        #region Keyboard
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_isDrawing)
                {
                    CancelActiveDrawing();
                    e.Handled = true;
                }
                else
                {
                    // Return to select tool
                    _toolButtons["select"].PerformClick();
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelected();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                Redo();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.V) _toolButtons["select"].PerformClick();
            else if (e.KeyCode == Keys.C) _toolButtons["crosshair"].PerformClick();
            else if (e.KeyCode == Keys.T) _toolButtons["trendline"].PerformClick();
            else if (e.KeyCode == Keys.H) _toolButtons["hline"].PerformClick();
            else if (e.KeyCode == Keys.R) _toolButtons["ray"].PerformClick();
            else if (e.KeyCode == Keys.S)
            {
                _snapToOHLC = !_snapToOHLC;
                _toolButtons["snap"].Appearance.BackColor = _snapToOHLC ? Color.FromArgb(33, 150, 243) : Color.FromArgb(35, 35, 35);
            }
        }
        
        #endregion
        
        #region Crosshair Tooltip
        
        private void Chart_CustomDrawCrosshair(object sender, CustomDrawCrosshairEventArgs e)
        {
            // Enhanced crosshair tooltip handled by DevExpress
        }
        
        #endregion
        
        #region Drawing Object Class
        
        private class ChartDrawingObject
        {
            public string Type { get; set; }
            public DateTime StartTime { get; set; }
            public double StartPrice { get; set; }
            public DateTime EndTime { get; set; }
            public double EndPrice { get; set; }
            public Color Color { get; set; }
            public string Text { get; set; }
            
            public ChartDrawingObject Clone()
            {
                return new ChartDrawingObject
                {
                    Type = this.Type,
                    StartTime = this.StartTime,
                    StartPrice = this.StartPrice,
                    EndTime = this.EndTime,
                    EndPrice = this.EndPrice,
                    Color = this.Color,
                    Text = this.Text
                };
            }
        }
        
        #endregion
    }
}
