using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    public class FullscreenChartForm : Form
    {
        private readonly IMarketDataProvider _dataProvider;
        private string _symbol;
        private string _timeframe = "1D";
        
        private Panel pnlToolbar;
        private ChartControl chartMain;
        private Panel pnlHeader;
        
        // Drawing tools
        private string _currentTool = "crosshair";
        private List<ChartDrawing> _drawings = new List<ChartDrawing>();
        private Point? _drawStart;
        
        // Tool buttons
        private SimpleButton btnCrosshair, btnTrendline, btnHLine, btnRectangle, btnFibonacci, btnText, btnClearAll;
        private SimpleButton btnTf1m, btnTf5m, btnTf15m, btnTf1H, btnTf4H, btnTf1D, btnTf1W;
        private LabelControl lblSymbol;
        private SimpleButton btnClose;
        
        // Indicators
        private CheckEdit chkMA20, chkMA50, chkEMA12, chkEMA26, chkBollinger;
        private bool _showMA20, _showMA50, _showEMA12, _showEMA26, _showBollinger;
        
        // Context menu
        private ContextMenuStrip ctxMenu;
        
        // Drawing state
        private ChartDrawing _selectedDrawing = null;
        private bool _isDragging = false;
        private Point _dragOffset;
        
        public FullscreenChartForm(IMarketDataProvider dataProvider, string symbol, string timeframe)
        {
            _dataProvider = dataProvider;
            _symbol = symbol;
            _timeframe = timeframe;
            
            InitializeComponents();
            this.KeyPreview = true;
            this.KeyDown += FullscreenChartForm_KeyDown;
        }
        
        private void FullscreenChartForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
        
        private void InitializeComponents()
        {
            this.Text = $"Grafik Analiz - {_symbol}";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(14, 14, 14);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Header
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 45;
            pnlHeader.BackColor = Color.FromArgb(22, 22, 22);
            
            lblSymbol = new LabelControl();
            lblSymbol.Text = $"📈 {_symbol} - Grafik Analiz Modu";
            lblSymbol.Appearance.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblSymbol.Appearance.ForeColor = Color.White;
            lblSymbol.Location = new Point(15, 10);
            pnlHeader.Controls.Add(lblSymbol);
            
            // Timeframe buttons in header
            int tfX = 350;
            btnTf1m = CreateTfButton("1m", tfX); tfX += 50;
            btnTf5m = CreateTfButton("5m", tfX); tfX += 50;
            btnTf15m = CreateTfButton("15m", tfX); tfX += 55;
            btnTf1H = CreateTfButton("1H", tfX); tfX += 50;
            btnTf4H = CreateTfButton("4H", tfX); tfX += 50;
            btnTf1D = CreateTfButton("1D", tfX); tfX += 50;
            btnTf1W = CreateTfButton("1W", tfX);
            
            pnlHeader.Controls.Add(btnTf1m);
            pnlHeader.Controls.Add(btnTf5m);
            pnlHeader.Controls.Add(btnTf15m);
            pnlHeader.Controls.Add(btnTf1H);
            pnlHeader.Controls.Add(btnTf4H);
            pnlHeader.Controls.Add(btnTf1D);
            pnlHeader.Controls.Add(btnTf1W);
            
            // Indicator checkboxes
            chkMA20 = CreateIndicatorCheck("MA20", tfX + 80);
            chkMA50 = CreateIndicatorCheck("MA50", tfX + 150);
            chkEMA12 = CreateIndicatorCheck("EMA12", tfX + 220);
            chkEMA26 = CreateIndicatorCheck("EMA26", tfX + 295);
            chkBollinger = CreateIndicatorCheck("BB", tfX + 370);
            pnlHeader.Controls.Add(chkMA20);
            pnlHeader.Controls.Add(chkMA50);
            pnlHeader.Controls.Add(chkEMA12);
            pnlHeader.Controls.Add(chkEMA26);
            pnlHeader.Controls.Add(chkBollinger);
            
            // Close button
            btnClose = new SimpleButton();
            btnClose.Text = "✕ Kapat (ESC)";
            btnClose.Size = new Size(120, 30);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(this.Width - 140, 8);
            btnClose.Appearance.BackColor = Color.FromArgb(180, 50, 50);
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Appearance.Options.UseForeColor = true;
            btnClose.Click += (s, e) => this.Close();
            pnlHeader.Controls.Add(btnClose);
            
            // Left toolbar
            pnlToolbar = new Panel();
            pnlToolbar.Dock = DockStyle.Left;
            pnlToolbar.Width = 50;
            pnlToolbar.BackColor = Color.FromArgb(22, 22, 22);
            
            int toolY = 10;
            btnCrosshair = CreateToolButton("╋", "crosshair", toolY); toolY += 45;
            btnTrendline = CreateToolButton("╱", "trendline", toolY); toolY += 45;
            btnHLine = CreateToolButton("─", "hline", toolY); toolY += 45;
            btnRectangle = CreateToolButton("▢", "rectangle", toolY); toolY += 45;
            btnFibonacci = CreateToolButton("ƒ", "fibonacci", toolY); toolY += 45;
            btnText = CreateToolButton("T", "text", toolY); toolY += 60;
            btnClearAll = CreateToolButton("🗑", "clear", toolY);
            btnClearAll.Click -= ToolButton_Click;
            btnClearAll.Click += (s, e) => { _drawings.Clear(); chartMain.Invalidate(); };
            
            pnlToolbar.Controls.Add(btnCrosshair);
            pnlToolbar.Controls.Add(btnTrendline);
            pnlToolbar.Controls.Add(btnHLine);
            pnlToolbar.Controls.Add(btnRectangle);
            pnlToolbar.Controls.Add(btnFibonacci);
            pnlToolbar.Controls.Add(btnText);
            pnlToolbar.Controls.Add(btnClearAll);
            
            // Context menu
            ctxMenu = new ContextMenuStrip();
            ctxMenu.BackColor = Color.FromArgb(30, 30, 30);
            ctxMenu.ForeColor = Color.White;
            ctxMenu.Items.Add("📝 Not Ekle", null, (s, ev) => { _currentTool = "text"; ToolButton_Click(btnText, EventArgs.Empty); });
            ctxMenu.Items.Add("━ Destek Çizgisi", null, (s, ev) => { _currentTool = "hline"; ToolButton_Click(btnHLine, EventArgs.Empty); });
            ctxMenu.Items.Add("╱ Direnç Çizgisi", null, (s, ev) => { _currentTool = "trendline"; ToolButton_Click(btnTrendline, EventArgs.Empty); });
            ctxMenu.Items.Add("ƒ Fibonacci", null, (s, ev) => { _currentTool = "fibonacci"; ToolButton_Click(btnFibonacci, EventArgs.Empty); });
            ctxMenu.Items.Add(new ToolStripSeparator());
            ctxMenu.Items.Add("🗑 Çizimleri Temizle", null, (s, ev) => { _drawings.Clear(); chartMain.Invalidate(); });
            
            // Chart
            chartMain = new ChartControl();
            chartMain.Dock = DockStyle.Fill;
            chartMain.BackColor = Color.FromArgb(14, 14, 14);
            chartMain.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chartMain.RuntimeHitTesting = true;
            chartMain.ContextMenuStrip = ctxMenu;
            chartMain.CustomPaint += ChartMain_CustomPaint;
            chartMain.MouseDown += ChartMain_MouseDown;
            chartMain.MouseUp += ChartMain_MouseUp;
            chartMain.MouseMove += ChartMain_MouseMove;
            
            // Add controls
            this.Controls.Add(chartMain);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(pnlHeader);
            
            // Highlight current timeframe
            HighlightTimeframe(_timeframe);
            
            // Load chart
            LoadChartAsync();
        }
        
        private SimpleButton CreateTfButton(string text, int x)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Size = new Size(45, 28);
            btn.Location = new Point(x, 8);
            btn.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Click += TfButton_Click;
            return btn;
        }
        
        private CheckEdit CreateIndicatorCheck(string text, int x)
        {
            var chk = new CheckEdit();
            chk.Text = text;
            chk.Size = new Size(70, 28);
            chk.Location = new Point(x, 10);
            chk.Properties.Appearance.ForeColor = Color.FromArgb(180, 180, 180);
            chk.Properties.Appearance.BackColor = Color.Transparent;
            chk.CheckedChanged += (s, e) => {
                if (chk == chkMA20) _showMA20 = chk.Checked;
                else if (chk == chkMA50) _showMA50 = chk.Checked;
                else if (chk == chkEMA12) _showEMA12 = chk.Checked;
                else if (chk == chkEMA26) _showEMA26 = chk.Checked;
                else if (chk == chkBollinger) _showBollinger = chk.Checked;
                LoadChartAsync();
            };
            return chk;
        }
        
        private SimpleButton CreateToolButton(string text, string tool, int y)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Tag = tool;
            btn.Size = new Size(40, 40);
            btn.Location = new Point(5, y);
            btn.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.Font = new Font("Segoe UI", 14F);
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Appearance.Options.UseFont = true;
            btn.Click += ToolButton_Click;
            return btn;
        }
        
        private void ToolButton_Click(object sender, EventArgs e)
        {
            var btn = sender as SimpleButton;
            if (btn == null) return;
            
            _currentTool = btn.Tag?.ToString() ?? "crosshair";
            
            // Reset all tool button colors
            btnCrosshair.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTrendline.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnHLine.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnRectangle.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnFibonacci.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnText.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            
            btn.Appearance.BackColor = Color.FromArgb(33, 150, 243);
        }
        
        private void TfButton_Click(object sender, EventArgs e)
        {
            var btn = sender as SimpleButton;
            if (btn == null) return;
            
            _timeframe = btn.Text;
            HighlightTimeframe(_timeframe);
            LoadChartAsync();
        }
        
        private void HighlightTimeframe(string tf)
        {
            btnTf1m.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf5m.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf15m.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf1H.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf4H.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf1D.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnTf1W.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            
            var btn = tf switch {
                "1m" => btnTf1m,
                "5m" => btnTf5m,
                "15m" => btnTf15m,
                "1H" => btnTf1H,
                "4H" => btnTf4H,
                "1D" => btnTf1D,
                "1W" => btnTf1W,
                _ => btnTf1D
            };
            btn.Appearance.BackColor = Color.FromArgb(33, 150, 243);
        }
        
        private async void LoadChartAsync()
        {
            try
            {
                var tfMap = _timeframe switch {
                    "1m" => "1", "5m" => "5", "15m" => "15",
                    "1H" => "60", "4H" => "240", "1D" => "D", "1W" => "W", _ => "D"
                };
                
                var candles = await _dataProvider.GetCandlesAsync(_symbol, tfMap, 200);
                RenderChart(candles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Grafik yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void RenderChart(List<MarketCandle> candles)
        {
            if (candles == null || candles.Count == 0) return;
            
            chartMain.Series.Clear();
            
            // Candlestick series
            var series = new Series(_symbol, ViewType.CandleStick);
            series.ArgumentScaleType = ScaleType.DateTime;
            
            var closes = new List<double>();
            foreach (var c in candles)
            {
                series.Points.Add(new SeriesPoint(c.Time, c.Low, c.High, c.Open, c.Close));
                closes.Add(c.Close);
            }
            
            var view = (CandleStickSeriesView)series.View;
            view.Color = Color.FromArgb(38, 166, 91);
            view.ReductionOptions.Color = Color.FromArgb(232, 65, 66);
            view.ReductionOptions.Level = StockLevel.Close;
            view.ReductionOptions.Visible = true;
            view.LineThickness = 2;
            view.LevelLineLength = 0.6;
            
            chartMain.Series.Add(series);
            
            // Add MA/EMA indicators
            if (_showMA20) AddMovingAverage(candles, 20, Color.FromArgb(255, 193, 7), "MA20");
            if (_showMA50) AddMovingAverage(candles, 50, Color.FromArgb(156, 39, 176), "MA50");
            if (_showEMA12) AddEMA(candles, 12, Color.FromArgb(0, 188, 212), "EMA12");
            if (_showEMA26) AddEMA(candles, 26, Color.FromArgb(255, 152, 0), "EMA26");
            if (_showBollinger) AddBollingerBands(candles, 20, 2);
            
            // Configure diagram
            var diagram = chartMain.Diagram as XYDiagram;
            if (diagram != null)
            {
                diagram.EnableAxisXZooming = true;
                diagram.EnableAxisYZooming = true;
                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisYScrolling = true;
                diagram.ZoomingOptions.UseKeyboard = true;
                diagram.ZoomingOptions.UseMouseWheel = true;
                
                diagram.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Day;
                diagram.AxisX.Label.TextPattern = "{A:MM/dd HH:mm}";
                diagram.AxisX.Color = Color.FromArgb(50, 50, 50);
                diagram.AxisY.Color = Color.FromArgb(50, 50, 50);
                diagram.AxisX.GridLines.Color = Color.FromArgb(35, 35, 35);
                diagram.AxisY.GridLines.Color = Color.FromArgb(35, 35, 35);
                diagram.AxisX.GridLines.Visible = true;
                diagram.AxisY.GridLines.Visible = true;
                diagram.AxisX.Label.Font = new Font("Segoe UI", 9F);
                diagram.AxisY.Label.Font = new Font("Segoe UI", 9F);
                diagram.AxisX.Label.TextColor = Color.FromArgb(150, 150, 150);
                diagram.AxisY.Label.TextColor = Color.FromArgb(150, 150, 150);
                diagram.DefaultPane.BackColor = Color.FromArgb(14, 14, 14);
            }
            
            // Crosshair on ChartControl
            chartMain.CrosshairEnabled = DevExpress.Utils.DefaultBoolean.True;
            chartMain.CrosshairOptions.ShowArgumentLabels = true;
            chartMain.CrosshairOptions.ShowValueLabels = true;
            chartMain.CrosshairOptions.ShowArgumentLine = true;
            chartMain.CrosshairOptions.ShowValueLine = true;
            chartMain.CrosshairOptions.CrosshairLabelMode = CrosshairLabelMode.ShowForEachSeries;
            chartMain.CrosshairOptions.ArgumentLineColor = Color.FromArgb(100, 100, 100);
            chartMain.CrosshairOptions.ValueLineColor = Color.FromArgb(100, 100, 100);
        }
        
        private void AddMovingAverage(List<MarketCandle> candles, int period, Color color, string name)
        {
            if (candles.Count < period) return;
            
            var maSeries = new Series(name, ViewType.Line);
            maSeries.ArgumentScaleType = ScaleType.DateTime;
            
            for (int i = period - 1; i < candles.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < period; j++) sum += candles[i - j].Close;
                maSeries.Points.Add(new SeriesPoint(candles[i].Time, sum / period));
            }
            
            var lineView = (LineSeriesView)maSeries.View;
            lineView.Color = color;
            lineView.LineStyle.Thickness = 2;
            
            chartMain.Series.Add(maSeries);
        }
        
        private void AddEMA(List<MarketCandle> candles, int period, Color color, string name)
        {
            if (candles.Count < period) return;
            
            var emaSeries = new Series(name, ViewType.Line);
            emaSeries.ArgumentScaleType = ScaleType.DateTime;
            
            double multiplier = 2.0 / (period + 1);
            double ema = candles[0].Close;
            
            for (int i = 0; i < candles.Count; i++)
            {
                ema = (candles[i].Close - ema) * multiplier + ema;
                if (i >= period - 1)
                    emaSeries.Points.Add(new SeriesPoint(candles[i].Time, ema));
            }
            
            var lineView = (LineSeriesView)emaSeries.View;
            lineView.Color = color;
            lineView.LineStyle.Thickness = 2;
            
            chartMain.Series.Add(emaSeries);
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
        
        private void ChartMain_CustomPaint(object sender, CustomPaintEventArgs e)
        {
            foreach (var drawing in _drawings)
            {
                using var pen = new Pen(drawing.Color, 2);
                switch (drawing.Type)
                {
                    case "trendline":
                        e.Graphics.DrawLine(pen, drawing.Start, drawing.End);
                        break;
                    case "hline":
                        e.Graphics.DrawLine(pen, new Point(0, drawing.Start.Y), new Point(chartMain.Width, drawing.Start.Y));
                        break;
                    case "rectangle":
                        var rect = new Rectangle(
                            Math.Min(drawing.Start.X, drawing.End.X),
                            Math.Min(drawing.Start.Y, drawing.End.Y),
                            Math.Abs(drawing.End.X - drawing.Start.X),
                            Math.Abs(drawing.End.Y - drawing.Start.Y));
                        e.Graphics.DrawRectangle(pen, rect);
                        break;
                }
            }
        }
        
        private void ChartMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _currentTool != "crosshair")
            {
                _drawStart = e.Location;
            }
        }
        
        private void ChartMain_MouseUp(object sender, MouseEventArgs e)
        {
            if (_drawStart.HasValue && _currentTool != "crosshair")
            {
                _drawings.Add(new ChartDrawing
                {
                    Type = _currentTool,
                    Start = _drawStart.Value,
                    End = e.Location,
                    Color = Color.FromArgb(33, 150, 243)
                });
                _drawStart = null;
                chartMain.Invalidate();
            }
        }
        
        private void ChartMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (_drawStart.HasValue)
            {
                chartMain.Invalidate();
            }
        }
        
        private class ChartDrawing
        {
            public string Type { get; set; }
            public Point Start { get; set; }
            public Point End { get; set; }
            public Color Color { get; set; }
        }
    }
}
