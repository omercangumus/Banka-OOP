using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.Utils;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// Enhanced Asset Card Control for Investment Dashboard V2
    /// Displays asset summary with sparkline chart, price, and change indicators
    /// </summary>
    public class AssetCardControl : XtraUserControl
    {
        private PanelControl pnlMain;
        private LabelControl lblSymbol;
        private LabelControl lblDisplayName;
        private LabelControl lblCurrentPrice;
        private LabelControl lblChange;
        private LabelControl lblChangePercent;
        private LabelControl lblPriceDirection;
        private ChartControl chartSparkline;
        private PictureEdit picLoading;
        
        private MarketData _marketData;
        private bool _isLoading;

        public event EventHandler<string> AssetCardClicked;

        public string Symbol { get; private set; }
        public MarketData MarketData => _marketData;
        public bool IsLoading 
        { 
            get => _isLoading; 
            set => SetLoadingState(value); 
        }

        public AssetCardControl()
        {
            InitializeUI();
        }

        public AssetCardControl(string symbol) : this()
        {
            Symbol = symbol;
            lblSymbol.Text = symbol;
        }

        private void InitializeUI()
        {
            this.Size = new Size(280, 160);
            this.Appearance.BackColor = Color.FromArgb(30, 41, 59); // Dark card background
            this.Appearance.Options.UseBackColor = true;
            this.Cursor = Cursors.Hand;

            // Main panel
            pnlMain = new PanelControl();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMain.Appearance.BackColor = Color.Transparent;
            pnlMain.Appearance.Options.UseBackColor = true;
            this.Controls.Add(pnlMain);

            // Symbol label (top-left)
            lblSymbol = new LabelControl();
            lblSymbol.Text = "SYMBOL";
            lblSymbol.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSymbol.Appearance.ForeColor = Color.FromArgb(226, 232, 240); // Light text
            lblSymbol.Location = new Point(10, 8);
            lblSymbol.AutoSizeMode = LabelAutoSizeMode.None;
            lblSymbol.Size = new Size(100, 20);
            pnlMain.Controls.Add(lblSymbol);

            // Display name (below symbol)
            lblDisplayName = new LabelControl();
            lblDisplayName.Text = "Asset Name";
            lblDisplayName.Appearance.Font = new Font("Segoe UI", 8F);
            lblDisplayName.Appearance.ForeColor = Color.FromArgb(148, 163, 184); // Muted text
            lblDisplayName.Location = new Point(10, 28);
            lblDisplayName.AutoSizeMode = LabelAutoSizeMode.None;
            lblDisplayName.Size = new Size(150, 15);
            pnlMain.Controls.Add(lblDisplayName);

            // Current price (large, center-left)
            lblCurrentPrice = new LabelControl();
            lblCurrentPrice.Text = "$0.00";
            lblCurrentPrice.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblCurrentPrice.Appearance.ForeColor = Color.FromArgb(226, 232, 240);
            lblCurrentPrice.Location = new Point(10, 50);
            lblCurrentPrice.AutoSizeMode = LabelAutoSizeMode.None;
            lblCurrentPrice.Size = new Size(120, 25);
            pnlMain.Controls.Add(lblCurrentPrice);

            // Price direction indicator
            lblPriceDirection = new LabelControl();
            lblPriceDirection.Text = "→";
            lblPriceDirection.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblPriceDirection.Appearance.ForeColor = Color.Gray;
            lblPriceDirection.Location = new Point(135, 52);
            lblPriceDirection.AutoSizeMode = LabelAutoSizeMode.None;
            lblPriceDirection.Size = new Size(20, 20);
            pnlMain.Controls.Add(lblPriceDirection);

            // Change amount
            lblChange = new LabelControl();
            lblChange.Text = "+0.00";
            lblChange.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblChange.Appearance.ForeColor = Color.Green;
            lblChange.Location = new Point(10, 78);
            lblChange.AutoSizeMode = LabelAutoSizeMode.None;
            lblChange.Size = new Size(60, 15);
            pnlMain.Controls.Add(lblChange);

            // Change percentage
            lblChangePercent = new LabelControl();
            lblChangePercent.Text = "(+0.00%)";
            lblChangePercent.Appearance.Font = new Font("Segoe UI", 9F);
            lblChangePercent.Appearance.ForeColor = Color.Green;
            lblChangePercent.Location = new Point(75, 78);
            lblChangePercent.AutoSizeMode = LabelAutoSizeMode.None;
            lblChangePercent.Size = new Size(80, 15);
            pnlMain.Controls.Add(lblChangePercent);

            // Sparkline chart (right side)
            chartSparkline = CreateSparklineChart();
            chartSparkline.Location = new Point(160, 45);
            chartSparkline.Size = new Size(110, 50);
            pnlMain.Controls.Add(chartSparkline);

            // Loading indicator
            picLoading = new PictureEdit();
            picLoading.Location = new Point(120, 60);
            picLoading.Size = new Size(32, 32);
            picLoading.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Never;
            picLoading.Properties.ShowMenu = false;
            picLoading.Properties.ReadOnly = true;
            picLoading.Visible = false;
            pnlMain.Controls.Add(picLoading);

            // Click events
            this.Click += OnCardClick;
            pnlMain.Click += OnCardClick;
            lblSymbol.Click += OnCardClick;
            lblDisplayName.Click += OnCardClick;
            lblCurrentPrice.Click += OnCardClick;
            chartSparkline.Click += OnCardClick;

            // Hover effects
            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
            pnlMain.MouseEnter += OnMouseEnter;
            pnlMain.MouseLeave += OnMouseLeave;
        }

        private ChartControl CreateSparklineChart()
        {
            var chart = new ChartControl();
            chart.BorderOptions.Visibility = DefaultBoolean.False;
            chart.Legend.Visibility = DefaultBoolean.False;
            chart.BackColor = Color.Transparent;

            // Create a dummy series to initialize XYDiagram
            var dummySeries = new Series("Dummy", ViewType.Line);
            chart.Series.Add(dummySeries);

            if (chart.Diagram is XYDiagram diagram)
            {
                diagram.AxisX.Visibility = DefaultBoolean.False;
                diagram.AxisY.Visibility = DefaultBoolean.False;
                diagram.DefaultPane.BackColor = Color.Transparent;
                diagram.DefaultPane.BorderVisible = false;
                diagram.AxisX.GridLines.Visible = false;
                diagram.AxisY.GridLines.Visible = false;
                diagram.Margins.All = 0;
            }

            chart.Series.Clear(); // Remove dummy series
            return chart;
        }

        public void UpdateData(MarketData data)
        {
            if (data == null) return;

            _marketData = data;
            Symbol = data.Symbol;

            // Update labels
            lblSymbol.Text = data.Symbol;
            lblDisplayName.Text = data.DisplayName;
            
            // Format price based on asset type
            string priceFormat = data.AssetType == AssetType.Forex ? "F5" : "F2";
            string currencySymbol = GetCurrencySymbol(data.AssetType);
            
            lblCurrentPrice.Text = $"{currencySymbol}{data.CurrentPrice.ToString(priceFormat)}";
            
            // Update change indicators
            UpdateChangeIndicators(data);
            
            // Update sparkline chart
            UpdateSparklineChart(data);
            
            // Hide loading indicator
            SetLoadingState(false);
        }

        private void UpdateChangeIndicators(MarketData data)
        {
            var changeColor = data.IsPriceUp ? Color.FromArgb(34, 197, 94) : // Green
                             data.IsPriceDown ? Color.FromArgb(239, 68, 68) : // Red
                             Color.Gray;

            string changePrefix = data.Change >= 0 ? "+" : "";
            string percentPrefix = data.ChangePercent >= 0 ? "+" : "";

            lblChange.Text = $"{changePrefix}{data.Change:F2}";
            lblChange.Appearance.ForeColor = changeColor;

            lblChangePercent.Text = $"({percentPrefix}{data.ChangePercent:F2}%)";
            lblChangePercent.Appearance.ForeColor = changeColor;

            lblPriceDirection.Text = data.PriceDirection;
            lblPriceDirection.Appearance.ForeColor = changeColor;
        }

        private void UpdateSparklineChart(MarketData data)
        {
            if (data.HistoricalData == null || data.HistoricalData.Length == 0)
            {
                return;
            }

            chartSparkline.Series.Clear();
            
            var series = new Series("Price", ViewType.Line);
            var lineView = (LineSeriesView)series.View;
            lineView.LineStyle.Thickness = 2;
            lineView.Color = data.IsPriceUp ? Color.FromArgb(34, 197, 94) : 
                            data.IsPriceDown ? Color.FromArgb(239, 68, 68) : 
                            Color.FromArgb(59, 130, 246);

            // Add last 24 hours of data (or available data)
            var recentData = data.HistoricalData.TakeLast(24).ToArray();
            
            for (int i = 0; i < recentData.Length; i++)
            {
                series.Points.Add(new SeriesPoint(i, recentData[i].Close));
            }

            chartSparkline.Series.Add(series);

            if (chartSparkline.Diagram is XYDiagram diagram)
            {
                diagram.AxisY.WholeRange.AlwaysShowZeroLevel = false;
            }
        }

        public void SetLoadingState(bool loading)
        {
            _isLoading = loading;
            picLoading.Visible = loading;
            
            if (loading)
            {
                lblCurrentPrice.Text = "Loading...";
                lblChange.Text = "";
                lblChangePercent.Text = "";
                lblPriceDirection.Text = "";
                chartSparkline.Series.Clear();
            }
        }

        private string GetCurrencySymbol(AssetType assetType)
        {
            return assetType switch
            {
                AssetType.Stock => "$",
                AssetType.Crypto => "$",
                AssetType.Commodity => "$",
                AssetType.Forex => "",
                _ => "$"
            };
        }

        private void OnCardClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Symbol))
            {
                AssetCardClicked?.Invoke(this, Symbol);
            }
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            this.Appearance.BackColor = Color.FromArgb(51, 65, 85); // Lighter on hover
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            this.Appearance.BackColor = Color.FromArgb(30, 41, 59); // Original color
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up event handlers
                this.Click -= OnCardClick;
                this.MouseEnter -= OnMouseEnter;
                this.MouseLeave -= OnMouseLeave;
            }
            base.Dispose(disposing);
        }
    }
}