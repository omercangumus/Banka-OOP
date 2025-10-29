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
        private RibbonPage pageInvestments;
        private RibbonPageGroup groupStats;
        private RibbonPageGroup groupAI;
        private RibbonPageGroup groupCustomerActions;
        private RibbonPageGroup groupInvestmentActions;
        
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
        private DevExpress.XtraBars.BarButtonItem btnCards;
        private DevExpress.XtraBars.BarButtonItem btnTimeDeposit;
        private DevExpress.XtraBars.BarButtonItem btnLoanApplication;
        private DevExpress.XtraBars.BarButtonItem btnLoanApproval;
        private DevExpress.XtraBars.BarButtonItem btnLogout;
        
        // Dashboard Controls - YENİ TASARIM
        private PanelControl pnlDashboard;
        private LayoutControl layoutDashboard;
        private LayoutControlGroup layoutGroupRoot;
        
        // Stat Cards (pnl prefix per Fırat standards)
        private PanelControl pnlTotalAssets;
        private PanelControl pnlDailyTransactions;
        private PanelControl pnlActiveCustomers;
        private PanelControl pnlExchangeRate;
        
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
        private ChartControl chartBalanceHistory;
        private ChartControl chartAssetDistribution;
        
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
            
            // Apply Dark Theme - Moved to Program.cs
            // UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
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
            this.btnCards = new DevExpress.XtraBars.BarButtonItem();
            this.btnTimeDeposit = new DevExpress.XtraBars.BarButtonItem();
            this.btnLoanApplication = new DevExpress.XtraBars.BarButtonItem();
            this.btnLoanApproval = new DevExpress.XtraBars.BarButtonItem();
            this.btnLogout = new DevExpress.XtraBars.BarButtonItem();
            
            // Dashboard Panel
            this.pnlDashboard = new PanelControl();
            this.layoutDashboard = new LayoutControl();
            this.layoutGroupRoot = new LayoutControlGroup();
            
            // Stat Cards (pnl prefix per Fırat standards)
            this.pnlTotalAssets = new PanelControl();
            this.pnlDailyTransactions = new PanelControl();
            this.pnlActiveCustomers = new PanelControl();
            this.pnlExchangeRate = new PanelControl();
            
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
            this.chartBalanceHistory = new ChartControl();
            this.chartAssetDistribution = new ChartControl();
            
            // Grid
            this.gridCustomers = new GridControl();
            this.gridViewCustomers = new GridView();
            
            // Begin Init
            ((ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((ISupportInitialize)(this.pnlDashboard)).BeginInit();
            ((ISupportInitialize)(this.layoutDashboard)).BeginInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).BeginInit();
            ((ISupportInitialize)(this.pnlTotalAssets)).BeginInit();
            ((ISupportInitialize)(this.pnlDailyTransactions)).BeginInit();
            ((ISupportInitialize)(this.pnlActiveCustomers)).BeginInit();
            ((ISupportInitialize)(this.pnlExchangeRate)).BeginInit();
            ((ISupportInitialize)(this.chartCurrency)).BeginInit();
            ((ISupportInitialize)(this.chartTransactions)).BeginInit();
            ((ISupportInitialize)(this.chartBalanceHistory)).BeginInit();
            ((ISupportInitialize)(this.chartAssetDistribution)).BeginInit();
            ((ISupportInitialize)(this.gridCustomers)).BeginInit();
            ((ISupportInitialize)(this.gridViewCustomers)).BeginInit();
            this.pnlDashboard.SuspendLayout();
            this.layoutDashboard.SuspendLayout();
            this.SuspendLayout();

            // ============================================
            // RIBBON CONTROL - MODERN OFFİCE 2019 STİL
            // ============================================
            // this.ribbonControl1.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2019;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
                this.btnAiAssist, this.btnRefresh, this.btnMoneyTransfer,
                this.btnAddCustomer, this.btnEditCustomer, this.btnDeleteCustomer,
                this.btnCustomerAccounts, this.btnExportExcel, this.btnExportPdf,
                this.btnAuditLogs, this.btnStockMarket, this.btnBES, this.btnCards,
                this.btnTimeDeposit, this.btnLoanApplication, this.btnLoanApproval, this.btnLogout
            });
            this.ribbonControl1.Location = new Point(0, 0);
            // this.ribbonControl1.MaxItemId = 18;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Pages.AddRange(new RibbonPage[] {
                this.pageDashboard, this.pageCustomers, this.pageInvestments
            });
            this.ribbonControl1.Size = new Size(1400, 130);

            // Button Configurations - MODERN İKONLAR
            this.btnAiAssist.Caption = "🤖 AI Asistan";
            this.btnAiAssist.Id = 1;
            this.btnAiAssist.Name = "btnAiAssist";
            this.btnAiAssist.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnAiAssist.ImageOptions.Image = CreateIconImage(Color.FromArgb(156, 39, 176), "AI");
            this.btnAiAssist.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAiAssist_ItemClick);

            this.btnRefresh.Caption = "🔄 Yenile";
            this.btnRefresh.Id = 2;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnRefresh.ImageOptions.Image = CreateIconImage(Color.FromArgb(76, 175, 80), "↻");

            this.btnMoneyTransfer.Caption = "💸 Para Transferi";
            this.btnMoneyTransfer.Id = 3;
            this.btnMoneyTransfer.Name = "btnMoneyTransfer";
            this.btnMoneyTransfer.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnMoneyTransfer.ImageOptions.Image = CreateIconImage(Color.FromArgb(33, 150, 243), "₺");
            this.btnMoneyTransfer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMoneyTransfer_ItemClick);

            this.btnAddCustomer.Caption = "➕ Yeni Müşteri";
            this.btnAddCustomer.Id = 4;
            this.btnAddCustomer.Name = "btnAddCustomer";
            this.btnAddCustomer.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnAddCustomer.ImageOptions.Image = CreateIconImage(Color.FromArgb(76, 175, 80), "+");
            this.btnAddCustomer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddCustomer_ItemClick);

            this.btnEditCustomer.Caption = "✏️ Düzenle";
            this.btnEditCustomer.Id = 5;
            this.btnEditCustomer.Name = "btnEditCustomer";
            this.btnEditCustomer.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnEditCustomer.ImageOptions.Image = CreateIconImage(Color.FromArgb(255, 152, 0), "✎");
            this.btnEditCustomer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnEditCustomer_ItemClick);

            this.btnDeleteCustomer.Caption = "🗑️ Sil";
            this.btnDeleteCustomer.Id = 6;
            this.btnDeleteCustomer.Name = "btnDeleteCustomer";
            this.btnDeleteCustomer.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnDeleteCustomer.ImageOptions.Image = CreateIconImage(Color.FromArgb(244, 67, 54), "✕");
            this.btnDeleteCustomer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteCustomer_ItemClick);

            this.btnCustomerAccounts.Caption = "🏦 Hesaplar";
            this.btnCustomerAccounts.Id = 7;
            this.btnCustomerAccounts.Name = "btnCustomerAccounts";
            this.btnCustomerAccounts.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnCustomerAccounts.ImageOptions.Image = CreateIconImage(Color.FromArgb(0, 150, 136), "$");
            this.btnCustomerAccounts.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnCustomerAccounts_ItemClick);

            this.btnExportExcel.Caption = "📊 Excel";
            this.btnExportExcel.Id = 8;
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnExportExcel.ImageOptions.Image = CreateIconImage(Color.FromArgb(76, 175, 80), "X");
            this.btnExportExcel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExportExcel_ItemClick);

            this.btnExportPdf.Caption = "📄 PDF";
            this.btnExportPdf.Id = 9;
            this.btnExportPdf.Name = "btnExportPdf";
            this.btnExportPdf.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnExportPdf.ImageOptions.Image = CreateIconImage(Color.FromArgb(244, 67, 54), "P");
            this.btnExportPdf.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExportPdf_ItemClick);

            this.btnAuditLogs.Caption = "📋 Sistem Logları";
            this.btnAuditLogs.Id = 10;
            this.btnAuditLogs.Name = "btnAuditLogs";
            this.btnAuditLogs.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnAuditLogs.ImageOptions.Image = CreateIconImage(Color.FromArgb(96, 125, 139), "L");
            this.btnAuditLogs.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAuditLogs_ItemClick);

            // YENİ: Yatırım Butonları - MODERN İKONLAR
            this.btnStockMarket.Caption = "📈 Borsa";
            this.btnStockMarket.Id = 11;
            this.btnStockMarket.Name = "btnStockMarket";
            this.btnStockMarket.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnStockMarket.ImageOptions.Image = CreateIconImage(Color.FromArgb(33, 150, 243), "↗");
            this.btnStockMarket.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnStockMarket_ItemClick);

            this.btnBES.Caption = "BES Başvurusu";
            this.btnBES.Id = 12;
            this.btnBES.Name = "btnBES";
            this.btnBES.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnBES.ImageOptions.Image = CreateIconImage(Color.FromArgb(156, 39, 176), "E");
            this.btnBES.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnBES_ItemClick);

            this.btnCards.Caption = "💳 Kartlarım";
            this.btnCards.Id = 13;
            this.btnCards.Name = "btnCards";
            this.btnCards.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnCards.ImageOptions.Image = CreateIconImage(Color.FromArgb(255, 193, 7), "$");
            this.btnCards.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnCards_ItemClick);

            this.btnTimeDeposit.Caption = "🏦 Vadeli Hesap";
            this.btnTimeDeposit.Id = 14;
            this.btnTimeDeposit.Name = "btnTimeDeposit";
            this.btnTimeDeposit.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnTimeDeposit.ImageOptions.Image = CreateIconImage(Color.FromArgb(0, 150, 136), "V");
            this.btnTimeDeposit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnTimeDeposit_ItemClick);

            this.btnLoanApplication.Caption = "💰 Kredi Başvurusu";
            this.btnLoanApplication.Id = 15;
            this.btnLoanApplication.Name = "btnLoanApplication";
            this.btnLoanApplication.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnLoanApplication.ImageOptions.Image = CreateIconImage(Color.FromArgb(76, 175, 80), "K");
            this.btnLoanApplication.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLoanApplication_ItemClick);

            this.btnLoanApproval.Caption = "✅ Kredi Onay (Admin)";
            this.btnLoanApproval.Id = 16;
            this.btnLoanApproval.Name = "btnLoanApproval";
            this.btnLoanApproval.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnLoanApproval.ImageOptions.Image = CreateIconImage(Color.FromArgb(244, 67, 54), "A");
            this.btnLoanApproval.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLoanApproval_ItemClick);

            this.btnLogout.Caption = "🚪 Çıkış Yap";
            this.btnLogout.Id = 17;
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            this.btnLogout.ImageOptions.Image = CreateIconImage(Color.FromArgb(120, 120, 130), "X");
            this.btnLogout.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLogout_ItemClick);

            // Ribbon Page Groups - YENİDEN DÜZENLENMIŞ
            this.groupStats.ItemLinks.Add(this.btnRefresh);
            this.groupStats.ItemLinks.Add(this.btnMoneyTransfer);
            this.groupStats.Name = "groupStats";
            this.groupStats.Text = "💼 Temel İşlemler";

            this.groupAI.ItemLinks.Add(this.btnAiAssist);
            this.groupAI.Name = "groupAI";
            this.groupAI.Text = "🤖 Yapay Zeka";

            this.groupCustomerActions.ItemLinks.Add(this.btnAddCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnEditCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnDeleteCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnCustomerAccounts);
            this.groupCustomerActions.Name = "groupCustomerActions";
            this.groupCustomerActions.Text = "👥 Müşteri İşlemleri";

            // Yeni Grup: Raporlar
            var groupReports = new RibbonPageGroup("📊 Raporlar");
            groupReports.ItemLinks.Add(this.btnExportExcel);
            groupReports.ItemLinks.Add(this.btnExportPdf);
            groupReports.ItemLinks.Add(this.btnAuditLogs);

            this.groupInvestmentActions.ItemLinks.Add(this.btnStockMarket);
            this.groupInvestmentActions.ItemLinks.Add(this.btnBES);
            this.groupInvestmentActions.ItemLinks.Add(this.btnCards);
            this.groupInvestmentActions.ItemLinks.Add(this.btnTimeDeposit);
            this.groupInvestmentActions.ItemLinks.Add(this.btnLoanApplication);
            this.groupInvestmentActions.ItemLinks.Add(this.btnLoanApproval);
            this.groupInvestmentActions.Name = "groupInvestmentActions";
            this.groupInvestmentActions.Text = "💳 Yatırım & Kredi";

            // Logout grubu
            var groupLogout = new RibbonPageGroup();
            groupLogout.ItemLinks.Add(this.btnLogout);
            groupLogout.Name = "groupLogout";
            groupLogout.Text = "🚪 Oturum";

            // Ribbon Pages - YENİ GRUPLARLA
            this.pageDashboard.Groups.Add(this.groupStats);
            this.pageDashboard.Groups.Add(this.groupAI);
            this.pageDashboard.Groups.Add(groupLogout); // Logout eklendi
            this.pageDashboard.Name = "pageDashboard";
            this.pageDashboard.Text = "🏠 Genel Bakış";

            this.pageCustomers.Groups.Add(this.groupCustomerActions);
            this.pageCustomers.Groups.Add(groupReports);
            this.pageCustomers.Name = "pageCustomers";
            this.pageCustomers.Text = "👥 Müşteriler";

            this.pageInvestments.Groups.Add(this.groupInvestmentActions);
            this.pageInvestments.Name = "pageInvestments";
            this.pageInvestments.Text = "📈 Yatırım İşlemleri";

            // ============================================
            // STAT CARDS - RENKLİ KARTLAR
            // ============================================
            
            // Card 1: Toplam Varlık (Mavi)
            this.pnlTotalAssets.Appearance.BackColor = Color.FromArgb(33, 150, 243);
            this.pnlTotalAssets.Appearance.Options.UseBackColor = true;
            this.pnlTotalAssets.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlTotalAssets.Size = new Size(280, 100);
            this.pnlTotalAssets.Name = "pnlTotalAssets";
            this.pnlTotalAssets.Padding = new Padding(15);
            
            this.lblTotalAssetsTitle.Appearance.ForeColor = Color.White;
            this.lblTotalAssetsTitle.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblTotalAssetsTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblTotalAssetsTitle.Size = new Size(250, 25);
            this.lblTotalAssetsTitle.Location = new Point(15, 15);
            this.lblTotalAssetsTitle.Text = "💰 Toplam Varlıklar";
            this.lblTotalAssetsTitle.Name = "lblTotalAssetsTitle";
            
            this.lblTotalAssetsValue.Appearance.ForeColor = Color.White;
            this.lblTotalAssetsValue.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblTotalAssetsValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblTotalAssetsValue.Size = new Size(250, 45);
            this.lblTotalAssetsValue.Location = new Point(15, 45);
            this.lblTotalAssetsValue.Text = "121.325,38";
            this.lblTotalAssetsValue.Name = "lblTotalAssetsValue";
            
            this.pnlTotalAssets.Controls.Add(this.lblTotalAssetsTitle);
            this.pnlTotalAssets.Controls.Add(this.lblTotalAssetsValue);

            // Card 2: Günlük İşlem (Kırmızı)
            this.pnlDailyTransactions.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            this.pnlDailyTransactions.Appearance.Options.UseBackColor = true;
            this.pnlDailyTransactions.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlDailyTransactions.Size = new Size(280, 100);
            this.pnlDailyTransactions.Name = "pnlDailyTransactions";
            
            this.lblDailyTransactionsTitle.Appearance.ForeColor = Color.White;
            this.lblDailyTransactionsTitle.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblDailyTransactionsTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblDailyTransactionsTitle.Size = new Size(250, 25);
            this.lblDailyTransactionsTitle.Location = new Point(15, 15);
            this.lblDailyTransactionsTitle.Text = "📊 Günlük İşlem Adedi";
            this.lblDailyTransactionsTitle.Name = "lblDailyTransactionsTitle";
            
            this.lblDailyTransactionsValue.Appearance.ForeColor = Color.White;
            this.lblDailyTransactionsValue.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblDailyTransactionsValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblDailyTransactionsValue.Size = new Size(250, 45);
            this.lblDailyTransactionsValue.Location = new Point(15, 45);
            this.lblDailyTransactionsValue.Text = "163";
            this.lblDailyTransactionsValue.Name = "lblDailyTransactionsValue";
            
            this.pnlDailyTransactions.Controls.Add(this.lblDailyTransactionsTitle);
            this.pnlDailyTransactions.Controls.Add(this.lblDailyTransactionsValue);

            // Card 3: Aktif Müşteri (Yeşil)
            this.pnlActiveCustomers.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.pnlActiveCustomers.Appearance.Options.UseBackColor = true;
            this.pnlActiveCustomers.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlActiveCustomers.Size = new Size(280, 100);
            this.pnlActiveCustomers.Name = "pnlActiveCustomers";
            
            this.lblActiveCustomersTitle.Appearance.ForeColor = Color.White;
            this.lblActiveCustomersTitle.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblActiveCustomersTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblActiveCustomersTitle.Size = new Size(250, 25);
            this.lblActiveCustomersTitle.Location = new Point(15, 15);
            this.lblActiveCustomersTitle.Text = "👥 Aktif Müşteri Sayısı";
            this.lblActiveCustomersTitle.Name = "lblActiveCustomersTitle";
            
            this.lblActiveCustomersValue.Appearance.ForeColor = Color.White;
            this.lblActiveCustomersValue.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblActiveCustomersValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblActiveCustomersValue.Size = new Size(250, 45);
            this.lblActiveCustomersValue.Location = new Point(15, 45);
            this.lblActiveCustomersValue.Text = "130";
            this.lblActiveCustomersValue.Name = "lblActiveCustomersValue";
            
            this.pnlActiveCustomers.Controls.Add(this.lblActiveCustomersTitle);
            this.pnlActiveCustomers.Controls.Add(this.lblActiveCustomersValue);

            // Card 4: Döviz Kuru (Mor)
            this.pnlExchangeRate.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            this.pnlExchangeRate.Appearance.Options.UseBackColor = true;
            this.pnlExchangeRate.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlExchangeRate.Size = new Size(280, 100);
            this.pnlExchangeRate.Name = "pnlExchangeRate";
            
            this.lblExchangeRateTitle.Appearance.ForeColor = Color.White;
            this.lblExchangeRateTitle.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblExchangeRateTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblExchangeRateTitle.Size = new Size(250, 25);
            this.lblExchangeRateTitle.Location = new Point(15, 15);
            this.lblExchangeRateTitle.Text = "💱 Döviz Kuru (USD/TL)";
            this.lblExchangeRateTitle.Name = "lblExchangeRateTitle";
            
            this.lblExchangeRateValue.Appearance.ForeColor = Color.White;
            this.lblExchangeRateValue.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold); 
            this.lblExchangeRateValue.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblExchangeRateValue.Size = new Size(250, 45);
            this.lblExchangeRateValue.Location = new Point(15, 45);
            this.lblExchangeRateValue.Text = "0,377%";
            this.lblExchangeRateValue.Name = "lblExchangeRateValue";
            
            this.pnlExchangeRate.Controls.Add(this.lblExchangeRateTitle);
            this.pnlExchangeRate.Controls.Add(this.lblExchangeRateValue);

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
            this.layoutDashboard.Controls.Add(this.pnlTotalAssets);
            this.layoutDashboard.Controls.Add(this.pnlDailyTransactions);
            this.layoutDashboard.Controls.Add(this.pnlActiveCustomers);
            this.layoutDashboard.Controls.Add(this.pnlExchangeRate);
            this.layoutDashboard.Controls.Add(this.chartCurrency);
            this.layoutDashboard.Controls.Add(this.chartTransactions);
            this.layoutDashboard.Controls.Add(this.chartBalanceHistory);
            this.layoutDashboard.Controls.Add(this.chartAssetDistribution);
            
            // Chart Settings
            this.chartBalanceHistory.BackColor = Color.FromArgb(30, 30, 30);
            this.chartBalanceHistory.AppearanceNameSerializable = "Dark Chameleon";
            this.chartBalanceHistory.Name = "chartBalanceHistory";
            
            this.chartAssetDistribution.BackColor = Color.FromArgb(30, 30, 30);
            this.chartAssetDistribution.AppearanceNameSerializable = "Dark Chameleon";
            this.chartAssetDistribution.Name = "chartAssetDistribution";
            
            // Layout Items
            var layoutCard1 = new LayoutControlItem(this.layoutDashboard, this.pnlTotalAssets);
            layoutCard1.TextVisible = false;
            layoutCard1.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard1.MinSize = new Size(280, 100);
            
            var layoutCard2 = new LayoutControlItem(this.layoutDashboard, this.pnlDailyTransactions);
            layoutCard2.TextVisible = false;
            layoutCard2.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard2.MinSize = new Size(280, 100);
            
            var layoutCard3 = new LayoutControlItem(this.layoutDashboard, this.pnlActiveCustomers);
            layoutCard3.TextVisible = false;
            layoutCard3.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard3.MinSize = new Size(280, 100);
            
            var layoutCard4 = new LayoutControlItem(this.layoutDashboard, this.pnlExchangeRate);
            layoutCard4.TextVisible = false;
            layoutCard4.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutCard4.MinSize = new Size(280, 100);
            
            var layoutChartPie = new LayoutControlItem(this.layoutDashboard, this.chartCurrency);
            layoutChartPie.TextVisible = false;
            layoutChartPie.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutChartPie.MinSize = new Size(300, 250);
            
            var layoutChartBar = new LayoutControlItem(this.layoutDashboard, this.chartTransactions);
            layoutChartBar.TextVisible = false;
            layoutChartBar.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutChartBar.MinSize = new Size(300, 250);

            // Yeni Grafikler için Layout Items
            var layoutChartLine = new LayoutControlItem(this.layoutDashboard, this.chartBalanceHistory);
            layoutChartLine.TextVisible = false;
            layoutChartLine.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutChartLine.MinSize = new Size(300, 250);

            var layoutChartTree = new LayoutControlItem(this.layoutDashboard, this.chartAssetDistribution);
            layoutChartTree.TextVisible = false;
            layoutChartTree.SizeConstraintsType = SizeConstraintsType.Custom;
            layoutChartTree.MinSize = new Size(300, 250);

            // Layout Grouping (2x2 Grid for Charts)
            // Row 1 Charts: Currency + Transactions
            var groupChartsRow1 = this.layoutGroupRoot.AddGroup();
            groupChartsRow1.GroupBordersVisible = false;
            groupChartsRow1.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
            groupChartsRow1.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
            groupChartsRow1.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
            groupChartsRow1.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Percent, Height = 100 });
            
            groupChartsRow1.AddItem(layoutChartPie);
            layoutChartPie.OptionsTableLayoutItem.RowIndex = 0;
            layoutChartPie.OptionsTableLayoutItem.ColumnIndex = 0;
            
            groupChartsRow1.AddItem(layoutChartBar);
            layoutChartBar.OptionsTableLayoutItem.RowIndex = 0;
            layoutChartBar.OptionsTableLayoutItem.ColumnIndex = 1;

            // Row 2 Charts: Balance History + Asset Distribution
            var groupChartsRow2 = this.layoutGroupRoot.AddGroup();
            groupChartsRow2.GroupBordersVisible = false;
            groupChartsRow2.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
            groupChartsRow2.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
            groupChartsRow2.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
            groupChartsRow2.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Percent, Height = 100 });

            groupChartsRow2.AddItem(layoutChartLine);
            layoutChartLine.OptionsTableLayoutItem.RowIndex = 0;
            layoutChartLine.OptionsTableLayoutItem.ColumnIndex = 0;

            groupChartsRow2.AddItem(layoutChartTree);
            layoutChartTree.OptionsTableLayoutItem.RowIndex = 0;
            layoutChartTree.OptionsTableLayoutItem.ColumnIndex = 1;
            
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
            this.gridViewCustomers.Appearance.Row.Font = new Font("Tahoma", 8.25F);
            this.gridViewCustomers.Appearance.HeaderPanel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);

            // ============================================
            // MAIN FORM
            // ============================================
            this.AutoScaleDimensions = new SizeF(7F, 16F); 
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1600, 900);
            this.Controls.Add(this.gridCustomers);
            this.Controls.Add(this.pnlDashboard);
            this.Controls.Add(this.ribbonControl1);
            this.Name = "MainForm";
            // this.Ribbon = this.ribbonControl1;
            this.Text = "NovaBank - Finansal Yönetim Sistemi";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            // End Init
            ((ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((ISupportInitialize)(this.pnlDashboard)).EndInit();
            ((ISupportInitialize)(this.layoutDashboard)).EndInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).EndInit();
            ((ISupportInitialize)(this.pnlTotalAssets)).EndInit();
            ((ISupportInitialize)(this.pnlDailyTransactions)).EndInit();
            ((ISupportInitialize)(this.pnlActiveCustomers)).EndInit();
            ((ISupportInitialize)(this.pnlExchangeRate)).EndInit();
            ((ISupportInitialize)(this.chartCurrency)).EndInit();
            ((ISupportInitialize)(this.chartTransactions)).EndInit();
            ((ISupportInitialize)(this.gridCustomers)).EndInit();
            ((ISupportInitialize)(this.gridViewCustomers)).EndInit();
            this.pnlDashboard.ResumeLayout(false);
            this.layoutDashboard.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Helper method - Create colorful icon for Ribbon buttons
        private Image CreateIconImage(Color backgroundColor, string symbol)
        {
            var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Draw rounded rectangle background
                using (var brush = new SolidBrush(backgroundColor))
                {
                    g.FillEllipse(brush, 2, 2, 28, 28);
                }
                
                // Draw symbol
                using (var font = new Font("Tahoma", 10F, FontStyle.Bold)) // 10F Bold per instructions
                using (var brush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(symbol, font, brush, new RectangleF(0, 0, 32, 32), sf);
                }
            }
            return bmp;
        }
    }
}
