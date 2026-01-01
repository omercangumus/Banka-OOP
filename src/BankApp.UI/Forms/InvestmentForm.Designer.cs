using System.ComponentModel;
using System.Drawing;
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
    /// <summary>
    /// Yatırım ve borsa işlemleri formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class InvestmentForm
    {
        private IContainer components = null;
        
        // Layout
        private LayoutControl layoutMain;
        private LayoutControlGroup layoutGroupRoot;
        
        // Header
        private LabelControl lblBaslik;
        private LabelControl lblPortfoyDegeri;
        private ToggleSwitch tglParaBirimi;
        
        // Grids
        private GridControl grdHisseler;
        private GridView grdwHisseler;
        private GridControl grdPortfoy;
        private GridView grdwPortfoy;
        
        // Buttons
        private SimpleButton btnHisseAl;
        private SimpleButton btnHisseSat;
        private SimpleButton btnYenile;

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
            
            this.lblBaslik = new LabelControl();
            this.lblPortfoyDegeri = new LabelControl();
            this.tglParaBirimi = new ToggleSwitch();
            
            this.grdHisseler = new GridControl();
            this.grdwHisseler = new GridView();
            this.grdPortfoy = new GridControl();
            this.grdwPortfoy = new GridView();
            
            this.btnHisseAl = new SimpleButton();
            this.btnHisseSat = new SimpleButton();
            this.btnYenile = new SimpleButton();

            // Begin Init
            ((ISupportInitialize)(this.layoutMain)).BeginInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).BeginInit();
            ((ISupportInitialize)(this.grdHisseler)).BeginInit();
            ((ISupportInitialize)(this.grdwHisseler)).BeginInit();
            ((ISupportInitialize)(this.grdPortfoy)).BeginInit();
            ((ISupportInitialize)(this.grdwPortfoy)).BeginInit();
            ((ISupportInitialize)(this.tglParaBirimi.Properties)).BeginInit();
            this.layoutMain.SuspendLayout();
            this.SuspendLayout();

            // ============================================
            // HEADER
            // ============================================
            this.lblBaslik.Text = "📈 Yatırım Portföyüm";
            this.lblBaslik.Appearance.Font = new Font("Tahoma", 20F, FontStyle.Bold);
            this.lblBaslik.Appearance.ForeColor = Color.White;
            this.lblBaslik.Appearance.Options.UseFont = true;
            this.lblBaslik.Appearance.Options.UseForeColor = true;
            this.lblBaslik.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblBaslik.Size = new Size(350, 45);
            
            this.lblPortfoyDegeri.Text = "₺ 0.00";
            this.lblPortfoyDegeri.Appearance.Font = new Font("Tahoma", 24F, FontStyle.Bold);
            this.lblPortfoyDegeri.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            this.lblPortfoyDegeri.Appearance.Options.UseFont = true;
            this.lblPortfoyDegeri.Appearance.Options.UseForeColor = true;
            this.lblPortfoyDegeri.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblPortfoyDegeri.Size = new Size(300, 50);

            this.tglParaBirimi.Name = "tglParaBirimi";
            this.tglParaBirimi.Properties.OffText = "TRY";
            this.tglParaBirimi.Properties.OnText = "USD";
            this.tglParaBirimi.Properties.Appearance.Font = new Font("Tahoma", 10F);
            this.tglParaBirimi.Size = new Size(100, 30);
            this.tglParaBirimi.Toggled += new System.EventHandler(this.tglParaBirimi_Toggled);

            // ============================================
            // STOCKS GRID - Piyasa
            // ============================================
            this.grdHisseler.Name = "grdHisseler";
            this.grdHisseler.MainView = this.grdwHisseler;
            this.grdHisseler.Size = new Size(500, 300);
            this.grdHisseler.ViewCollection.Add(this.grdwHisseler);

            this.grdwHisseler.GridControl = this.grdHisseler;
            this.grdwHisseler.Name = "grdwHisseler";
            this.grdwHisseler.OptionsView.ShowGroupPanel = false;
            this.grdwHisseler.OptionsView.ShowAutoFilterRow = true;
            this.grdwHisseler.OptionsBehavior.Editable = false;
            this.grdwHisseler.Appearance.Row.Font = new Font("Tahoma", 10F);
            this.grdwHisseler.Appearance.Row.ForeColor = Color.White;
            this.grdwHisseler.Appearance.HeaderPanel.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.grdwHisseler.Appearance.HeaderPanel.ForeColor = Color.FromArgb(180, 180, 190);
            this.grdwHisseler.RowCellStyle += new RowCellStyleEventHandler(this.grdwHisseler_RowCellStyle);
            
            // Columns
            this.grdwHisseler.Columns.AddVisible("Symbol", "Sembol");
            this.grdwHisseler.Columns.AddVisible("Name", "Şirket");
            this.grdwHisseler.Columns.AddVisible("CurrentPrice", "Fiyat");
            this.grdwHisseler.Columns.AddVisible("ChangePercent", "Değişim %");
            this.grdwHisseler.Columns.AddVisible("Sector", "Sektör");
            
            this.grdwHisseler.Columns["CurrentPrice"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.grdwHisseler.Columns["CurrentPrice"].DisplayFormat.FormatString = "N2";
            this.grdwHisseler.Columns["ChangePercent"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.grdwHisseler.Columns["ChangePercent"].DisplayFormat.FormatString = "+0.00%;-0.00%;0.00%";

            // ============================================
            // PORTFOLIO GRID - Portföy
            // ============================================
            this.grdPortfoy.Name = "grdPortfoy";
            this.grdPortfoy.MainView = this.grdwPortfoy;
            this.grdPortfoy.Size = new Size(500, 250);
            this.grdPortfoy.ViewCollection.Add(this.grdwPortfoy);

            this.grdwPortfoy.GridControl = this.grdPortfoy;
            this.grdwPortfoy.Name = "grdwPortfoy";
            this.grdwPortfoy.OptionsView.ShowGroupPanel = false;
            this.grdwPortfoy.OptionsBehavior.Editable = false;
            this.grdwPortfoy.Appearance.Row.Font = new Font("Tahoma", 10F);
            this.grdwPortfoy.Appearance.Row.ForeColor = Color.White;
            this.grdwPortfoy.Appearance.HeaderPanel.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.grdwPortfoy.Appearance.HeaderPanel.ForeColor = Color.FromArgb(180, 180, 190);
            this.grdwPortfoy.RowCellStyle += new RowCellStyleEventHandler(this.grdwPortfoy_RowCellStyle);
            
            // Columns
            this.grdwPortfoy.Columns.AddVisible("StockSymbol", "Sembol");
            this.grdwPortfoy.Columns.AddVisible("StockName", "Şirket");
            this.grdwPortfoy.Columns.AddVisible("Quantity", "Adet");
            this.grdwPortfoy.Columns.AddVisible("AverageCost", "Ort. Maliyet");
            this.grdwPortfoy.Columns.AddVisible("CurrentPrice", "Güncel Fiyat");
            this.grdwPortfoy.Columns.AddVisible("CurrentValue", "Değer");
            this.grdwPortfoy.Columns.AddVisible("ProfitLoss", "Kâr/Zarar");
            this.grdwPortfoy.Columns.AddVisible("ProfitLossPercent", "K/Z %");
            
            foreach (GridColumn col in this.grdwPortfoy.Columns)
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
            this.btnHisseAl.Text = "🛒 Hisse Al";
            this.btnHisseAl.Appearance.Font = new Font("Tahoma", 11F, FontStyle.Bold);
            this.btnHisseAl.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnHisseAl.Appearance.ForeColor = Color.White;
            this.btnHisseAl.Appearance.Options.UseBackColor = true;
            this.btnHisseAl.Appearance.Options.UseForeColor = true;
            this.btnHisseAl.Appearance.Options.UseFont = true;
            this.btnHisseAl.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnHisseAl.Size = new Size(150, 45);
            this.btnHisseAl.Click += new System.EventHandler(this.btnHisseAl_Click);

            this.btnHisseSat.Text = "💰 Hisse Sat";
            this.btnHisseSat.Appearance.Font = new Font("Tahoma", 11F, FontStyle.Bold);
            this.btnHisseSat.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            this.btnHisseSat.Appearance.ForeColor = Color.White;
            this.btnHisseSat.Appearance.Options.UseBackColor = true;
            this.btnHisseSat.Appearance.Options.UseForeColor = true;
            this.btnHisseSat.Appearance.Options.UseFont = true;
            this.btnHisseSat.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnHisseSat.Size = new Size(150, 45);
            this.btnHisseSat.Click += new System.EventHandler(this.btnHisseSat_Click);

            this.btnYenile.Text = "🔄 Yenile";
            this.btnYenile.Appearance.Font = new Font("Tahoma", 10F);
            this.btnYenile.Appearance.BackColor = Color.FromArgb(50, 50, 60);
            this.btnYenile.Appearance.ForeColor = Color.White;
            this.btnYenile.Appearance.Options.UseBackColor = true;
            this.btnYenile.Appearance.Options.UseForeColor = true;
            this.btnYenile.Appearance.Options.UseFont = true;
            this.btnYenile.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnYenile.Size = new Size(100, 40);

            // ============================================
            // LAYOUT
            // ============================================
            this.layoutMain.Dock = DockStyle.Fill;
            this.layoutMain.Controls.Add(this.lblBaslik);
            this.layoutMain.Controls.Add(this.lblPortfoyDegeri);
            this.layoutMain.Controls.Add(this.tglParaBirimi);
            this.layoutMain.Controls.Add(this.grdHisseler);
            this.layoutMain.Controls.Add(this.grdPortfoy);
            this.layoutMain.Controls.Add(this.btnHisseAl);
            this.layoutMain.Controls.Add(this.btnHisseSat);
            this.layoutMain.Controls.Add(this.btnYenile);
            this.layoutMain.Root = this.layoutGroupRoot;

            var layoutItemTitle = new LayoutControlItem(this.layoutMain, this.lblBaslik);
            layoutItemTitle.TextVisible = false;
            layoutItemTitle.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutItemTitle.MinSize = new Size(350, 55);
            layoutItemTitle.MaxSize = new Size(350, 55);

            var layoutItemValue = new LayoutControlItem(this.layoutMain, this.lblPortfoyDegeri);
            layoutItemValue.TextVisible = false;
            layoutItemValue.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutItemValue.MinSize = new Size(300, 60);
            layoutItemValue.MaxSize = new Size(300, 60);

            var layoutItemToggle = new LayoutControlItem(this.layoutMain, this.tglParaBirimi);
            layoutItemToggle.Text = "Para Birimi:";
            layoutItemToggle.AppearanceItemCaption.ForeColor = Color.White;
            layoutItemToggle.AppearanceItemCaption.Font = new Font("Tahoma", 10F);
            layoutItemToggle.AppearanceItemCaption.Options.UseForeColor = true;
            layoutItemToggle.AppearanceItemCaption.Options.UseFont = true;

            var layoutItemStocks = new LayoutControlItem(this.layoutMain, this.grdHisseler);
            layoutItemStocks.Text = "📊 Piyasa";
            layoutItemStocks.TextLocation = Locations.Top;
            layoutItemStocks.AppearanceItemCaption.Font = new Font("Tahoma", 12F, FontStyle.Bold);
            layoutItemStocks.AppearanceItemCaption.ForeColor = Color.White;
            layoutItemStocks.AppearanceItemCaption.Options.UseFont = true;
            layoutItemStocks.AppearanceItemCaption.Options.UseForeColor = true;

            var layoutItemPortfolio = new LayoutControlItem(this.layoutMain, this.grdPortfoy);
            layoutItemPortfolio.Text = "💼 Portföyüm";
            layoutItemPortfolio.TextLocation = Locations.Top;
            layoutItemPortfolio.AppearanceItemCaption.Font = new Font("Tahoma", 12F, FontStyle.Bold);
            layoutItemPortfolio.AppearanceItemCaption.ForeColor = Color.White;
            layoutItemPortfolio.AppearanceItemCaption.Options.UseFont = true;
            layoutItemPortfolio.AppearanceItemCaption.Options.UseForeColor = true;

            var layoutItemBuy = new LayoutControlItem(this.layoutMain, this.btnHisseAl);
            layoutItemBuy.TextVisible = false;
            
            var layoutItemSell = new LayoutControlItem(this.layoutMain, this.btnHisseSat);
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
            ((ISupportInitialize)(this.grdHisseler)).EndInit();
            ((ISupportInitialize)(this.grdwHisseler)).EndInit();
            ((ISupportInitialize)(this.grdPortfoy)).EndInit();
            ((ISupportInitialize)(this.grdwPortfoy)).EndInit();
            ((ISupportInitialize)(this.tglParaBirimi.Properties)).EndInit();
            this.layoutMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
