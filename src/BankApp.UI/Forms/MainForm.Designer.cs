using System.ComponentModel;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraCharts;

namespace BankApp.UI.Forms
{
    partial class MainForm
    {
        private IContainer components = null;
        private RibbonControl ribbonControl1;
        private RibbonPage pageDashboard;
        private RibbonPageGroup groupStats;
        private RibbonPageGroup groupAI;
        private RibbonPage pageCustomers;
        private RibbonPageGroup groupCustomerActions;
        
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





        
        // Dashboard Controls
        private PanelControl pnlDashboard;
        private ChartControl chartCurrency;
        private ChartControl chartTransactions;
        private TileControl tileControlStats;
        private TileGroup tileGroup1;
        private TileItem tileItemTotalBalance;
        private TileItem tileItemTotalCustomers;
        
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
            this.ribbonControl1 = new RibbonControl();
            this.pageDashboard = new RibbonPage("Genel Bakış");
            this.groupStats = new RibbonPageGroup("Durum");
            this.groupAI = new RibbonPageGroup("AI");
            this.btnAiAssist = new DevExpress.XtraBars.BarButtonItem();
            this.btnRefresh = new DevExpress.XtraBars.BarButtonItem();
            
            this.pnlDashboard = new PanelControl();
            this.chartCurrency = new ChartControl();
            this.chartTransactions = new ChartControl();
            this.tileControlStats = new TileControl();
            this.tileGroup1 = new TileGroup();
            this.tileItemTotalBalance = new TileItem();
            this.tileItemTotalCustomers = new TileItem();
            this.btnMoneyTransfer = new DevExpress.XtraBars.BarButtonItem(); 
            
            this.btnAddCustomer = new DevExpress.XtraBars.BarButtonItem();
            this.btnEditCustomer = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteCustomer = new DevExpress.XtraBars.BarButtonItem();
            this.btnCustomerAccounts = new DevExpress.XtraBars.BarButtonItem();
            this.btnExportExcel = new DevExpress.XtraBars.BarButtonItem();
            this.btnExportPdf = new DevExpress.XtraBars.BarButtonItem();
            this.btnAuditLogs = new DevExpress.XtraBars.BarButtonItem();

            this.gridCustomers = new GridControl();




            this.gridViewCustomers = new GridView();
            ((ISupportInitialize)(this.gridCustomers)).BeginInit();
            ((ISupportInitialize)(this.gridViewCustomers)).BeginInit();



            ((ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((ISupportInitialize)(this.pnlDashboard)).BeginInit();
            this.pnlDashboard.SuspendLayout();
            ((ISupportInitialize)(this.chartCurrency)).BeginInit();
            ((ISupportInitialize)(this.chartTransactions)).BeginInit();
            this.SuspendLayout();

            // RibbonControl
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem,
            this.btnAiAssist,
            this.btnRefresh,
            this.btnMoneyTransfer,
            this.btnAddCustomer,
            this.btnEditCustomer,
            this.btnDeleteCustomer,
            this.btnCustomerAccounts,
            this.btnExportExcel,
            this.btnExportPdf,
            this.btnAuditLogs});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 11;





            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Pages.AddRange(new RibbonPage[] {
            this.pageDashboard,
            this.pageCustomers});
            this.ribbonControl1.Size = new System.Drawing.Size(1200, 160);

            
            // Buttons
            this.btnAiAssist.Caption = "AI Asistan";
            this.btnAiAssist.Id = 1;
            this.btnAiAssist.ImageOptions.SvgImage = null; // Add SVG later
            this.btnAiAssist.Name = "btnAiAssist";
            this.btnAiAssist.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAiAssist_ItemClick);

            this.btnRefresh.Caption = "Yenile";
            this.btnRefresh.Id = 2;
            this.btnRefresh.Name = "btnRefresh";

            this.btnMoneyTransfer.Caption = "Para Transferi";
            this.btnMoneyTransfer.Id = 3;
            this.btnMoneyTransfer.Name = "btnMoneyTransfer";
            this.btnMoneyTransfer.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMoneyTransfer_ItemClick);

            // Customer Buttons
            this.btnAddCustomer.Caption = "Yeni Müşteri";
            this.btnAddCustomer.Id = 4;
            this.btnAddCustomer.Name = "btnAddCustomer";
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
            this.btnExportExcel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExportExcel_ItemClick);

            this.btnExportPdf.Caption = "PDF'e Aktar";
            this.btnExportPdf.Id = 9;
            this.btnExportPdf.Name = "btnExportPdf";
            this.btnExportPdf.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExportPdf_ItemClick);

            this.btnAuditLogs.Caption = "Sistem Logları";
            this.btnAuditLogs.Id = 10;
            this.btnAuditLogs.Name = "btnAuditLogs";
            this.btnAuditLogs.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAuditLogs_ItemClick);

            // Groups
            this.groupStats.ItemLinks.Add(this.btnRefresh);
            this.groupStats.ItemLinks.Add(this.btnMoneyTransfer);
            this.groupStats.ItemLinks.Add(this.btnAuditLogs);

            this.groupAI.ItemLinks.Add(this.btnAiAssist);
            
            this.groupCustomerActions.ItemLinks.Add(this.btnAddCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnEditCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnDeleteCustomer);
            this.groupCustomerActions.ItemLinks.Add(this.btnCustomerAccounts);
            this.groupCustomerActions.ItemLinks.Add(this.btnExportExcel);
            this.groupCustomerActions.ItemLinks.Add(this.btnExportPdf);




            this.pageDashboard.Groups.Add(this.groupStats);
            this.pageDashboard.Groups.Add(this.groupAI);

            // Dashboard Panel
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Controls.Add(this.chartTransactions);
            this.pnlDashboard.Controls.Add(this.chartCurrency);
            this.pnlDashboard.Controls.Add(this.tileControlStats);
            this.pnlDashboard.Location = new System.Drawing.Point(0, 160);
            this.pnlDashboard.Name = "pnlDashboard";
            
            // TileControl
            this.tileControlStats.Dock = System.Windows.Forms.DockStyle.Top;
            this.tileControlStats.Groups.Add(this.tileGroup1);
            this.tileControlStats.Height = 150;
            this.tileControlStats.Name = "tileControlStats";
            
            // Tile Items
            this.tileItemTotalBalance.AppearanceItem.Normal.BackColor = System.Drawing.Color.Teal;
            this.tileItemTotalBalance.Text = "Toplam Mevduat";
            this.tileItemTotalBalance.Text2 = "1.500.000 TL";
            
            this.tileItemTotalCustomers.AppearanceItem.Normal.BackColor = System.Drawing.Color.DarkOrange;
            this.tileItemTotalCustomers.Text = "Müşteri Sayısı";
            this.tileItemTotalCustomers.Text2 = "1.250";
            
            this.tileGroup1.Items.Add(this.tileItemTotalBalance);
            this.tileGroup1.Items.Add(this.tileItemTotalCustomers);

            // Charts
            this.chartCurrency.Dock = System.Windows.Forms.DockStyle.Left;
            this.chartCurrency.Width = 500;
            this.chartCurrency.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            
            this.chartTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
            
            // Grid Customers
            this.gridCustomers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCustomers.MainView = this.gridViewCustomers;
            this.gridCustomers.Name = "gridCustomers";
            this.gridCustomers.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewCustomers});
            this.gridCustomers.Visible = false; // Hidden by default

            // Grid View Settings
            this.gridViewCustomers.GridControl = this.gridCustomers;
            this.gridViewCustomers.Name = "gridViewCustomers";
            this.gridViewCustomers.OptionsView.ShowGroupPanel = false;
            this.gridViewCustomers.OptionsView.ShowAutoFilterRow = true;
            this.gridViewCustomers.OptionsBehavior.Editable = false;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.gridCustomers); // Add Grid
            this.Controls.Add(this.pnlDashboard); 
            this.Controls.Add(this.ribbonControl1);

            this.Name = "MainForm";
            this.Ribbon = this.ribbonControl1;
            this.Text = "NovaBank - Finansal Yönetim Sistemi";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            ((ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((ISupportInitialize)(this.pnlDashboard)).EndInit();
            this.pnlDashboard.ResumeLayout(false);
            ((ISupportInitialize)(this.chartCurrency)).EndInit();
            ((ISupportInitialize)(this.chartTransactions)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
