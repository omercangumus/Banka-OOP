using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraCharts;
using DevExpress.LookAndFeel;

namespace BankApp.UI.Forms
{
    public partial class StockMarketForm : XtraForm
    {
        private GridControl gridStocks;
        private GridView gridViewStocks;
        private ChartControl chartStock;
        private PanelControl pnlBottom;
        private SpinEdit spinQuantity;
        private SimpleButton btnBuy;
        private SimpleButton btnSell;
        private LabelControl lblTitle;
        private LabelControl lblSelectedStock;

        public StockMarketForm()
        {
            InitializeComponent();
            LoadStockData();
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            this.gridStocks = new GridControl();
            this.gridViewStocks = new GridView();
            this.chartStock = new ChartControl();
            this.pnlBottom = new PanelControl();
            this.spinQuantity = new SpinEdit();
            this.btnBuy = new SimpleButton();
            this.btnSell = new SimpleButton();
            this.lblTitle = new LabelControl();
            this.lblSelectedStock = new LabelControl();

            ((System.ComponentModel.ISupportInitialize)(this.gridStocks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewStocks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQuantity.Properties)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // Title
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.White;
            this.lblTitle.Location = new Point(20, 15);
            this.lblTitle.Text = "📈 Borsa - Hisse Senedi İşlemleri";
            this.lblTitle.Name = "lblTitle";

            // Grid Stocks - Sol Panel
            this.gridStocks.Location = new Point(20, 60);
            this.gridStocks.Size = new Size(400, 450);
            this.gridStocks.MainView = this.gridViewStocks;
            this.gridStocks.Name = "gridStocks";
            this.gridStocks.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gridViewStocks });

            this.gridViewStocks.GridControl = this.gridStocks;
            this.gridViewStocks.Name = "gridViewStocks";
            this.gridViewStocks.OptionsView.ShowGroupPanel = false;
            this.gridViewStocks.OptionsBehavior.Editable = false;
            this.gridViewStocks.FocusedRowChanged += GridViewStocks_FocusedRowChanged;
            this.gridViewStocks.RowStyle += GridViewStocks_RowStyle;

            // Chart - Sağ Panel (CandleStick)
            this.chartStock.Location = new Point(440, 60);
            this.chartStock.Size = new Size(540, 450);
            this.chartStock.Name = "chartStock";
            this.chartStock.BackColor = Color.FromArgb(30, 30, 30);
            this.chartStock.AppearanceNameSerializable = "Dark Chameleon";

            // Selected Stock Label
            this.lblSelectedStock.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblSelectedStock.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            this.lblSelectedStock.Location = new Point(440, 520);
            this.lblSelectedStock.Text = "Seçili Hisse: -";
            this.lblSelectedStock.Name = "lblSelectedStock";

            // Bottom Panel
            this.pnlBottom.Location = new Point(20, 550);
            this.pnlBottom.Size = new Size(960, 80);
            this.pnlBottom.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            this.pnlBottom.Appearance.Options.UseBackColor = true;
            this.pnlBottom.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlBottom.Name = "pnlBottom";

            // Spin Edit - Adet
            var lblQuantity = new LabelControl();
            lblQuantity.Appearance.ForeColor = Color.White;
            lblQuantity.Location = new Point(20, 28);
            lblQuantity.Text = "Adet:";
            this.pnlBottom.Controls.Add(lblQuantity);

            this.spinQuantity.Location = new Point(70, 25);
            this.spinQuantity.Size = new Size(120, 30);
            this.spinQuantity.Properties.MinValue = 1;
            this.spinQuantity.Properties.MaxValue = 10000;
            this.spinQuantity.Value = 1;
            this.spinQuantity.Name = "spinQuantity";

            // Buy Button
            this.btnBuy.Location = new Point(700, 20);
            this.btnBuy.Size = new Size(120, 40);
            this.btnBuy.Text = "AL";
            this.btnBuy.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnBuy.Appearance.ForeColor = Color.White;
            this.btnBuy.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnBuy.Appearance.Options.UseBackColor = true;
            this.btnBuy.Appearance.Options.UseForeColor = true;
            this.btnBuy.Appearance.Options.UseFont = true;
            this.btnBuy.Name = "btnBuy";
            this.btnBuy.Click += BtnBuy_Click;

            // Sell Button
            this.btnSell.Location = new Point(830, 20);
            this.btnSell.Size = new Size(120, 40);
            this.btnSell.Text = "SAT";
            this.btnSell.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            this.btnSell.Appearance.ForeColor = Color.White;
            this.btnSell.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnSell.Appearance.Options.UseBackColor = true;
            this.btnSell.Appearance.Options.UseForeColor = true;
            this.btnSell.Appearance.Options.UseFont = true;
            this.btnSell.Name = "btnSell";
            this.btnSell.Click += BtnSell_Click;

            this.pnlBottom.Controls.Add(this.spinQuantity);
            this.pnlBottom.Controls.Add(this.btnBuy);
            this.pnlBottom.Controls.Add(this.btnSell);

            // Form
            this.ClientSize = new Size(1000, 650);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.gridStocks);
            this.Controls.Add(this.chartStock);
            this.Controls.Add(this.lblSelectedStock);
            this.Controls.Add(this.pnlBottom);
            this.Name = "StockMarketForm";
            this.Text = "Borsa - Hisse Senedi";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            ((System.ComponentModel.ISupportInitialize)(this.gridStocks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewStocks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQuantity.Properties)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadStockData()
        {
            var stocks = new List<StockData>
            {
                new StockData { Symbol = "THYAO", Name = "Türk Hava Yolları", Price = 285.50m, Change = 2.35m, Volume = 125000000 },
                new StockData { Symbol = "GARAN", Name = "Garanti Bankası", Price = 95.20m, Change = -1.15m, Volume = 98000000 },
                new StockData { Symbol = "ASELS", Name = "Aselsan", Price = 52.80m, Change = 4.20m, Volume = 45000000 },
                new StockData { Symbol = "SISE", Name = "Şişecam", Price = 48.90m, Change = -0.85m, Volume = 32000000 },
                new StockData { Symbol = "EREGL", Name = "Ereğli Demir Çelik", Price = 56.40m, Change = 1.50m, Volume = 78000000 },
                new StockData { Symbol = "KCHOL", Name = "Koç Holding", Price = 175.60m, Change = 0.95m, Volume = 55000000 },
                new StockData { Symbol = "AKBNK", Name = "Akbank", Price = 48.75m, Change = -2.10m, Volume = 88000000 },
                new StockData { Symbol = "TUPRS", Name = "Tüpraş", Price = 165.30m, Change = 3.45m, Volume = 42000000 },
                new StockData { Symbol = "SAHOL", Name = "Sabancı Holding", Price = 78.90m, Change = 1.25m, Volume = 61000000 },
                new StockData { Symbol = "BIMAS", Name = "BİM", Price = 395.00m, Change = -0.50m, Volume = 28000000 }
            };

            gridStocks.DataSource = stocks;
            
            // İlk hisseyi seç
            if (stocks.Count > 0)
            {
                LoadCandleStickChart(stocks[0].Symbol);
            }
        }

        private void GridViewStocks_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var row = gridViewStocks.GetFocusedRow() as StockData;
            if (row != null)
            {
                lblSelectedStock.Text = $"Seçili Hisse: {row.Symbol} - {row.Name} | Fiyat: {row.Price:N2} TL";
                LoadCandleStickChart(row.Symbol);
            }
        }

        private void GridViewStocks_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                var row = gridViewStocks.GetRow(e.RowHandle) as StockData;
                if (row != null)
                {
                    // Değişim pozitifse yeşil, negatifse kırmızı arka plan
                    if (row.Change > 0)
                        e.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
                    else if (row.Change < 0)
                        e.Appearance.ForeColor = Color.FromArgb(244, 67, 54);
                }
            }
        }

        private void LoadCandleStickChart(string symbol)
        {
            chartStock.Series.Clear();
            
            // Simüle edilmiş OHLC verisi
            var random = new Random(symbol.GetHashCode());
            var series = new Series(symbol, ViewType.CandleStick);
            
            var basePrice = random.Next(50, 200);
            DateTime startDate = DateTime.Now.AddDays(-30);
            
            for (int i = 0; i < 30; i++)
            {
                var open = basePrice + random.NextDouble() * 10 - 5;
                var high = open + random.NextDouble() * 5;
                var low = open - random.NextDouble() * 5;
                var close = low + random.NextDouble() * (high - low);
                
                series.Points.Add(new SeriesPoint(startDate.AddDays(i), new double[] { high, low, open, close }));
                basePrice = (int)close;
            }

            var candleView = (CandleStickSeriesView)series.View;
            candleView.Color = Color.FromArgb(76, 175, 80);
            candleView.ReductionOptions.Color = Color.FromArgb(244, 67, 54);
            candleView.ReductionOptions.Visible = true;
            candleView.LineThickness = 2;

            chartStock.Series.Add(series);
            chartStock.Titles.Clear();
            chartStock.Titles.Add(new ChartTitle() { Text = $"{symbol} - Son 30 Gün", TextColor = Color.White });
        }

        private void BtnBuy_Click(object sender, EventArgs e)
        {
            var row = gridViewStocks.GetFocusedRow() as StockData;
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen bir hisse seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantity = (int)spinQuantity.Value;
            decimal total = row.Price * quantity;

            XtraMessageBox.Show(
                $"✅ ALIM EMRİ GÖNDERİLDİ\n\n" +
                $"Hisse: {row.Symbol}\n" +
                $"Adet: {quantity}\n" +
                $"Fiyat: {row.Price:N2} TL\n" +
                $"Toplam: {total:N2} TL",
                "Alım Emri", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSell_Click(object sender, EventArgs e)
        {
            var row = gridViewStocks.GetFocusedRow() as StockData;
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen bir hisse seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantity = (int)spinQuantity.Value;
            decimal total = row.Price * quantity;

            XtraMessageBox.Show(
                $"✅ SATIM EMRİ GÖNDERİLDİ\n\n" +
                $"Hisse: {row.Symbol}\n" +
                $"Adet: {quantity}\n" +
                $"Fiyat: {row.Price:N2} TL\n" +
                $"Toplam: {total:N2} TL",
                "Satım Emri", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // Stock Data Model
    public class StockData
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public long Volume { get; set; }
        
        public string ChangeDisplay => Change >= 0 ? $"+{Change:N2}%" : $"{Change:N2}%";
    }
}
