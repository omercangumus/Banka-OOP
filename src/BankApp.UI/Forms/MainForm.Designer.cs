using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraCharts;
using DevExpress.XtraLayout;
using DevExpress.LookAndFeel;

namespace BankApp.UI.Forms
{
    public partial class MainForm
    {
        private IContainer components = null;
        private RibbonControl ribbonControl1;
        private RibbonPage pageDashboard;
        private RibbonPage pageCustomers;
        private RibbonPage pageInvestments; // Yeni: Yatırım sayfası
        private RibbonPageGroup groupStats;
        private RibbonPageGroup groupAI;
        private RibbonPageGroup groupCustomerActions;
        private RibbonPageGroup groupInvestmentActions; // Yeni: Yatırım grubu
        
        // Ribbon Items
        private DevExpress.XtraBars.BarButtonItem btnAiAssist;
        private DevExpress.XtraBars.BarButtonItem btnRefresh;
        private DevExpress.XtraBars.BarButtonItem btnMoneyTransfer; 
        
        // Customer CRUD
        private DevExpress.XtraBars.BarButtonItem btnAddCustomer;
        private DevExpress.XtraBars.BarButtonItem btnEditCustomer;
        private DevExpress.XtraBars.BarButtonItem btnDeleteCustomer;
        private DevExpress.XtraBars.BarButtonItem btnCustomerAccounts;
        private DevExpress.XtraBars.BarButtonItem btnExportExcel;
        private DevExpress.XtraBars.BarButtonItem btnExportPdf;
        private DevExpress.XtraBars.BarButtonItem btnAuditLogs;
        
        // Yeni: Yatırım butonları
        private DevExpress.XtraBars.BarButtonItem btnStockMarket;
        private DevExpress.XtraBars.BarButtonItem btnBES;
        
        // Dashboard Controls - YENİ TASARIM
        private PanelControl pnlDashboard;
        private LayoutControl layoutDashboard;
        private LayoutControlGroup layoutGroupRoot;
        
        // Stat Cards
        private PanelControl cardTotalAssets;
        private PanelControl cardDailyTransactions;
        private PanelControl cardActiveCustomers;
        private PanelControl cardExchangeRate;
        
        private LabelControl lblTotalAssetsTitle;
        private LabelControl lblTotalAssetsValue;
        private LabelControl lblDailyTransactionsTitle;
        private LabelControl lblDailyTransactionsValue;
        private LabelControl lblActiveCustomersTitle;
        private LabelControl lblActiveCustomersValue;
        private LabelControl lblExchangeRateTitle;
        private LabelControl lblExchangeRateValue;
        
        // Charts
        private ChartControl chartCurrency;
        private ChartControl chartTransactions;
        
        // Customer Grid
        private GridControl gridCustomers;
        private GridView gridViewCustomers;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            
            // Apply Dark Theme
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Initialize all components
            this.ribbonControl1 = new RibbonControl();
            this.pageDashboard = new RibbonPage("Genel Bakış");
            this.pageCustomers = new RibbonPage("Müşteriler");
            this.pageInvestments = new RibbonPage("Yatırım İşlemleri");
            this.groupStats = new RibbonPageGroup("Durum");
            this.groupAI = new RibbonPageGroup("AI");
            this.groupCustomerActions = new RibbonPageGroup("Müşteri İşlemleri");
            this.groupInvestmentActions = new RibbonPageGroup("Yatırım");
            
            // Buttons
            this.btnAiAssist = new DevExpress.XtraBars.BarButtonItem();
            this.btnRefresh = new DevExpress.XtraBars.BarButtonItem();
            this.btnMoneyTransfer = new DevExpress.XtraBars.BarButtonItem();
            this.btnAddCustomer = new DevExpress.XtraBars.BarButtonItem();
            this.btnEditCustomer = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteCustomer = new DevExpress.XtraBars.BarButtonItem();
            this.btnCustomerAccounts = new DevExpress.XtraBars.BarButtonItem();
            this.btnExportExcel = new DevExpress.XtraBars.BarButtonItem();
            this.btnExportPdf = new DevExpress.XtraBars.BarButtonItem();
            this.btnAuditLogs = new DevExpress.XtraBars.BarButtonItem();
            this.btnStockMarket = new DevExpress.XtraBars.BarButtonItem();
            this.btnBES = new DevExpress.XtraBars.BarButtonItem();
            
            // Dashboard Panel
            this.pnlDashboard = new PanelControl();
            this.layoutDashboard = new LayoutControl();
            this.layoutGroupRoot = new LayoutControlGroup();
            
            // Stat Cards
            this.cardTotalAssets = new PanelControl();
            this.cardDailyTransactions = new PanelControl();
            this.cardActiveCustomers = new PanelControl();
            this.cardExchangeRate = new PanelControl();
            
            this.lblTotalAssetsTitle = new LabelControl();
            this.lblTotalAssetsValue = new LabelControl();
            this.lblDailyTransactionsTitle = new LabelControl();
            this.lblDailyTransactionsValue = new LabelControl();
            this.lblActiveCustomersTitle = new LabelControl();
            this.lblActiveCustomersValue = new LabelControl();
            this.lblExchangeRateTitle = new LabelControl();
            this.lblExchangeRateValue = new LabelControl();
            
            // Charts
            this.chartCurrency = new ChartControl();
            this.chartTransactions = new ChartControl();
            
            // Grid
            this.gridCustomers = new GridControl();
            this.gridViewCustomers = new GridView();
            
            // Begin Init
            ((ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((ISupportInitialize)(this.pnlDashboard)).BeginInit();
            ((ISupportInitialize)(this.layoutDashboard)).BeginInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).BeginInit();
            ((ISupportInitialize)(this.cardTotalAssets)).BeginInit();
            ((ISupportInitialize)(this.cardDailyTransactions)).BeginInit();
            ((ISupportInitialize)(this.cardActiveCustomers)).BeginInit();
            ((ISupportInitialize)(this.cardExchangeRate)).BeginInit();
            ((ISupportInitialize)(this.chartCurrency)).BeginInit();
            ((ISupportInitialize)(this.chartTransactions)).BeginInit();
            ((ISupportInitialize)(this.gridCustomers)).BeginInit();
            ((ISupportInitialize)(this.gridViewCustomers)).BeginInit();
            this.pnlDashboard.SuspendLayout();
            this.layoutDashboard.SuspendLayout();
            this.SuspendLayout();

            // ============================================
            // RIBBON CONTROL
            // ============================================
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
                this.ribbonControl1.ExpandCollapseItem,
                this.btnAiAssist, this.btnRefresh, this.btnMoneyTransfer,
                this.btnAddCustomer, this.btnEditCustomer, this.btnDeleteCustomer,
                this.btnCustomerAccounts, this.btnExportExcel, this.btnExportPdf,
                this.btnAuditLogs, this.btnStockMarket, this.btnBES
            });
            this.ribbonControl1.Location = new Point(0, 0);
            this.ribbonControl1.MaxItemId = 13;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Pages.AddRange(new RibbonPage[] {
                this.pageDashboard, this.pageCustomers, this.pageInvestments
            });
            this.ribbonControl1.Size = new Size(1400, 158);

            // Button Configurations
            this.btnAiAssist.Caption = "AI Asistan";
            this.btnAiAssist.Id = 1;
            this.btnAiAssist.Name = "btnAiAssist";
            this.btnAiAssist.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnAiAssist.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAiAssist_ItemClick);

            this.btnRefresh.Caption = "Yenile";
            this.btnRefresh.Id = 2;
            this.btnRefresh.Name = "btnRefresh";

            this.btnMoneyTransfer.Caption = "Para Transferi";
            this.btnMoneyTransfer.Id = 3;
            this.btnMoneyTransfer.Name = "btnMoneyTransfer";
            this.btnMoneyTransfer.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnMoneyTransfer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMoneyTransfer_ItemClick);

            this.btnAddCustomer.Caption = "Yeni Müşteri";
            this.btnAddCustomer.Id = 4;
            this.btnAddCustomer.Name = "btnAddCustomer";
            this.btnAddCustomer.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnAddCustomer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddCustomer_ItemClick);

            this.btnEditCustomer.Caption = "Düzenle";
            this.btnEditCustomer.Id = 5;
            this.btnEditCustomer.Name = "btnEditCustomer";
            this.btnEditCustomer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnEditCustomer_ItemClick);

            this.btnDeleteCustomer.Caption = "Sil";
            this.btnDeleteCustomer.Id = 6;
            this.btnDeleteCustomer.Name = "btnDeleteCustomer";
            this.btnDeleteCustomer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteCustomer_ItemClick);

            this.btnCustomerAccounts.Caption = "Hesaplar";
            this.btnCustomerAccounts.Id = 7;
            this.btnCustomerAccounts.Name = "btnCustomerAccounts";
            this.btnCustomerAccounts.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnCustomerAccounts_ItemClick);

            this.btnExportExcel.Caption = "Excel'e Aktar";
            this.btnExportExcel.Id = 8;
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnExportExcel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExportExcel_ItemClick);

            this.btnExportPdf.Caption = "PDF'e Aktar";
            this.btnExportPdf.Id = 9;
            this.btnExportPdf.Name = "btnExportPdf";
            this.btnExportPdf.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnExportPdf.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExportPdf_ItemClick);

            this.btnAuditLogs.Caption = "Sistem Logları";
            this.btnAuditLogs.Id = 10;
            this.btnAuditLogs.Name = "btnAuditLogs";
            this.btnAuditLogs.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAuditLogs_ItemClick);

            // YENİ: Yatırım Butonları
            this.btnStockMarket.Caption = "Borsa";
            this.btnStockMarket.Id = 11;
            this.btnStockMarket.Name = "btnStockMarket";
            this.btnStockMarket.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnStockMarket.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnStockMarket_ItemClick);

            this.btnBES.Caption = "BES (Emeklilik)";
            this.btnBES.Id = 12;
            this.btnBES.Name = "btnBES";
            this.btnBES.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnBES.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnBES_ItemClick);

            // Ribbon Page Groups
            this.groupStats.ItemLinks.Add(this.btnRefresh);
            this.groupStats.ItemLinks.Add(this.btnMoneyTransfer);
            this.groupStats.ItemLinks.Add(this.btnAuditLogs);
            this.groupStats.Name = "groupStats";
            this.groupStats.Text = "Genel";

            this.groupAI.ItemLinks.Add(this.btnAiAssist);
            this.groupAI.Name = "groupAI";
            this.groupAI.Text = "Yapay Zeka";

            this.groupCustomerActions.ItemLinks.Add(this.btnAddCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnEditCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnDeleteCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnCustomerAccounts);
            this.groupCustomerActions.ItemLinks.Add(this.btnExportExcel);
            this.groupCustomerActions.ItemLinks.Add(this.btnExportPdf);
            this.groupCustomerActions.Name = "groupCustomerActions";
            this.groupCustomerActions.Text = "Müşteri";

            this.groupInvestmentActions.ItemLinks.Add(this.btnStockMarket);
            this.groupInvestmentActions.ItemLinks.Add(this.btnBES);
            this.groupInvestmentActions.Name = "groupInvestmentActions";
            this.groupInvestmentActions.Text = "Yatırım Araçları";

            // Ribbon Pages
            this.pageDashboard.Groups.Add(this.groupStats);
            this.pageDashboard.Groups.Add(this.groupAI);
            this.pageDashboard.Name = "pageDashboard";
            this.pageDashboard.Text = "Genel Bakış";

            this.pageCustomers.Groups.Add(this.groupCustomerActions);
            this.pageCustomers.Name = "pageCustomers";
            this.pageCustomers.Text = "Müşteriler";

            this.pageInvestments.Groups.Add(this.groupInvestmentActions);
            this.pageInvestments.Name = "pageInvestments";
            this.pageInvestments.Text = "Yatırım İşlemleri";

            // ============================================
            // STAT CARDS - RENKLİ KARTLAR
            // ============================================
            
            // Card 1: Toplam Varlık (Mavi)
            this.cardTotalAssets.Appearance.BackColor = Color.FromArgb(33, 150, 243);
            this.cardTotalAssets.Appearance.Options.UseBackColor = true;
            this.cardTotalAssets.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardTotalAssets.Size = new Size(280, 100);
            this.cardTotalAssets.Name = "cardTotalAssets";
            this.cardTotalAssets.Padding = new Padding(15);
            
            this.lblTotalAssetsTitle.Appearance.ForeColor = Color.White;
            this.lblTotalAssetsTitle.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            this.lblTotalAssetsTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblTotalAssetsTitle.Size = new Size(250, 25);
            this.lblTotalAssetsTitle.Location = new Point(15, 15);
            this.lblTotalAssetsTitle.Text = "💰 Toplam Varlıklar";
            this.lblTotalAssetsTitle.Name = "lblTotalAssetsTitle";
            
            this.lblTotalAssetsValue.Appearance.ForeColor = Color.White;
            this.lblTotalAssetsValue.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            this.lblTotalAssetsValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblTotalAssetsValue.Size = new Size(250, 45);
            this.lblTotalAssetsValue.Location = new Point(15, 45);
            this.lblTotalAssetsValue.Text = "121.325,38";
            this.lblTotalAssetsValue.Name = "lblTotalAssetsValue";
            
            this.cardTotalAssets.Controls.Add(this.lblTotalAssetsTitle);
            this.cardTotalAssets.Controls.Add(this.lblTotalAssetsValue);

            // Card 2: Günlük İşlem (Kırmızı)
            this.cardDailyTransactions.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            this.cardDailyTransactions.Appearance.Options.UseBackColor = true;
            this.cardDailyTransactions.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardDailyTransactions.Size = new Size(280, 100);
            this.cardDailyTransactions.Name = "cardDailyTransactions";
            
            this.lblDailyTransactionsTitle.Appearance.ForeColor = Color.White;
            this.lblDailyTransactionsTitle.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            this.lblDailyTransactionsTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblDailyTransactionsTitle.Size = new Size(250, 25);
            this.lblDailyTransactionsTitle.Location = new Point(15, 15);
            this.lblDailyTransactionsTitle.Text = "📊 Günlük İşlem Adedi";
            this.lblDailyTransactionsTitle.Name = "lblDailyTransactionsTitle";
            
            this.lblDailyTransactionsValue.Appearance.ForeColor = Color.White;
            this.lblDailyTransactionsValue.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            this.lblDailyTransactionsValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblDailyTransactionsValue.Size = new Size(250, 45);
            this.lblDailyTransactionsValue.Location = new Point(15, 45);
            this.lblDailyTransactionsValue.Text = "163";
            this.lblDailyTransactionsValue.Name = "lblDailyTransactionsValue";
            
            this.cardDailyTransactions.Controls.Add(this.lblDailyTransactionsTitle);
            this.cardDailyTransactions.Controls.Add(this.lblDailyTransactionsValue);

            // Card 3: Aktif Müşteri (Yeşil)
            this.cardActiveCustomers.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.cardActiveCustomers.Appearance.Options.UseBackColor = true;
            this.cardActiveCustomers.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardActiveCustomers.Size = new Size(280, 100);
            this.cardActiveCustomers.Name = "cardActiveCustomers";
            
            this.lblActiveCustomersTitle.Appearance.ForeColor = Color.White;
            this.lblActiveCustomersTitle.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            this.lblActiveCustomersTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblActiveCustomersTitle.Size = new Size(250, 25);
            this.lblActiveCustomersTitle.Location = new Point(15, 15);
            this.lblActiveCustomersTitle.Text = "👥 Aktif Müşteri Sayısı";
            this.lblActiveCustomersTitle.Name = "lblActiveCustomersTitle";
            
            this.lblActiveCustomersValue.Appearance.ForeColor = Color.White;
            this.lblActiveCustomersValue.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            this.lblActiveCustomersValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblActiveCustomersValue.Size = new Size(250, 45);
            this.lblActiveCustomersValue.Location = new Point(15, 45);
            this.lblActiveCustomersValue.Text = "130";
            this.lblActiveCustomersValue.Name = "lblActiveCustomersValue";
            
            this.cardActiveCustomers.Controls.Add(this.lblActiveCustomersTitle);
            this.cardActiveCustomers.Controls.Add(this.lblActiveCustomersValue);

            // Card 4: Döviz Kuru (Mor)
            this.cardExchangeRate.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            this.cardExchangeRate.Appearance.Options.UseBackColor = true;
            this.cardExchangeRate.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardExchangeRate.Size = new Size(280, 100);
            this.cardExchangeRate.Name = "cardExchangeRate";
            
            this.lblExchangeRateTitle.Appearance.ForeColor = Color.White;
            this.lblExchangeRateTitle.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            this.lblExchangeRateTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblExchangeRateTitle.Size = new Size(250, 25);
            this.lblExchangeRateTitle.Location = new Point(15, 15);
            this.lblExchangeRateTitle.Text = "💱 Döviz Kuru (USD/TL)";
            this.lblExchangeRateTitle.Name = "lblExchangeRateTitle";
            
            this.lblExchangeRateValue.Appearance.ForeColor = Color.White;
            this.lblExchangeRateValue.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            this.lblExchangeRateValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblExchangeRateValue.Size = new Size(250, 45);
            this.lblExchangeRateValue.Location = new Point(15, 45);
            this.lblExchangeRateValue.Text = "0,377%";
            this.lblExchangeRateValue.Name = "lblExchangeRateValue";
            
            this.cardExchangeRate.Controls.Add(this.lblExchangeRateTitle);
            this.cardExchangeRate.Controls.Add(this.lblExchangeRateValue);

            // ============================================
            // CHARTS
            // ============================================
            this.chartCurrency.Size = new Size(450, 350);
            this.chartCurrency.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            this.chartCurrency.Legend.AlignmentHorizontal = DevExpress.XtraCharts.LegendAlignmentHorizontal.Left;
            this.chartCurrency.Name = "chartCurrency";
            this.chartCurrency.BackColor = Color.FromArgb(30, 30, 30);
            this.chartCurrency.AppearanceNameSerializable = "Dark Chameleon";
            
            this.chartTransactions.Size = new Size(500, 350);
            this.chartTransactions.Name = "chartTransactions";
            this.chartTransactions.BackColor = Color.FromArgb(30, 30, 30);
            this.chartTransactions.AppearanceNameSerializable = "Dark Chameleon";

            // ============================================
            // LAYOUT CONTROL - TAM EKRAN DÜZEN
            // ============================================
            this.layoutDashboard.Dock = DockStyle.Fill;
            this.layoutDashboard.Name = "layoutDashboard";
            this.layoutDashboard.Root = this.layoutGroupRoot;
            this.layoutDashboard.Controls.Add(this.cardTotalAssets);
            this.layoutDashboard.Controls.Add(this.cardDailyTransactions);
            this.layoutDashboard.Controls.Add(this.cardActiveCustomers);
            this.layoutDashboard.Controls.Add(this.cardExchangeRate);
            this.layoutDashboard.Controls.Add(this.chartCurrency);
            this.layoutDashboard.Controls.Add(this.chartTransactions);
            
            // Layout Items
            var layoutCard1 = new LayoutControlItem(this.layoutDashboard, this.cardTotalAssets);
            layoutCard1.TextVisible = false;
            layoutCard1.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard1.MinSize = new Size(280, 100);
            
            var layoutCard2 = new LayoutControlItem(this.layoutDashboard, this.cardDailyTransactions);
            layoutCard2.TextVisible = false;
            layoutCard2.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard2.MinSize = new Size(280, 100);
            
            var layoutCard3 = new LayoutControlItem(this.layoutDashboard, this.cardActiveCustomers);
            layoutCard3.TextVisible = false;
            layoutCard3.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard3.MinSize = new Size(280, 100);
            
            var layoutCard4 = new LayoutControlItem(this.layoutDashboard, this.cardExchangeRate);
            layoutCard4.TextVisible = false;
            layoutCard4.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard4.MinSize = new Size(280, 100);
            
            var layoutChartPie = new LayoutControlItem(this.layoutDashboard, this.chartCurrency);
            layoutChartPie.TextVisible = false;
            layoutChartPie.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutChartPie.MinSize = new Size(400, 300);
            
            var layoutChartBar = new LayoutControlItem(this.layoutDashboard, this.chartTransactions);
            layoutChartBar.TextVisible = false;
            layoutChartBar.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutChartBar.MinSize = new Size(400, 300);
            
            this.layoutGroupRoot.Name = "layoutGroupRoot";
            this.layoutGroupRoot.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutGroupRoot.GroupBordersVisible = false;
            this.layoutGroupRoot.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutGroupRoot.Spacing = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);

            // ============================================
            // DASHBOARD PANEL
            // ============================================
            this.pnlDashboard.Dock = DockStyle.Fill;
            this.pnlDashboard.Appearance.BackColor = Color.FromArgb(25, 25, 25);
            this.pnlDashboard.Appearance.Options.UseBackColor = true;
            this.pnlDashboard.Controls.Add(this.layoutDashboard);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // ============================================
            // GRID CUSTOMERS
            // ============================================
            this.gridCustomers.Dock = DockStyle.Fill;
            this.gridCustomers.MainView = this.gridViewCustomers;
            this.gridCustomers.Name = "gridCustomers";
            this.gridCustomers.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gridViewCustomers });
            this.gridCustomers.Visible = false;

            this.gridViewCustomers.GridControl = this.gridCustomers;
            this.gridViewCustomers.Name = "gridViewCustomers";
            this.gridViewCustomers.OptionsView.ShowGroupPanel = false;
            this.gridViewCustomers.OptionsView.ShowAutoFilterRow = true;
            this.gridViewCustomers.OptionsBehavior.Editable = false;
            this.gridViewCustomers.OptionsView.EnableAppearanceOddRow = true;
            this.gridViewCustomers.OptionsView.EnableAppearanceEvenRow = true;

            // ============================================
            // MAIN FORM
            // ============================================
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1600, 900);
            this.Controls.Add(this.gridCustomers);
            this.Controls.Add(this.pnlDashboard);
            this.Controls.Add(this.ribbonControl1);
            this.Name = "MainForm";
            this.Ribbon = this.ribbonControl1;
            this.Text = "NovaBank - Finansal Yönetim Sistemi";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            // End Init
            ((ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((ISupportInitialize)(this.pnlDashboard)).EndInit();
            ((ISupportInitialize)(this.layoutDashboard)).EndInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).EndInit();
            ((ISupportInitialize)(this.cardTotalAssets)).EndInit();
            ((ISupportInitialize)(this.cardDailyTransactions)).EndInit();
            ((ISupportInitialize)(this.cardActiveCustomers)).EndInit();
            ((ISupportInitialize)(this.cardExchangeRate)).EndInit();
            ((ISupportInitialize)(this.chartCurrency)).EndInit();
            ((ISupportInitialize)(this.chartTransactions)).EndInit();
            ((ISupportInitialize)(this.gridCustomers)).EndInit();
            ((ISupportInitialize)(this.gridViewCustomers)).EndInit();
            this.pnlDashboard.ResumeLayout(false);
            this.layoutDashboard.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
