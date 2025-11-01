namespace BankApp.UI.Forms
{
    partial class TradeTerminalForm
    {
        private System.ComponentModel.IContainer components = null;
        
        // Ana Paneller
        private DevExpress.XtraEditors.PanelControl pnlMain; // Tüm ekran
        private DevExpress.XtraEditors.PanelControl pnlTop; // Üst kısım (Liste+Grafik+Trade)
        private DevExpress.XtraEditors.PanelControl pnlBottom; // ALT KISIM (Portföy)
        
        // Bileşenler
        private DevExpress.XtraEditors. TextEdit txtSearch;
        private DevExpress.XtraEditors.PanelControl pnlSearch;
        private DevExpress.XtraEditors.ListBoxControl lstStocks;
        private DevExpress.XtraCharts.ChartControl chartStock;
        private DevExpress.XtraEditors.GroupControl grpTrade;
        private DevExpress.XtraGrid.GridControl gridPortfolio;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewPortfolio;
        
        // Trade Elemanları
        private DevExpress.XtraEditors.LabelControl lblSymbol;
        private DevExpress.XtraEditors.LabelControl lblPrice;
        private DevExpress.XtraEditors.ComboBoxEdit cmbLeverage; // KALDIRAÇ
        private DevExpress.XtraEditors.LabelControl lblLevTitle;
        private DevExpress.XtraEditors.TextEdit txtQuantity;
        private DevExpress.XtraEditors.CheckEdit chkStopLoss;
        private DevExpress.XtraEditors.TextEdit txtStopPrice;
        private DevExpress.XtraEditors.SimpleButton btnBuy;
        private DevExpress.XtraEditors.SimpleButton btnSell;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // CHART AYARLARI
            DevExpress.XtraCharts.XYDiagram xyDiagram = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.CandleStickSeriesView view = new DevExpress.XtraCharts.CandleStickSeriesView();

            this.pnlMain = new DevExpress.XtraEditors.PanelControl();
            this.pnlTop = new DevExpress.XtraEditors.PanelControl();
            this.pnlBottom = new DevExpress.XtraEditors.PanelControl();
            this.pnlSearch = new DevExpress.XtraEditors.PanelControl();
            
            this.txtSearch = new DevExpress.XtraEditors.TextEdit();
            this.lstStocks = new DevExpress.XtraEditors.ListBoxControl();
            this.chartStock = new DevExpress.XtraCharts.ChartControl();
            this.grpTrade = new DevExpress.XtraEditors.GroupControl();
            
            this.gridPortfolio = new DevExpress.XtraGrid.GridControl();
            this.gridViewPortfolio = new DevExpress.XtraGrid.Views.Grid.GridView();
            
            // Trade Items init...
            this.lblSymbol = new DevExpress.XtraEditors.LabelControl();
            this.lblPrice = new DevExpress.XtraEditors.LabelControl();
            this.cmbLeverage = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblLevTitle = new DevExpress.XtraEditors.LabelControl();
            this.txtQuantity = new DevExpress.XtraEditors.TextEdit();
            this.chkStopLoss = new DevExpress.XtraEditors.CheckEdit();
            this.txtStopPrice = new DevExpress.XtraEditors.TextEdit();
            this.btnBuy = new DevExpress.XtraEditors.SimpleButton();
            this.btnSell = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.pnlMain)).BeginInit();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlTop)).BeginInit();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlBottom)).BeginInit();
            this.pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstStocks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpTrade)).BeginInit();
            this.grpTrade.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPortfolio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewPortfolio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLeverage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuantity.Properties)).BeginInit();
            this.SuspendLayout();

            // 
            // pnlMain
            // 
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Controls.Add(this.pnlTop);
            this.pnlMain.Controls.Add(this.pnlBottom);
            this.pnlMain.Name = "pnlMain";

            // 
            // pnlBottom (ALT TARAF - PORTFÖY)
            // 
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height = 200; // Yükseklik
            this.pnlBottom.Controls.Add(this.gridPortfolio);
            this.pnlBottom.Text = "Portföyüm";
            
            this.gridPortfolio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPortfolio.MainView = this.gridViewPortfolio;
            this.gridViewPortfolio.OptionsView.ShowGroupPanel = false;
            this.gridViewPortfolio.Appearance.HeaderPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridViewPortfolio.Appearance.Row.Font = new System.Drawing.Font("Tahoma", 8.25F);

            // 
            // pnlTop (Üst Taraf)
            // 
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTop.Controls.Add(this.chartStock);
            this.pnlTop.Controls.Add(this.grpTrade);
            this.pnlTop.Controls.Add(this.lstStocks);
            this.pnlTop.Controls.Add(this.pnlSearch);

            // 
            // pnlSearch (Search Panel)
            // 
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSearch.Width = 180;
            this.pnlSearch.Height = 35;
            this.pnlSearch.Controls.Add(this.txtSearch);
            
            // 
            // txtSearch
            // 
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Properties.NullText = "🔍 Search stocks...";
            this.txtSearch.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.txtSearch.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtSearch.EditValueChanged += new System.EventHandler(this.txtSearch_TextChanged);

            // 
            // lstStocks (Sol Liste)
            // 
            this.lstStocks.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstStocks.Width = 180;
            this.lstStocks.Appearance.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.lstStocks.Appearance.ForeColor = System.Drawing.Color.White;
            this.lstStocks.SelectedIndexChanged += new System.EventHandler(this.lstStocks_SelectedIndexChanged);

            // 
            // grpTrade (Sağ Panel)
            // 
            this.grpTrade.Dock = System.Windows.Forms.DockStyle.Right;
            this.grpTrade.Width = 220;
            this.grpTrade.Text = "İşlem Paneli";
            this.grpTrade.Controls.Add(this.lblSymbol);
            this.grpTrade.Controls.Add(this.lblPrice);
            this.grpTrade.Controls.Add(this.lblLevTitle);
            this.grpTrade.Controls.Add(this.cmbLeverage);
            this.grpTrade.Controls.Add(this.txtQuantity);
            this.grpTrade.Controls.Add(this.chkStopLoss);
            this.grpTrade.Controls.Add(this.txtStopPrice);
            this.grpTrade.Controls.Add(this.btnBuy);
            this.grpTrade.Controls.Add(this.btnSell);

            // Kaldıraç Ayarı
            this.lblLevTitle.Text = "Kaldıraç Oranı:";
            this.lblLevTitle.Location = new System.Drawing.Point(15, 100);
            
            this.cmbLeverage.Properties.Items.AddRange(new object[] { "x1", "x5", "x10", "x20", "x50", "x100" });
            this.cmbLeverage.SelectedIndex = 0; // x1
            this.cmbLeverage.Location = new System.Drawing.Point(15, 120);

            // Diğer Kontrollerin Yerleşimi (Basitleştirildi)
            this.lblSymbol.Location = new System.Drawing.Point(15, 30);
            this.lblSymbol.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            
            this.lblPrice.Location = new System.Drawing.Point(15, 60);
            this.lblPrice.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lblPrice.Appearance.ForeColor = System.Drawing.Color.Gold;

            this.txtQuantity.Properties.NullText = "Adet";
            this.txtQuantity.Location = new System.Drawing.Point(15, 160);

            this.btnBuy.Text = "AL";
            this.btnBuy.Appearance.BackColor = System.Drawing.Color.Green;
            this.btnBuy.Location = new System.Drawing.Point(15, 250);
            this.btnBuy.Click += new System.EventHandler(this.btnBuy_Click);
            
            this.btnSell.Text = "SAT";
            this.btnSell.Appearance.BackColor = System.Drawing.Color.Red;
            this.btnSell.Location = new System.Drawing.Point(110, 250);
            this.btnSell.Click += new System.EventHandler(this.btnSell_Click);

            // 
            // chartStock
            // 
            this.chartStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartStock.BackColor = System.Drawing.Color.FromArgb(35, 35, 35);
            xyDiagram.AxisX.DateTimeScaleOptions.MeasureUnit = DevExpress.XtraCharts.DateTimeMeasureUnit.Day;
            xyDiagram.DefaultPane.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.chartStock.Diagram = xyDiagram;
            series.View = view;
            view.Color = System.Drawing.Color.Lime;
            // view.ReductionOptions.Color = System.Drawing.Color.Red;
            this.chartStock.SeriesSerializable = new DevExpress.XtraCharts.Series[] { series };
            this.chartStock.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;

            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.pnlMain);
            this.Name = "TradeTerminalForm";
            this.Text = "📈 Trade Terminal - Aktif İşlem Ekranı";
            
            ((System.ComponentModel.ISupportInitialize)(this.pnlMain)).EndInit();
            this.pnlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlTop)).EndInit();
            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlBottom)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lstStocks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpTrade)).EndInit();
            this.grpTrade.ResumeLayout(false);
            this.grpTrade.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPortfolio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewPortfolio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLeverage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuantity.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
