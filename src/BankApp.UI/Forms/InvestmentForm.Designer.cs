using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.LookAndFeel;
using DevExpress.Utils;

namespace BankApp.UI.Forms
{
    public partial class InvestmentForm
    {
        private IContainer components = null;
        
        // Layout
        private LayoutControl layoutMain;
        private LayoutControlGroup layoutGroupRoot;
        
        // Header
        private LabelControl lblTitle;
        private LabelControl lblPortfolioValue;
        private ToggleSwitch toggleCurrency;
        
        // Grids
        private GridControl gridStocks;
        private GridView gridViewStocks;
        private GridControl gridPortfolio;
        private GridView gridViewPortfolio;
        
        // Buttons
        private SimpleButton btnBuyStock;
        private SimpleButton btnSellStock;
        private SimpleButton btnRefresh;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            
            // Dark theme
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Initialize controls
            this.layoutMain = new LayoutControl();
            this.layoutGroupRoot = new LayoutControlGroup();
            
            this.lblTitle = new LabelControl();
            this.lblPortfolioValue = new LabelControl();
            this.toggleCurrency = new ToggleSwitch();
            
            this.gridStocks = new GridControl();
            this.gridViewStocks = new GridView();
            this.gridPortfolio = new GridControl();
            this.gridViewPortfolio = new GridView();
            
            this.btnBuyStock = new SimpleButton();
            this.btnSellStock = new SimpleButton();
            this.btnRefresh = new SimpleButton();

            // Begin Init
            ((ISupportInitialize)(this.layoutMain)).BeginInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).BeginInit();
            ((ISupportInitialize)(this.gridStocks)).BeginInit();
            ((ISupportInitialize)(this.gridViewStocks)).BeginInit();
            ((ISupportInitialize)(this.gridPortfolio)).BeginInit();
            ((ISupportInitialize)(this.gridViewPortfolio)).BeginInit();
            ((ISupportInitialize)(this.toggleCurrency.Properties)).BeginInit();
            this.layoutMain.SuspendLayout();
            this.SuspendLayout();

            // ============================================
            // HEADER
            // ============================================
            this.lblTitle.Text = "📈 Yatırım Portföyüm";
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.White;
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblTitle.Size = new Size(350, 45);
            
            this.lblPortfolioValue.Text = "₺ 0.00";
            this.lblPortfolioValue.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            this.lblPortfolioValue.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            this.lblPortfolioValue.Appearance.Options.UseFont = true;
            this.lblPortfolioValue.Appearance.Options.UseForeColor = true;
            this.lblPortfolioValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblPortfolioValue.Size = new Size(300, 50);

            this.toggleCurrency.Name = "toggleCurrency";
            this.toggleCurrency.Properties.OffText = "TRY";
            this.toggleCurrency.Properties.OnText = "USD";
            this.toggleCurrency.Size = new Size(80, 25);
            this.toggleCurrency.Toggled += new System.EventHandler(this.toggleCurrency_Toggled);

            // ============================================
            // STOCKS GRID - Piyasa
            // ============================================
            this.gridStocks.Name = "gridStocks";
            this.gridStocks.MainView = this.gridViewStocks;
            this.gridStocks.Size = new Size(500, 300);
            this.gridStocks.ViewCollection.Add(this.gridViewStocks);

            this.gridViewStocks.GridControl = this.gridStocks;
            this.gridViewStocks.Name = "gridViewStocks";
            this.gridViewStocks.OptionsView.ShowGroupPanel = false;
            this.gridViewStocks.OptionsView.ShowAutoFilterRow = true;
            this.gridViewStocks.OptionsBehavior.Editable = false;
            this.gridViewStocks.Appearance.Row.Font = new Font("Segoe UI", 10F);
            this.gridViewStocks.Appearance.Row.ForeColor = Color.White;
            this.gridViewStocks.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 10F);
            this.gridViewStocks.Appearance.HeaderPanel.ForeColor = Color.FromArgb(180, 180, 190);
            this.gridViewStocks.RowCellStyle += new RowCellStyleEventHandler(this.gridViewStocks_RowCellStyle);
            
            // Columns
            this.gridViewStocks.Columns.AddVisible("Symbol", "Sembol");
            this.gridViewStocks.Columns.AddVisible("Name", "Şirket");
            this.gridViewStocks.Columns.AddVisible("CurrentPrice", "Fiyat (₺)");
            this.gridViewStocks.Columns.AddVisible("ChangePercent", "Değişim %");
            this.gridViewStocks.Columns.AddVisible("Sector", "Sektör");
            
            this.gridViewStocks.Columns["CurrentPrice"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridViewStocks.Columns["CurrentPrice"].DisplayFormat.FormatString = "N2";
            this.gridViewStocks.Columns["ChangePercent"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridViewStocks.Columns["ChangePercent"].DisplayFormat.FormatString = "+0.00%;-0.00%;0.00%";

            // ============================================
            // PORTFOLIO GRID - Portföy
            // ============================================
            this.gridPortfolio.Name = "gridPortfolio";
            this.gridPortfolio.MainView = this.gridViewPortfolio;
            this.gridPortfolio.Size = new Size(500, 250);
            this.gridPortfolio.ViewCollection.Add(this.gridViewPortfolio);

            this.gridViewPortfolio.GridControl = this.gridPortfolio;
            this.gridViewPortfolio.Name = "gridViewPortfolio";
            this.gridViewPortfolio.OptionsView.ShowGroupPanel = false;
            this.gridViewPortfolio.OptionsBehavior.Editable = false;
            this.gridViewPortfolio.Appearance.Row.Font = new Font("Segoe UI", 10F);
            this.gridViewPortfolio.Appearance.Row.ForeColor = Color.White;
            this.gridViewPortfolio.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 10F);
            this.gridViewPortfolio.Appearance.HeaderPanel.ForeColor = Color.FromArgb(180, 180, 190);
            this.gridViewPortfolio.RowCellStyle += new RowCellStyleEventHandler(this.gridViewPortfolio_RowCellStyle);
            
            // Columns
            this.gridViewPortfolio.Columns.AddVisible("StockSymbol", "Sembol");
            this.gridViewPortfolio.Columns.AddVisible("StockName", "Şirket");
            this.gridViewPortfolio.Columns.AddVisible("Quantity", "Adet");
            this.gridViewPortfolio.Columns.AddVisible("AverageCost", "Ortalama Maliyet");
            this.gridViewPortfolio.Columns.AddVisible("CurrentPrice", "Güncel Fiyat");
            this.gridViewPortfolio.Columns.AddVisible("CurrentValue", "Değer");
            this.gridViewPortfolio.Columns.AddVisible("ProfitLoss", "Kâr/Zarar");
            this.gridViewPortfolio.Columns.AddVisible("ProfitLossPercent", "K/Z %");
            
            foreach (GridColumn col in this.gridViewPortfolio.Columns)
            {
                if (col.FieldName.Contains("Cost") || col.FieldName.Contains("Price") || 
                    col.FieldName.Contains("Value") || col.FieldName == "ProfitLoss")
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = "N2";
                }
                if (col.FieldName == "ProfitLossPercent")
                {
                    col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = "+0.00%;-0.00%;0.00%";
                }
            }

            // ============================================
            // BUTTONS
            // ============================================
            this.btnBuyStock.Text = "🛒 Hisse Al";
            this.btnBuyStock.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnBuyStock.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnBuyStock.Appearance.ForeColor = Color.White;
            this.btnBuyStock.Appearance.Options.UseBackColor = true;
            this.btnBuyStock.Appearance.Options.UseForeColor = true;
            this.btnBuyStock.Appearance.Options.UseFont = true;
            this.btnBuyStock.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnBuyStock.Size = new Size(150, 45);
            this.btnBuyStock.Click += new System.EventHandler(this.btnBuyStock_Click);

            this.btnSellStock.Text = "💰 Hisse Sat";
            this.btnSellStock.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnSellStock.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            this.btnSellStock.Appearance.ForeColor = Color.White;
            this.btnSellStock.Appearance.Options.UseBackColor = true;
            this.btnSellStock.Appearance.Options.UseForeColor = true;
            this.btnSellStock.Appearance.Options.UseFont = true;
            this.btnSellStock.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnSellStock.Size = new Size(150, 45);
            this.btnSellStock.Click += new System.EventHandler(this.btnSellStock_Click);

            this.btnRefresh.Text = "🔄 Yenile";
            this.btnRefresh.Appearance.Font = new Font("Segoe UI", 11F);
            this.btnRefresh.Appearance.BackColor = Color.FromArgb(50, 50, 60);
            this.btnRefresh.Appearance.ForeColor = Color.White;
            this.btnRefresh.Appearance.Options.UseBackColor = true;
            this.btnRefresh.Appearance.Options.UseForeColor = true;
            this.btnRefresh.Appearance.Options.UseFont = true;
            this.btnRefresh.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnRefresh.Size = new Size(100, 40);

            // ============================================
            // LAYOUT
            // ============================================
            this.layoutMain.Dock = DockStyle.Fill;
            this.layoutMain.Controls.Add(this.lblTitle);
            this.layoutMain.Controls.Add(this.lblPortfolioValue);
            this.layoutMain.Controls.Add(this.toggleCurrency);
            this.layoutMain.Controls.Add(this.gridStocks);
            this.layoutMain.Controls.Add(this.gridPortfolio);
            this.layoutMain.Controls.Add(this.btnBuyStock);
            this.layoutMain.Controls.Add(this.btnSellStock);
            this.layoutMain.Controls.Add(this.btnRefresh);
            this.layoutMain.Root = this.layoutGroupRoot;

            var layoutItemTitle = new LayoutControlItem(this.layoutMain, this.lblTitle);
            layoutItemTitle.TextVisible = false;
            layoutItemTitle.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutItemTitle.MinSize = new Size(350, 55);
            layoutItemTitle.MaxSize = new Size(350, 55);

            var layoutItemValue = new LayoutControlItem(this.layoutMain, this.lblPortfolioValue);
            layoutItemValue.TextVisible = false;
            layoutItemValue.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutItemValue.MinSize = new Size(300, 60);
            layoutItemValue.MaxSize = new Size(300, 60);

            var layoutItemToggle = new LayoutControlItem(this.layoutMain, this.toggleCurrency);
            layoutItemToggle.Text = "Para Birimi:";
            layoutItemToggle.AppearanceItemCaption.ForeColor = Color.White;
            layoutItemToggle.AppearanceItemCaption.Options.UseForeColor = true;

            var layoutItemStocks = new LayoutControlItem(this.layoutMain, this.gridStocks);
            layoutItemStocks.Text = "📊 Piyasa";
            layoutItemStocks.TextLocation = Locations.Top;
            layoutItemStocks.AppearanceItemCaption.Font = new Font("Segoe UI Semibold", 12F);
            layoutItemStocks.AppearanceItemCaption.ForeColor = Color.White;
            layoutItemStocks.AppearanceItemCaption.Options.UseFont = true;
            layoutItemStocks.AppearanceItemCaption.Options.UseForeColor = true;

            var layoutItemPortfolio = new LayoutControlItem(this.layoutMain, this.gridPortfolio);
            layoutItemPortfolio.Text = "💼 Portföyüm";
            layoutItemPortfolio.TextLocation = Locations.Top;
            layoutItemPortfolio.AppearanceItemCaption.Font = new Font("Segoe UI Semibold", 12F);
            layoutItemPortfolio.AppearanceItemCaption.ForeColor = Color.White;
            layoutItemPortfolio.AppearanceItemCaption.Options.UseFont = true;
            layoutItemPortfolio.AppearanceItemCaption.Options.UseForeColor = true;

            var layoutItemBuy = new LayoutControlItem(this.layoutMain, this.btnBuyStock);
            layoutItemBuy.TextVisible = false;
            
            var layoutItemSell = new LayoutControlItem(this.layoutMain, this.btnSellStock);
            layoutItemSell.TextVisible = false;

            this.layoutGroupRoot.Name = "layoutGroupRoot";
            this.layoutGroupRoot.EnableIndentsWithoutBorders = DefaultBoolean.True;
            this.layoutGroupRoot.GroupBordersVisible = false;
            this.layoutGroupRoot.Padding = new DevExpress.XtraLayout.Utils.Padding(20, 20, 20, 20);

            // ============================================
            // FORM
            // ============================================
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1100, 750);
            this.Controls.Add(this.layoutMain);
            this.Name = "InvestmentForm";
            this.Text = "📈 Yatırım Portföyü";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);

            // End Init
            ((ISupportInitialize)(this.layoutMain)).EndInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).EndInit();
            ((ISupportInitialize)(this.gridStocks)).EndInit();
            ((ISupportInitialize)(this.gridViewStocks)).EndInit();
            ((ISupportInitialize)(this.gridPortfolio)).EndInit();
            ((ISupportInitialize)(this.gridViewPortfolio)).EndInit();
            ((ISupportInitialize)(this.toggleCurrency.Properties)).EndInit();
            this.layoutMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
