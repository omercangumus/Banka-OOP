using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraCharts;
using DevExpress.LookAndFeel;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    public partial class StockMarketForm : XtraForm
    {
        private GridControl gridStocks;
        private GridView gridViewStocks;
        private ChartControl chartStock;
        private PanelControl pnlOrderEntry;
        private LabelControl lblTitle;
        private LabelControl lblSelectedStock;
        private LabelControl lblCurrentPrice;
        
        // Order Entry Controls
        private TextEdit txtSymbol;
        private SpinEdit numQuantity;
        private CheckEdit chkStopLoss;
        private SpinEdit numStopLoss;
        private CheckEdit chkTakeProfit;
        private SpinEdit numTakeProfit;
        private SimpleButton btnBuy;
        private SimpleButton btnSell;
        private SimpleButton btnRefresh;
        
        private readonly StockService _stockService;
        private StockLiveData _currentStock;

        public StockMarketForm()
        {
            _stockService = new StockService();
            InitializeComponent();
            LoadStockData();
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            this.gridStocks = new GridControl();
            this.gridViewStocks = new GridView();
            this.chartStock = new ChartControl();
            this.pnlOrderEntry = new PanelControl();
            this.lblTitle = new LabelControl();
            this.lblSelectedStock = new LabelControl();
            this.lblCurrentPrice = new LabelControl();
            
            // Order entry controls
            this.txtSymbol = new TextEdit();
            this.numQuantity = new SpinEdit();
            this.chkStopLoss = new CheckEdit();
            this.numStopLoss = new SpinEdit();
            this.chkTakeProfit = new CheckEdit();
            this.numTakeProfit = new SpinEdit();
            this.btnBuy = new SimpleButton();
            this.btnSell = new SimpleButton();
            this.btnRefresh = new SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.gridStocks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewStocks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlOrderEntry)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSymbol.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantity.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStopLoss.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTakeProfit.Properties)).BeginInit();
            this.pnlOrderEntry.SuspendLayout();
            this.SuspendLayout();

            // ======= TITLE =======
            this.lblTitle.Appearance.Font = new Font("Tahoma", 14F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.White;
            this.lblTitle.Location = new Point(20, 15);
            this.lblTitle.Text = "📈 Profesyonel Borsa Platformu";
            this.lblTitle.Name = "lblTitle";

            // ======= GRID STOCKS - Sol Panel =======
            this.gridStocks.Location = new Point(20, 55);
            this.gridStocks.Size = new Size(350, 450);
            this.gridStocks.MainView = this.gridViewStocks;
            this.gridStocks.Name = "gridStocks";
            this.gridStocks.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gridViewStocks });

            this.gridViewStocks.GridControl = this.gridStocks;
            this.gridViewStocks.Name = "gridViewStocks";
            this.gridViewStocks.OptionsView.ShowGroupPanel = false;
            this.gridViewStocks.OptionsBehavior.Editable = false;
            this.gridViewStocks.Appearance.Row.Font = new Font("Tahoma", 8.25F);
            this.gridViewStocks.Appearance.HeaderPanel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.gridViewStocks.FocusedRowChanged += GridViewStocks_FocusedRowChanged;
            this.gridViewStocks.RowStyle += GridViewStocks_RowStyle;

            // ======= CHART - Orta Panel (Büyük CandleStick) =======
            this.chartStock.Location = new Point(385, 55);
            this.chartStock.Size = new Size(620, 450);
            this.chartStock.Name = "chartStock";
            this.chartStock.BackColor = Color.FromArgb(30, 30, 30);
            this.chartStock.AppearanceNameSerializable = "Dark Chameleon";

            // ======= SELECTED STOCK INFO =======
            this.lblSelectedStock.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.lblSelectedStock.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            this.lblSelectedStock.Location = new Point(385, 515);
            this.lblSelectedStock.Text = "Seçili Hisse: -";
            this.lblSelectedStock.Name = "lblSelectedStock";
            
            this.lblCurrentPrice.Appearance.Font = new Font("Tahoma", 9F);
            this.lblCurrentPrice.Appearance.ForeColor = Color.White;
            this.lblCurrentPrice.Location = new Point(385, 535);
            this.lblCurrentPrice.Text = "Güncel Fiyat: -";
            this.lblCurrentPrice.Name = "lblCurrentPrice";

            // ======= ORDER ENTRY PANEL - Sağ Panel =======
            this.pnlOrderEntry.Location = new Point(1020, 55);
            this.pnlOrderEntry.Size = new Size(280, 480);
            this.pnlOrderEntry.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            this.pnlOrderEntry.Appearance.Options.UseBackColor = true;
            this.pnlOrderEntry.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.pnlOrderEntry.Name = "pnlOrderEntry";

            // === Panel Header ===
            var lblPanelTitle = new LabelControl();
            lblPanelTitle.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            lblPanelTitle.Appearance.ForeColor = Color.White;
            lblPanelTitle.Location = new Point(15, 10);
            lblPanelTitle.Text = "📋 Emir Girişi";
            this.pnlOrderEntry.Controls.Add(lblPanelTitle);

            // === Symbol ===
            var lblSymbol = new LabelControl();
            lblSymbol.Appearance.Font = new Font("Tahoma", 8.25F);
            lblSymbol.Appearance.ForeColor = Color.LightGray;
            lblSymbol.Location = new Point(15, 45);
            lblSymbol.Text = "Sembol:";
            this.pnlOrderEntry.Controls.Add(lblSymbol);

            this.txtSymbol.Location = new Point(15, 65);
            this.txtSymbol.Size = new Size(250, 20);
            this.txtSymbol.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.txtSymbol.Properties.ReadOnly = true;
            this.txtSymbol.Name = "txtSymbol";
            this.pnlOrderEntry.Controls.Add(this.txtSymbol);

            // === Quantity ===
            var lblQuantity = new LabelControl();
            lblQuantity.Appearance.Font = new Font("Tahoma", 8.25F);
            lblQuantity.Appearance.ForeColor = Color.LightGray;
            lblQuantity.Location = new Point(15, 100);
            lblQuantity.Text = "Adet:";
            this.pnlOrderEntry.Controls.Add(lblQuantity);

            this.numQuantity.Location = new Point(15, 120);
            this.numQuantity.Size = new Size(250, 20);
            this.numQuantity.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.numQuantity.Properties.MinValue = 1;
            this.numQuantity.Properties.MaxValue = 100000;
            this.numQuantity.Value = 1;
            this.numQuantity.Name = "numQuantity";
            this.pnlOrderEntry.Controls.Add(this.numQuantity);

            // === Stop-Loss ===
            this.chkStopLoss.Location = new Point(15, 155);
            this.chkStopLoss.Size = new Size(250, 20);
            this.chkStopLoss.Text = "Stop-Loss Aktif";
            this.chkStopLoss.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.chkStopLoss.Properties.Appearance.ForeColor = Color.FromArgb(244, 67, 54);
            this.chkStopLoss.Properties.Appearance.Options.UseFont = true;
            this.chkStopLoss.Properties.Appearance.Options.UseForeColor = true;
            this.chkStopLoss.Name = "chkStopLoss";
            this.chkStopLoss.CheckedChanged += ChkStopLoss_CheckedChanged;
            this.pnlOrderEntry.Controls.Add(this.chkStopLoss);

            var lblStopLoss = new LabelControl();
            lblStopLoss.Appearance.Font = new Font("Tahoma", 8.25F);
            lblStopLoss.Appearance.ForeColor = Color.LightGray;
            lblStopLoss.Location = new Point(15, 180);
            lblStopLoss.Text = "Stop Fiyatı:";
            this.pnlOrderEntry.Controls.Add(lblStopLoss);

            this.numStopLoss.Location = new Point(15, 200);
            this.numStopLoss.Size = new Size(250, 20);
            this.numStopLoss.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.numStopLoss.Properties.MinValue = 0;
            this.numStopLoss.Properties.MaxValue = 999999;
            this.numStopLoss.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.numStopLoss.Properties.DisplayFormat.FormatString = "N2";
            this.numStopLoss.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.numStopLoss.Properties.EditFormat.FormatString = "N2";
            this.numStopLoss.Enabled = false;
            this.numStopLoss.Name = "numStopLoss";
            this.pnlOrderEntry.Controls.Add(this.numStopLoss);

            // === Take-Profit ===
            this.chkTakeProfit.Location = new Point(15, 235);
            this.chkTakeProfit.Size = new Size(250, 20);
            this.chkTakeProfit.Text = "Take-Profit Aktif";
            this.chkTakeProfit.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.chkTakeProfit.Properties.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            this.chkTakeProfit.Properties.Appearance.Options.UseFont = true;
            this.chkTakeProfit.Properties.Appearance.Options.UseForeColor = true;
            this.chkTakeProfit.Name = "chkTakeProfit";
            this.chkTakeProfit.CheckedChanged += ChkTakeProfit_CheckedChanged;
            this.pnlOrderEntry.Controls.Add(this.chkTakeProfit);

            var lblTakeProfit = new LabelControl();
            lblTakeProfit.Appearance.Font = new Font("Tahoma", 8.25F);
            lblTakeProfit.Appearance.ForeColor = Color.LightGray;
            lblTakeProfit.Location = new Point(15, 260);
            lblTakeProfit.Text = "Kâr Al Fiyatı:";
            this.pnlOrderEntry.Controls.Add(lblTakeProfit);

            this.numTakeProfit.Location = new Point(15, 280);
            this.numTakeProfit.Size = new Size(250, 20);
            this.numTakeProfit.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.numTakeProfit.Properties.MinValue = 0;
            this.numTakeProfit.Properties.MaxValue = 999999;
            this.numTakeProfit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.numTakeProfit.Properties.DisplayFormat.FormatString = "N2";
            this.numTakeProfit.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.numTakeProfit.Properties.EditFormat.FormatString = "N2";
            this.numTakeProfit.Enabled = false;
            this.numTakeProfit.Name = "numTakeProfit";
            this.pnlOrderEntry.Controls.Add(this.numTakeProfit);

            // === Buy Button ===
            this.btnBuy.Location = new Point(15, 330);
            this.btnBuy.Size = new Size(250, 40);
            this.btnBuy.Text = "AL (BUY)";
            this.btnBuy.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnBuy.Appearance.ForeColor = Color.White;
            this.btnBuy.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.btnBuy.Appearance.Options.UseBackColor = true;
            this.btnBuy.Appearance.Options.UseForeColor = true;
            this.btnBuy.Appearance.Options.UseFont = true;
            this.btnBuy.Name = "btnBuy";
            this.btnBuy.Click += BtnBuy_Click;
            this.pnlOrderEntry.Controls.Add(this.btnBuy);

            // === Sell Button ===
            this.btnSell.Location = new Point(15, 380);
            this.btnSell.Size = new Size(250, 40);
            this.btnSell.Text = "SAT (SELL)";
            this.btnSell.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            this.btnSell.Appearance.ForeColor = Color.White;
            this.btnSell.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.btnSell.Appearance.Options.UseBackColor = true;
            this.btnSell.Appearance.Options.UseForeColor = true;
            this.btnSell.Appearance.Options.UseFont = true;
            this.btnSell.Name = "btnSell";
            this.btnSell.Click += BtnSell_Click;
            this.pnlOrderEntry.Controls.Add(this.btnSell);

            // === Refresh Button ===
            this.btnRefresh.Location = new Point(15, 435);
            this.btnRefresh.Size = new Size(250, 30);
            this.btnRefresh.Text = "🔄 Yenile";
            this.btnRefresh.Appearance.BackColor = Color.FromArgb(60, 60, 60);
            this.btnRefresh.Appearance.ForeColor = Color.White;
            this.btnRefresh.Appearance.Font = new Font("Tahoma", 8.25F);
            this.btnRefresh.Appearance.Options.UseBackColor = true;
            this.btnRefresh.Appearance.Options.UseForeColor = true;
            this.btnRefresh.Appearance.Options.UseFont = true;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Click += BtnRefresh_Click;
            this.pnlOrderEntry.Controls.Add(this.btnRefresh);

            // ======= FORM =======
            this.ClientSize = new Size(1320, 560);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.gridStocks);
            this.Controls.Add(this.chartStock);
            this.Controls.Add(this.lblSelectedStock);
            this.Controls.Add(this.lblCurrentPrice);
            this.Controls.Add(this.pnlOrderEntry);
            this.Name = "StockMarketForm";
            this.Text = "Profesyonel Borsa Platformu - NovaBank";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 20);

            ((System.ComponentModel.ISupportInitialize)(this.gridStocks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewStocks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlOrderEntry)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSymbol.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantity.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStopLoss.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTakeProfit.Properties)).EndInit();
            this.pnlOrderEntry.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadStockData()
        {
            var stocks = new List<StockData>
            {
                new StockData { Symbol = "THYAO.IS", Name = "Türk Hava Yolları", Price = 285.50m, Change = 2.35m, Volume = 125000000 },
                new StockData { Symbol = "GARAN.IS", Name = "Garanti Bankası", Price = 95.20m, Change = -1.15m, Volume = 98000000 },
                new StockData { Symbol = "ASELS.IS", Name = "Aselsan", Price = 52.80m, Change = 4.20m, Volume = 45000000 },
                new StockData { Symbol = "SISE.IS", Name = "Şişecam", Price = 48.90m, Change = -0.85m, Volume = 32000000 },
                new StockData { Symbol = "EREGL.IS", Name = "Ereğli Demir Çelik", Price = 56.40m, Change = 1.50m, Volume = 78000000 },
                new StockData { Symbol = "KCHOL.IS", Name = "Koç Holding", Price = 175.60m, Change = 0.95m, Volume = 55000000 },
                new StockData { Symbol = "AKBNK.IS", Name = "Akbank", Price = 48.75m, Change = -2.10m, Volume = 88000000 },
                new StockData { Symbol = "TUPRS.IS", Name = "Tüpraş", Price = 165.30m, Change = 3.45m, Volume = 42000000 },
                new StockData { Symbol = "SAHOL.IS", Name = "Sabancı Holding", Price = 78.90m, Change = 1.25m, Volume = 61000000 },
                new StockData { Symbol = "BIMAS.IS", Name = "BİM", Price = 395.00m, Change = -0.50m, Volume = 28000000 }
            };

            gridStocks.DataSource = stocks;
            
            // İlk hisseyi seç
            if (stocks.Count > 0)
            {
                LoadCandleStickChart(stocks[0].Symbol);
            }
        }

        private async void GridViewStocks_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var row = gridViewStocks.GetFocusedRow() as StockData;
            if (row != null)
            {
                // Canlı veri çek
                _currentStock = await _stockService.GetLiveStockDataAsync(row.Symbol);
                
                lblSelectedStock.Text = $"Seçili Hisse: {row.Symbol} - {row.Name}";
                lblCurrentPrice.Text = $"Güncel Fiyat: {_currentStock.Price:N2} TL | Değişim: {_currentStock.ChangePercentDisplay} ({(_currentStock.IsLive ? "CANLI" : "SİMÜLASYON")})";
                lblCurrentPrice.Appearance.ForeColor = _currentStock.Change >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(244, 67, 54);
                
                txtSymbol.Text = row.Symbol;
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
            
            // CandleStick verisi
            var random = new Random(symbol.GetHashCode());
            var seriesCandle = new Series(symbol, ViewType.CandleStick);
            
            var basePrice = random.Next(50, 200);
            DateTime startDate = DateTime.Now.AddDays(-60);
            
            var priceData = new List<double>();
            
            for (int i = 0; i < 60; i++)
            {
                var open = basePrice + (random.NextDouble() * 10 - 5);
                var high = open + random.NextDouble() * 5;
                var low = open - random.NextDouble() * 5;
                var close = low + random.NextDouble() * (high - low);
                
                seriesCandle.Points.Add(new SeriesPoint(startDate.AddDays(i), new double[] { high, low, open, close }));
                priceData.Add(close);
                basePrice = (int)close;
            }

            var candleView = (CandleStickSeriesView)seriesCandle.View;
            candleView.Color = Color.FromArgb(76, 175, 80);
            candleView.ReductionOptions.Color = Color.FromArgb(244, 67, 54);
            candleView.ReductionOptions.Visible = true;
            candleView.LineThickness = 2;

            chartStock.Series.Add(seriesCandle);
            
            // SMA (20 günlük basit hareketli ortalama) ekle
            var seriesSMA = new Series("SMA (20)", ViewType.Line);
            for (int i = 19; i < 60; i++)
            {
                double sma = 0;
                for (int j = i - 19; j <= i; j++)
                {
                    sma += priceData[j];
                }
                sma /= 20;
                seriesSMA.Points.Add(new SeriesPoint(startDate.AddDays(i), sma));
            }
            
            var lineView = (LineSeriesView)seriesSMA.View;
            lineView.Color = Color.FromArgb(255, 193, 7);
            lineView.LineStyle.Thickness = 2;
            
            chartStock.Series.Add(seriesSMA);
            
            chartStock.Titles.Clear();
            var title = new ChartTitle() 
            { 
                Text = $"{symbol} - Son 60 Gün (CandleStick + SMA)", 
                TextColor = Color.White,
                Font = new Font("Tahoma", 10F, FontStyle.Bold)
            };
            chartStock.Titles.Add(title);
        }

        private void ChkStopLoss_CheckedChanged(object sender, EventArgs e)
        {
            numStopLoss.Enabled = chkStopLoss.Checked;
        }

        private void ChkTakeProfit_CheckedChanged(object sender, EventArgs e)
        {
            numTakeProfit.Enabled = chkTakeProfit.Checked;
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (_currentStock != null)
            {
                _currentStock = await _stockService.GetLiveStockDataAsync(_currentStock.Symbol);
                lblCurrentPrice.Text = $"Güncel Fiyat: {_currentStock.Price:N2} TL | Değişim: {_currentStock.ChangePercentDisplay} ({(_currentStock.IsLive ? "CANLI" : "SİMÜLASYON")})";
                lblCurrentPrice.Appearance.ForeColor = _currentStock.Change >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(244, 67, 54);
                XtraMessageBox.Show("Veriler güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnBuy_Click(object sender, EventArgs e)
        {
            if (_currentStock == null)
            {
                XtraMessageBox.Show("Lütfen bir hisse seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantity = (int)numQuantity.Value;
            decimal total = _currentStock.Price * quantity;
            
            string orderDetails = $"✅ ALIM EMRİ GÖNDERİLDİ\n\n" +
                $"Hisse: {_currentStock.Symbol}\n" +
                $"Adet: {quantity}\n" +
                $"Fiyat: {_currentStock.Price:N2} TL\n" +
                $"Toplam: {total:N2} TL\n";
            
            if (chkStopLoss.Checked)
            {
                orderDetails += $"\n🛑 Stop-Loss: {numStopLoss.Value:N2} TL";
            }
            
            if (chkTakeProfit.Checked)
            {
                orderDetails += $"\n💰 Take-Profit: {numTakeProfit.Value:N2} TL";
            }

            XtraMessageBox.Show(orderDetails, "Alım Emri", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSell_Click(object sender, EventArgs e)
        {
            if (_currentStock == null)
            {
                XtraMessageBox.Show("Lütfen bir hisse seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantity = (int)numQuantity.Value;
            decimal total = _currentStock.Price * quantity;
            
            string orderDetails = $"✅ SATIM EMRİ GÖNDERİLDİ\n\n" +
                $"Hisse: {_currentStock.Symbol}\n" +
                $"Adet: {quantity}\n" +
                $"Fiyat: {_currentStock.Price:N2} TL\n" +
                $"Toplam: {total:N2} TL\n";
            
            if (chkStopLoss.Checked)
            {
                orderDetails += $"\n🛑 Stop-Loss: {numStopLoss.Value:N2} TL";
            }
            
            if (chkTakeProfit.Checked)
            {
                orderDetails += $"\n💰 Take-Profit: {numTakeProfit.Value:N2} TL";
            }

            XtraMessageBox.Show(orderDetails, "Satım Emri", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
