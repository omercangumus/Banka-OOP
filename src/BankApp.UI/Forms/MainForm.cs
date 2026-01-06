using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.XtraLayout;
using DevExpress.LookAndFeel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using BankApp.Core.Interfaces;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Services.Dashboard;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Events;
using BankApp.UI.Controls;
using BankApp.UI.Services;
using Dapper;

namespace BankApp.UI.Forms
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly IAIService _aiService;
        private readonly IDashboardService _dashboardService;


        // Dashboard Widgets
        private HeroNetWorthCard heroCard;
        private InvestmentOpportunitiesWidget opportunitiesWidget;
        private QuickActionsBar quickActions;
        private RecentTransactionsWidget recentTransactions;
        private AssetAllocationChart assetChart;
        private AdminDashboardPanel adminPanel;
        private PortfolioView portfolioView;
        
        // Portfolio Dashboard (for Tab2)
        private PanelControl pnlPortfolioDashboard;
        private LayoutControl layoutPortfolioDashboard;
        private LayoutControlGroup layoutPortfolioGroupRoot;
        private HeroNetWorthCard portfolioHeroCard;
        private InvestmentOpportunitiesWidget portfolioOpportunitiesWidget;
        private QuickActionsBar portfolioQuickActions;
        private RecentTransactionsWidget portfolioRecentTransactions;
        private AssetAllocationChart portfolioAssetChart;
        private readonly DashboardSummaryService _dashboardSummaryService;
        
        // Guards to prevent multiple setup
        private bool _dashboardInitialized = false;
        private bool _portfolioDashboardInitialized = false;

        public MainForm()
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] OPENED: {GetType().FullName}");
            
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeComponent Error: {ex.Message}");
                // Continue execution to attempt showing the form
            }
            
            // BUILD MARKER - NO EXCUSES VALIDATION
            string buildTime = DateTime.Now.ToString("HH:mm:ss");
            this.Text = $"NovaBank DEBUG MARKER {buildTime}";
            System.Diagnostics.Debug.WriteLine($"=== MAINFORM LOADED v2 @ {buildTime} ===");
            System.Diagnostics.Debug.WriteLine($"=== EXE PATH: {System.Reflection.Assembly.GetExecutingAssembly().Location} ===");
            // Use real AI service with environment variable
            string apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "your-api-key-here";
            _aiService = new OpenRouterAIService(apiKey);
            
            // Initialize Dashboard Service
            var context = new DapperContext();
            _dashboardService = new DashboardService(context);
            _dashboardSummaryService = new DashboardSummaryService(context);
            
            InitializeInvestmentDashboard();

            this.ribbonControl1.SelectedPageChanged += RibbonControl1_SelectedPageChanged;
            this.Load += MainForm_Load;
            
            // Event sistemine abone ol
            SubscribeToEvents();
            
            // DashboardRefreshOrchestrator entegrasyonu
            DashboardRefreshOrchestrator.Instance.DashboardRefreshed += OnDashboardRefreshRequested;
            
            // Portfolio events için abone ol
            PortfolioEvents.PortfolioChanged += (s, e) => {
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] PortfolioChanged received: UserId={e.UserId}, ChangeType={e.ChangeType}");
                if (e.UserId == AppEvents.CurrentSession.UserId)
                {
                    System.Diagnostics.Debug.WriteLine("[DASHBOARD] Triggering refresh for Investment");
                    _ = DashboardRefreshOrchestrator.Instance.RequestRefreshAsync(e.UserId, RefreshReason.Investment);
                }
            };
            PortfolioEvents.TransactionChanged += (s, e) => {
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] TransactionChanged received: UserId={e.UserId}, Amount={e.Amount}");
                if (e.UserId == AppEvents.CurrentSession.UserId)
                {
                    System.Diagnostics.Debug.WriteLine("[DASHBOARD] Triggering refresh for Transfer");
                    _ = DashboardRefreshOrchestrator.Instance.RequestRefreshAsync(e.UserId, RefreshReason.Transfer);
                }
            };
            PortfolioEvents.LoanChanged += (s, e) => {
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] LoanChanged received: UserId={e.UserId}");
                if (e.UserId == AppEvents.CurrentSession.UserId)
                {
                    System.Diagnostics.Debug.WriteLine("[DASHBOARD] Triggering refresh for LoanPayment");
                    _ = DashboardRefreshOrchestrator.Instance.RequestRefreshAsync(e.UserId, RefreshReason.LoanPayment);
                }
            };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] MainForm_Load fired, this={GetType().FullName}");
            
            // Role-Based Dashboard
            if (AppEvents.CurrentSession.IsAdmin)
            {
                ShowAdminDashboard();
            }
            else
            {
                ShowCustomerDashboard();
            }
            
            UpdateMenuForRole();
        }

        private void ShowAdminDashboard()
        {
            try
            {
                // Hide all customer tabs and widgets
                if (pnlDashboard != null) pnlDashboard.Visible = false;
                if (quickActions != null) quickActions.Visible = false;
                if (recentTransactions != null) recentTransactions.Visible = false;
                if (assetChart != null) assetChart.Visible = false;
                
                // Show admin panel as separate form
                var adminForm = new AdminDashboardForm();
                adminForm.Show();
                this.Hide();
                
                // When admin form closes, show main form again
                adminForm.FormClosed += (s, args) => {
                    this.Show();
                };
            }
            catch (Exception ex)
            {
                // Show error instead of crashing
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    $"Admin paneli açılırken hata oluştu:\n\n{ex.Message}\n\nDetay: {ex.InnerException?.Message ?? "Yok"}",
                    "Admin Panel Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                
                // Log to debug output
                System.Diagnostics.Debug.WriteLine($"AdminDashboard Error: {ex}");
                
                // Show customer dashboard as fallback
                ShowCustomerDashboard();
            }
        }

        private void ShowCustomerDashboard()
        {
            LoadDashboardData();
            LoadDashboardCharts();
            LoadCustomers();
            SetupFintechProDashboard();
            SetupPortfolioDashboard(); // Setup portfolio dashboard (same as Tab1)
        }

        private void SetupFintechProDashboard()
        {
            System.Diagnostics.Debug.WriteLine($"[SETUP] SetupFintechProDashboard called, _dashboardInitialized={_dashboardInitialized}");
            
            try {
                if (layoutDashboard == null) return;
                
                // Guard: Only setup once
                if (_dashboardInitialized)
                {
                    System.Diagnostics.Debug.WriteLine("[SETUP] Dashboard already initialized, skipping setup, just updating data");
                    UpdateHeroCard();
                    UpdateAssetChart();
                    return;
                }
                _dashboardInitialized = true;
                
                layoutDashboard.BeginUpdate();
                
                // **REMOVE ALL OLD CONTROLS FROM LAYOUT**
                layoutDashboard.Controls.Clear();
                layoutGroupRoot.Items.Clear();
                
                // Background
                pnlDashboard.Appearance.BackColor = Color.FromArgb(18, 18, 18);
                
                // Create widgets
                heroCard = new HeroNetWorthCard();
                opportunitiesWidget = new InvestmentOpportunitiesWidget();
                quickActions = new QuickActionsBar();
                recentTransactions = new RecentTransactionsWidget();
                assetChart = new AssetAllocationChart();
                
                // Add new controls to layout
                layoutDashboard.Controls.Add(heroCard);
                layoutDashboard.Controls.Add(opportunitiesWidget);
                layoutDashboard.Controls.Add(quickActions);
                layoutDashboard.Controls.Add(recentTransactions);
                layoutDashboard.Controls.Add(assetChart);
                
                // Wire up quick actions
                quickActions.SendMoneyClicked += (s, e) => btnMoneyTransfer_ItemClick(s, null);
                quickActions.SupportClicked += (s, e) => {
                    // Open AI Assistant Form V2
                    var aiForm = new AIAssistantFormV2();
                    aiForm.Show();
                };
                
                // Create 2x3 Grid Layout
                // Row 1: Hero + Opportunities
                var groupRow1 = layoutGroupRoot.AddGroup();
                groupRow1.GroupBordersVisible = false;
                groupRow1.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
                groupRow1.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
                groupRow1.OptionsTableLayoutGroup.RowDefinitions.Clear();
                groupRow1.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
                groupRow1.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
                groupRow1.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Absolute, Height = 210 });
                
                AddControlToGroup(groupRow1, heroCard, 0, 0);
                AddControlToGroup(groupRow1, opportunitiesWidget, 0, 1);
                
                // Row 2: Quick Actions (Full Width)
                var itemQuickActions = layoutGroupRoot.AddItem();
                itemQuickActions.Control = quickActions;
                itemQuickActions.TextVisible = false;
                itemQuickActions.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                itemQuickActions.MinSize = new Size(0, 130);
                itemQuickActions.MaxSize = new Size(0, 130);
                
                // Row 3: Transactions + Asset Chart
                var groupRow3 = layoutGroupRoot.AddGroup();
                groupRow3.GroupBordersVisible = false;
                groupRow3.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
                groupRow3.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
                groupRow3.OptionsTableLayoutGroup.RowDefinitions.Clear();
                groupRow3.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
                groupRow3.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
                groupRow3.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Percent, Height = 100 });
                
                AddControlToGroup(groupRow3, recentTransactions, 0, 0);
                AddControlToGroup(groupRow3, assetChart, 0, 1);
                
                layoutDashboard.EndUpdate();
                
                // Update Hero Card with real data (BANK BALANCE)
                UpdateHeroCard();
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SetupFintechProDashboard Error: {ex.Message}");
            }
        }
        
        private void SetupPortfolioDashboard()
        {
            System.Diagnostics.Debug.WriteLine($"[SETUP] SetupPortfolioDashboard called, _portfolioDashboardInitialized={_portfolioDashboardInitialized}");
            
            try {
                // Guard: Only setup once
                if (_portfolioDashboardInitialized)
                {
                    System.Diagnostics.Debug.WriteLine("[SETUP] Portfolio dashboard already initialized, just updating");
                    UpdatePortfolioHeroCard();
                    UpdatePortfolioAssetChart();
                    return;
                }
                
                // Create portfolio dashboard panel if not exists
                if (pnlPortfolioDashboard == null)
                {
                    pnlPortfolioDashboard = new PanelControl();
                    pnlPortfolioDashboard.Dock = DockStyle.Fill;
                    pnlPortfolioDashboard.Appearance.BackColor = Color.FromArgb(18, 18, 18);
                    pnlPortfolioDashboard.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                    pnlPortfolioDashboard.Name = "pnlPortfolioDashboard";
                    
                    layoutPortfolioDashboard = new LayoutControl();
                    layoutPortfolioDashboard.Dock = DockStyle.Fill;
                    layoutPortfolioDashboard.BackColor = Color.FromArgb(18, 18, 18);
                    
                    layoutPortfolioGroupRoot = new LayoutControlGroup();
                    layoutPortfolioDashboard.Root = layoutPortfolioGroupRoot;
                    
                    pnlPortfolioDashboard.Controls.Add(layoutPortfolioDashboard);
                    this.Controls.Add(pnlPortfolioDashboard);
                }
                
                _portfolioDashboardInitialized = true;
                
                layoutPortfolioDashboard.BeginUpdate();
                
                // Remove old controls
                layoutPortfolioDashboard.Controls.Clear();
                layoutPortfolioGroupRoot.Items.Clear();
                
                // Create portfolio widgets (same as Tab1)
                portfolioHeroCard = new HeroNetWorthCard();
                portfolioOpportunitiesWidget = new InvestmentOpportunitiesWidget();
                portfolioQuickActions = new QuickActionsBar();
                portfolioRecentTransactions = new RecentTransactionsWidget();
                portfolioAssetChart = new AssetAllocationChart();
                
                // Add controls to portfolio layout
                layoutPortfolioDashboard.Controls.Add(portfolioHeroCard);
                layoutPortfolioDashboard.Controls.Add(portfolioOpportunitiesWidget);
                layoutPortfolioDashboard.Controls.Add(portfolioQuickActions);
                layoutPortfolioDashboard.Controls.Add(portfolioRecentTransactions);
                layoutPortfolioDashboard.Controls.Add(portfolioAssetChart);
                
                // Wire up portfolio quick actions
                portfolioQuickActions.SendMoneyClicked += (s, e) => btnMoneyTransfer_ItemClick(s, null);
                portfolioQuickActions.SupportClicked += (s, e) => {
                    System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] HANDLER: portfolioQuickActions.Support clicked, opening AIAssistantFormV4");
                    var aiForm = new AIAssistantFormV4();
                    aiForm.Show();
                };
                
                // Create 2x3 Grid Layout (same as Tab1)
                var groupRow1 = layoutPortfolioGroupRoot.AddGroup();
                groupRow1.GroupBordersVisible = false;
                groupRow1.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
                groupRow1.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
                groupRow1.OptionsTableLayoutGroup.RowDefinitions.Clear();
                groupRow1.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
                groupRow1.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 50 });
                groupRow1.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Percent, Height = 100 });
                
                AddControlToPortfolioGroup(groupRow1, portfolioHeroCard, 0, 0);
                AddControlToPortfolioGroup(groupRow1, portfolioOpportunitiesWidget, 0, 1);
                
                var groupRow2 = layoutPortfolioGroupRoot.AddGroup();
                groupRow2.GroupBordersVisible = false;
                groupRow2.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
                groupRow2.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
                groupRow2.OptionsTableLayoutGroup.RowDefinitions.Clear();
                groupRow2.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 33 });
                groupRow2.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 33 });
                groupRow2.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 34 });
                groupRow2.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Percent, Height = 100 });
                
                AddControlToPortfolioGroup(groupRow2, portfolioQuickActions, 0, 0);
                AddControlToPortfolioGroup(groupRow2, portfolioRecentTransactions, 0, 1);
                AddControlToPortfolioGroup(groupRow2, portfolioAssetChart, 0, 2);
                
                layoutPortfolioDashboard.EndUpdate();
                
                // Update portfolio hero card with real data
                UpdatePortfolioHeroCard();
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SetupPortfolioDashboard Error: {ex.Message}");
            }
        }
        
        private void AddControlToPortfolioGroup(LayoutControlGroup group, Control ctrl, int row, int col)
        {
            var item = group.AddItem();
            item.Control = ctrl;
            item.TextVisible = false;
            item.OptionsTableLayoutItem.RowIndex = row;
            item.OptionsTableLayoutItem.ColumnIndex = col;
        }
        
        private async void UpdatePortfolioHeroCard()
        {
            try
            {
                var userId = AppEvents.CurrentSession.UserId;
                
                // Get real data using DashboardSummaryService
                var totalDebt = await _dashboardSummaryService.GetTotalDebtAsync(userId);
                var netWorth = await _dashboardSummaryService.GetNetWorthAsync(userId);
                
                // Update portfolio hero card
                if (portfolioHeroCard != null)
                {
                    // Calculate trend (simplified)
                    var trend = netWorth >= 0;
                    var trendPercent = totalDebt > 0 ? (netWorth / totalDebt * 100) : 0;
                    portfolioHeroCard.SetNetWorth(netWorth, totalDebt, trendPercent, trend, trend ? "📈" : "📉");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePortfolioHeroCard Error: {ex.Message}");
            }
        }

        private async void UpdateHeroCard()
        {
            try {
                if (heroCard == null) return;
                
                // Use DashboardSummaryService for all metrics (premium data)
                var fullData = await _dashboardSummaryService.GetFullDashboardDataAsync(AppEvents.CurrentSession.UserId);
                
                // Update hero card with full data
                heroCard.SetFullData(
                    fullData.TotalBalance,
                    fullData.TotalDebt,
                    fullData.NetWorth,
                    fullData.MonthlyChange,
                    fullData.ActiveAccounts,
                    fullData.PrimaryIban
                );
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"UpdateHeroCard Error: {ex.Message}");
            }
        }
        
        private void UpdateAssetChart()
        {
            try {
                if (assetChart == null) return;
                System.Diagnostics.Debug.WriteLine($"[CHART] UpdateAssetChart called, instance={assetChart.GetHashCode()}");
                assetChart.RefreshData();
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"UpdateAssetChart Error: {ex.Message}");
            }
        }
        
        private void UpdatePortfolioAssetChart()
        {
            try {
                if (portfolioAssetChart == null) return;
                System.Diagnostics.Debug.WriteLine($"[CHART] UpdatePortfolioAssetChart called, instance={portfolioAssetChart.GetHashCode()}");
                portfolioAssetChart.RefreshData();
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"UpdatePortfolioAssetChart Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Refresh ALL dashboard widgets (both tabs)
        /// </summary>
        private void RefreshAllDashboards()
        {
            System.Diagnostics.Debug.WriteLine("[REFRESH] RefreshAllDashboards called");
            UpdateHeroCard();
            UpdateAssetChart();
            UpdatePortfolioHeroCard();
            UpdatePortfolioAssetChart();
        }

        private async void LoadDashboardData()
        {
            try
            {
                var context = new BankApp.Infrastructure.Data.DapperContext();
                using (var conn = context.CreateConnection())
                {
                    conn.Open();
                    
                    // Toplam Varlık
                    // Toplam Varlık (Bank Accounts + Investment Portfolio)
                    var totalBankAssets = await conn.ExecuteScalarAsync<decimal?>(
                        "SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\"") ?? 0;
                        
                    // Add Portfolio Value
                    var portfolioService = new PortfolioService();
                    var portfolioValue = await portfolioService.GetNetWorthAsync();
                    
                    var totalWealth = totalBankAssets + portfolioValue;
                    
                    lblTotalAssetsValue.Text = totalWealth.ToString("N2");
                    
                    // Update Title to reflect integration
                    if (lblTotalAssetsTitle != null) lblTotalAssetsTitle.Text = "💰 Toplam Varlık (Banka + Yatırım)";
                    
                    // Günlük İşlem Sayısı
                    var dailyTransactions = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM \"Transactions\" WHERE DATE(\"TransactionDate\") = CURRENT_DATE");
                    lblDailyTransactionsValue.Text = dailyTransactions.ToString();
                    
                    // Aktif Müşteri Sayısı
                    var activeCustomers = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM \"Customers\"");
                    lblActiveCustomersValue.Text = activeCustomers.ToString();
                    
                    // Döviz değişim simülasyonu
                    var random = new Random();
                    var change = (random.NextDouble() * 2 - 1).ToString("F3");
                    lblExchangeRateValue.Text = $"{change}%";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard Data Error: {ex.Message}");
            }
        }

        // Old campaign panel code removed - replaced by InvestmentOpportunitiesWidget

        private async void LoadCustomers()
        {
            try
            {
                if (gridCustomers == null) return;

                var context = new BankApp.Infrastructure.Data.DapperContext();
                using(var conn = context.CreateConnection())
                {
                    conn.Open();
                    var customers = await conn.QueryAsync<BankApp.Core.Entities.Customer>(
                        "SELECT * FROM \"Customers\" ORDER BY \"CreatedAt\" DESC");
                    gridCustomers.DataSource = customers?.ToList() ?? new List<BankApp.Core.Entities.Customer>();
                }
            }
            catch(Exception ex) 
            {
                XtraMessageBox.Show($"Müşteri verileri yüklenirken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private InvestmentDashboard investmentDashboard;
        private InvestmentViewContainer investmentView;

        private void InitializeInvestmentDashboard()
        {
            // Legacy InvestmentDashboard - kept for compatibility (used by buttons)
            investmentDashboard = new InvestmentDashboard();
            investmentDashboard.Dock = DockStyle.Fill;
            investmentDashboard.Visible = false; 
            this.Controls.Add(investmentDashboard);
            
            // NEW: TradingView-like Investment View for Tab3 (Yatırım)
            // Now uses container with MarketHome -> InstrumentDetail navigation
            investmentView = new InvestmentViewContainer();
            investmentView.Dock = DockStyle.Fill;
            investmentView.Visible = false;
            this.Controls.Add(investmentView);
            
            // Portfolio View for Tab2 (Portföy)
            portfolioView = new PortfolioView();
            portfolioView.Dock = DockStyle.Fill;
            portfolioView.Visible = false;
            this.Controls.Add(portfolioView);
        }

        private void RibbonControl1_SelectedPageChanged(object sender, EventArgs e)
        {
            if (ribbonControl1 == null || pageDashboard == null) return;

            // Determine which tab is selected
            bool isTab1Dashboard = (ribbonControl1.SelectedPage == pageDashboard);
            bool isTab2Portfolio = (ribbonControl1.SelectedPage == pagePortfolio);
            bool isTab3Investment = (ribbonControl1.SelectedPage == pageInvestments);
            bool isTab4Customers = (ribbonControl1.SelectedPage == pageCustomers);

            // Tab1: Dashboard & Tab2: Portfolio (AYNI DASHBOARD)
            if (pnlDashboard != null)
            {
                // Hem Genel Bakış hem Portföy sekmesinde aynı dashboard göster
                pnlDashboard.Visible = isTab1Dashboard || isTab2Portfolio;
                if (isTab1Dashboard || isTab2Portfolio) pnlDashboard.BringToFront();
            }
            
            // Hide old portfolio view - artık kullanılmıyor
            if (portfolioView != null)
            {
                portfolioView.Visible = false;
            }
            
            // Hide portfolio dashboard panel - artık kullanılmıyor
            if (pnlPortfolioDashboard != null)
            {
                pnlPortfolioDashboard.Visible = false;
            }
            
            // Hide legacy investmentDashboard - no longer used in tab switching
            if (investmentDashboard != null)
            {
                investmentDashboard.Visible = false;
            }
            
            // Tab3: Show TradingView-like investment screen
            if (investmentView != null)
            {
                investmentView.Visible = isTab3Investment;
                if (isTab3Investment) investmentView.BringToFront();
            }
            
            // Hide legacy "Yatırım Araçları" ribbon group when Investment tab is active
            // We now have buttons in MarketHome/Detail views instead
            if (groupTradingActions != null)
            {
                groupTradingActions.Visible = !isTab3Investment;
            }

            // Tab4: Customers grid (Admin only)
            if (gridCustomers != null)
            {
                gridCustomers.Visible = isTab4Customers;
                if (isTab4Customers)
                {
                    gridCustomers.Dock = DockStyle.Fill;
                    gridCustomers.BringToFront();
                    LoadCustomers();
                }
            }
        }

        // DYNAMIC DASHBOARD LOGIC
        private enum DashboardChartType
        {
            Expenses,           // Pie
            Transactions,       // Bar
            BalanceHistory,     // Line
            AssetDistribution,  // Pie3D
            IncomeExpense,      // Stacked Bar
            CreditUsage,        // Doughnut (Gauge style)
            StockPortfolio      // Bar
        }

        private void InitializeDynamicCharts()
        {
            // Default Assignments
            RenderChart(chartCurrency, DashboardChartType.Expenses);
            RenderChart(chartTransactions, DashboardChartType.Transactions);
            RenderChart(chartBalanceHistory, DashboardChartType.BalanceHistory);
            RenderChart(chartAssetDistribution, DashboardChartType.AssetDistribution);

            // Context Menu Event
            chartCurrency.MouseUp += Chart_MouseUp;
            chartTransactions.MouseUp += Chart_MouseUp;
            chartBalanceHistory.MouseUp += Chart_MouseUp;
            chartAssetDistribution.MouseUp += Chart_MouseUp;
            
            chartAssetDistribution.MouseUp += Chart_MouseUp;
            // Click popups disabled per user request
        }

        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is ChartControl chart)
            {
                ChartDetailForm detail = new ChartDetailForm(chart);
                detail.ShowDialog();
            }
        }

        private void Chart_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && sender is ChartControl chart)
            {
                DevExpress.XtraBars.PopupMenu menu = new DevExpress.XtraBars.PopupMenu();
                // Create Menu Items from Enum
                foreach (DashboardChartType type in Enum.GetValues(typeof(DashboardChartType)))
                {
                    var item = new DevExpress.XtraBars.BarButtonItem(this.ribbonControl1.Manager, type.ToString());
                    item.ItemClick += (s, args) => RenderChart(chart, type);
                    menu.ItemLinks.Add(item);
                }
                menu.ShowPopup(Control.MousePosition);
            }
        }

        private void RenderChart(ChartControl chart, DashboardChartType type)
        {
            chart.Series.Clear();
            chart.Titles.Clear();
            chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
            chart.Legend.AlignmentVertical = LegendAlignmentVertical.BottomOutside;

            switch (type)
            {
                case DashboardChartType.Expenses:
                    {
                        Series s = new Series("Expenses", ViewType.Doughnut);
                        s.Points.Add(new SeriesPoint("Market", 1200));
                        s.Points.Add(new SeriesPoint("Bills", 850));
                        s.Points.Add(new SeriesPoint("Clothing", 450));
                        s.Points.Add(new SeriesPoint("Fun", 600));
                        // ((DoughnutSeriesView)s.View).HoleRadiusPercent = 40;
                        s.Label.TextPattern = "{A}: {VP:P0}";
                        chart.Series.Add(s);
                        chart.Titles.Add(new ChartTitle { Text = "Harcama Dağılımı", TextColor = Color.White });
                    }
                    break;

                case DashboardChartType.Transactions:
                    {
                        Series s = new Series("Transactions", ViewType.Bar);
                        var today = DateTime.Now;
                        s.Points.Add(new SeriesPoint(today.AddDays(-4).ToString("dd.MM"), 120));
                        s.Points.Add(new SeriesPoint(today.AddDays(-3).ToString("dd.MM"), 180));
                        s.Points.Add(new SeriesPoint(today.AddDays(-2).ToString("dd.MM"), 95));
                        s.Points.Add(new SeriesPoint(today.AddDays(-1).ToString("dd.MM"), 210));
                        s.Points.Add(new SeriesPoint(today.ToString("dd.MM"), 175));
                        chart.Series.Add(s);
                        chart.Titles.Add(new ChartTitle { Text = "Günlük İşlem Adedi", TextColor = Color.White });
                    }
                    break;
                
                case DashboardChartType.BalanceHistory:
                    {
                        Series s = new Series("Balance", ViewType.Spline); // Smoother line
                        s.Points.Add(new SeriesPoint("Jan", 85000));
                        s.Points.Add(new SeriesPoint("Feb", 92000));
                        s.Points.Add(new SeriesPoint("Mar", 88000));
                        s.Points.Add(new SeriesPoint("Apr", 95000));
                        s.Points.Add(new SeriesPoint("May", 110000));
                        s.Points.Add(new SeriesPoint("Jun", 121325));
                        // ((SplineSeriesView)s.View).Color = Color.FromArgb(76, 175, 80);
                        // ((SplineSeriesView)s.View).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
                        chart.Series.Add(s);
                        chart.Titles.Add(new ChartTitle { Text = "Varlık Gelişimi", TextColor = Color.White });
                    }
                    break;

                case DashboardChartType.AssetDistribution:
                    {
                        Series s = new Series("Assets", ViewType.Pie3D);
                        s.Points.Add(new SeriesPoint("TRY", 65000));
                        s.Points.Add(new SeriesPoint("Gold", 35000));
                        s.Points.Add(new SeriesPoint("Stocks", 15000));
                        s.Points.Add(new SeriesPoint("BES", 6325));
                        chart.Series.Add(s);
                        chart.Titles.Add(new ChartTitle { Text = "Portföy Dağılımı", TextColor = Color.White });
                    }
                    break;

                case DashboardChartType.IncomeExpense:
                    {
                        Series s1 = new Series("Income", ViewType.Bar);
                        Series s2 = new Series("Expense", ViewType.Bar);
                        
                        s1.Points.Add(new SeriesPoint("May", 45000));
                        s1.Points.Add(new SeriesPoint("Jun", 48000));
                        
                        s2.Points.Add(new SeriesPoint("May", 32000));
                        s2.Points.Add(new SeriesPoint("Jun", 28000));

                        chart.Series.Add(s1);
                        chart.Series.Add(s2);
                        chart.Titles.Add(new ChartTitle { Text = "Gelir / Gider", TextColor = Color.White });
                    }
                    break;

                case DashboardChartType.CreditUsage:
                    {
                         Series s = new Series("Limit", ViewType.Pie);
                         s.Points.Add(new SeriesPoint("Used", 12500));
                         s.Points.Add(new SeriesPoint("Available", 37500));
                         chart.Series.Add(s);
                         chart.Titles.Add(new ChartTitle { Text = "Kredi Kartı Limiti", TextColor = Color.White });
                    }
                    break;

                case DashboardChartType.StockPortfolio:
                    {
                        Series s = new Series("Stocks", ViewType.Bar);
                        s.Points.Add(new SeriesPoint("THYAO", 15000));
                        s.Points.Add(new SeriesPoint("ASELS", 8000));
                        s.Points.Add(new SeriesPoint("GARAN", 12000));
                        chart.Series.Add(s);
                        chart.Titles.Add(new ChartTitle { Text = "Hisse Portföyü", TextColor = Color.White });
                    }
                    break;
            }
        }
        
        // LoadDashboardCharts calls InitializeDynamicCharts instead of hardcoded logic
        private async void LoadDashboardCharts()
        {
             // Initialize controls
             InitializeDynamicCharts();
             
             // Populate with real data
             await LoadRealDashboardChartsDataAsync();
        }

        // Duplicate Chart_MouseClick removed


        private async Task LoadRealDashboardChartsDataAsync()
        {
            try {
                var context = new BankApp.Infrastructure.Data.DapperContext();
                using (var conn = context.CreateConnection())
                {
                    conn.Open();

                    // 1. Harcama Dağılımı (Doughnut) - Real Category Data
                    Series seriesPie = new Series("Harcamalar", ViewType.Doughnut);
                    var categoryData = await conn.QueryAsync<(string Category, decimal Total)>(
                        "SELECT \"Description\" as Category, SUM(\"Amount\") as Total FROM \"Transactions\" WHERE \"TransactionType\" IN ('Withdraw', 'TransferOut') GROUP BY \"Description\"");
                    
                    if (!categoryData.Any()) {
                        seriesPie.Points.Add(new SeriesPoint("Diğer", 100));
                    } else {
                        foreach(var cat in categoryData.Take(5)) {
                            // Extract first word as category if desc is long
                            string catName = cat.Category.Split(' ')[0];
                            seriesPie.Points.Add(new SeriesPoint(catName, cat.Total));
                        }
                    }

                    DoughnutSeriesView doughnutView = (DoughnutSeriesView)seriesPie.View;
                    doughnutView.HoleRadiusPercent = 55;
                    seriesPie.Label.TextPattern = "{A}: {VP:P1}";
                    
                    chartCurrency.Series.Clear();
                    chartCurrency.Series.Add(seriesPie);
                    chartCurrency.Titles.Clear();
                    chartCurrency.Titles.Add(new ChartTitle() { Text = "💸 Harcama Dağılımı", TextColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold) });
                    chartCurrency.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
                    chartCurrency.PaletteName = "Pastel Kit";
                    
                    // 2. İşlem Hacmi (Bar) - Real Volume for 5 days
                    Series seriesBar = new Series("İşlem Hacmi", ViewType.Bar);
                    var volumeData = await conn.QueryAsync<(string Day, decimal Total)>(
                        "SELECT TO_CHAR(\"TransactionDate\", 'DD.MM') as Day, SUM(\"Amount\") as Total FROM \"Transactions\" GROUP BY Day ORDER BY Day DESC LIMIT 5");
                    
                    foreach(var vol in volumeData.Reverse()) {
                        seriesBar.Points.Add(new SeriesPoint(vol.Day, vol.Total));
                    }

                    chartTransactions.Series.Clear();
                    chartTransactions.Series.Add(seriesBar);
                    chartTransactions.Titles.Clear();
                    chartTransactions.Titles.Add(new ChartTitle() { Text = "📈 İşlem Hacmi (Son 5 Gün)", TextColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold) });
                    chartTransactions.PaletteName = "Mixed";

                    // 3. Asset Distribution - Real Portfolio Data
                    var portfolioService = new PortfolioService();
                    var allocation = await portfolioService.GetAssetAllocationAsync();
                    Series seriesAssets = new Series("Varlıklar", ViewType.Pie3D);
                    foreach(var alloc in allocation) {
                        seriesAssets.Points.Add(new SeriesPoint(alloc.Key.ToString(), alloc.Value));
                    }
                    chartAssetDistribution.Series.Clear();
                    chartAssetDistribution.Series.Add(seriesAssets);
                    chartAssetDistribution.Titles.Clear();
                    chartAssetDistribution.Titles.Add(new ChartTitle { Text = "🌟 Varlık Dağılımı", TextColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold) });
                    chartAssetDistribution.PaletteName = "Chameleon";

                    // Enable Animations for everything
                    foreach(var c in new[] { chartCurrency, chartTransactions, chartAssetDistribution, chartBalanceHistory, chartFinancialHealth, chartSavingsGoals, chartCreditScore, chartBudgetPerformance })
                    {
                        if (c == null) continue;
                        // Animation logic removed due to version incompatibility
                        foreach(Series s in c.Series) {
                            s.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;
                        }
                    }
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"LoadRealDashboardChartsData Error: {ex.Message}");
            }

            // Call aesthetic renderers (which currently use mock logic but could be enriched)
            RenderFinancialHealthChart();
            RenderSpendingFunnelChart();
            RenderCreditScoreChart();
            RenderBudgetPerformanceChart();
        }

        private void RenderFinancialHealthChart()
        {
            chartFinancialHealth.Series.Clear();
            chartFinancialHealth.Titles.Clear();
            chartFinancialHealth.BackColor = Color.FromArgb(30, 30, 30);
            
            Series series = new Series("Finansal Sağlık", ViewType.RadarArea);
            series.Points.Add(new SeriesPoint("Gelir İstikrarı", 85));
            series.Points.Add(new SeriesPoint("Borç/Gelir", 90));
            series.Points.Add(new SeriesPoint("Tasarruf", 65));
            series.Points.Add(new SeriesPoint("Yatırım Çeşitliliği", 70));
            series.Points.Add(new SeriesPoint("Acil Durum Fonu", 50));

            chartFinancialHealth.Series.Add(series);
            
            if (series.View is RadarAreaSeriesView view)
            {
                view.Color = Color.FromArgb(100, 76, 175, 80); // Semi-transparent Green
                view.Border.Color = Color.FromArgb(76, 175, 80);
                view.Border.Visibility = DevExpress.Utils.DefaultBoolean.True;
                view.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            }

            if (chartFinancialHealth.Diagram is RadarDiagram diagram)
            {
                diagram.AxisX.Label.TextColor = Color.White;
                diagram.AxisY.Label.TextColor = Color.White;
                diagram.AxisY.GridLines.Color = Color.FromArgb(60, 60, 60);
                diagram.AxisX.GridLines.Color = Color.FromArgb(60, 60, 60);
            }

            chartFinancialHealth.Titles.Add(new ChartTitle { Text = "Finansal Sağlık Skoru", TextColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
            chartFinancialHealth.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
        }

        private void RenderSpendingFunnelChart()
        {
            chartSavingsGoals.Series.Clear();
            chartSavingsGoals.Titles.Clear();
            chartSavingsGoals.BackColor = Color.FromArgb(30, 30, 30);

            Series series = new Series("Gider Dağılımı", ViewType.Funnel);
            series.Points.Add(new SeriesPoint("Kira & Konut", 15000));
            series.Points.Add(new SeriesPoint("Mutfak & Gıda", 8000));
            series.Points.Add(new SeriesPoint("Ulaşım", 4000));
            series.Points.Add(new SeriesPoint("Faturalar", 2500));
            series.Points.Add(new SeriesPoint("Eğlence", 1500));

            chartSavingsGoals.Series.Add(series);

            if (series.View is FunnelSeriesView view)
            {
                view.ColorEach = true;
                view.Titles.Add(new SeriesTitle { Text = "Aylık Gider Profili" });
            }

            // chartSavingsGoals.Titles.Add(new ChartTitle { Text = "Gider Analizi", TextColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
            chartSavingsGoals.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chartSavingsGoals.Legend.BackColor = Color.Transparent;
            chartSavingsGoals.Legend.TextColor = Color.White;
        }

        private void RenderCreditScoreChart()
        {
            chartCreditScore.Series.Clear();
            chartCreditScore.Titles.Clear();
            chartCreditScore.BackColor = Color.FromArgb(30, 30, 30);
            
            // Linear Gauge for Credit Score
            // Note: ChartControl doesn't support Gauges directly (GaugeControl does).
            // We'll simulate a 'progress' bar using a Stacked Bar Chart for now or simple Bar
            
            Series series = new Series("Kredi Puanı", ViewType.Bar);
            series.Points.Add(new SeriesPoint("Puan", 1650)); // Max 1900
            
            chartCreditScore.Series.Add(series);
            
            if (series.View is BarSeriesView view)
            {
                view.Color = Color.FromArgb(255, 193, 7); // Amber/Gold
                view.Border.Visibility = DevExpress.Utils.DefaultBoolean.False;
            }
            
            if (chartCreditScore.Diagram is XYDiagram diagram)
            {
                diagram.Rotated = true; // Horizontal Bar
                diagram.AxisY.WholeRange.MaxValue = 1900;
                diagram.AxisY.WholeRange.MinValue = 0;
                diagram.AxisY.GridLines.Color = Color.FromArgb(60, 60, 60);
                diagram.AxisX.Visibility = DevExpress.Utils.DefaultBoolean.False;
                diagram.AxisY.Label.TextColor = Color.White;
                
                // Add strip for 'Risk' zones
                Strip stripRisk = new Strip("Risk", 0, 900);
                stripRisk.Color = Color.FromArgb(100, 255, 0, 0);
                diagram.AxisY.Strips.Add(stripRisk);
                
                Strip stripGood = new Strip("Good", 1500, 1900);
                stripGood.Color = Color.FromArgb(100, 0, 255, 0);
                diagram.AxisY.Strips.Add(stripGood);
            }

            chartCreditScore.Titles.Add(new ChartTitle { Text = "Findeks Kredi Notu (1650 - Çok İyi)", TextColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
            chartCreditScore.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
        }

        private void RenderBudgetPerformanceChart()
        {
            chartBudgetPerformance.Series.Clear();
            chartBudgetPerformance.Titles.Clear();
            chartBudgetPerformance.BackColor = Color.FromArgb(30, 30, 30);
            
            // Planned vs Actual
            Series seriesPlanned = new Series("Planlanan", ViewType.Bar);
            Series seriesActual = new Series("Gerçekleşen", ViewType.Bar);
            
            seriesPlanned.Points.Add(new SeriesPoint("Gıda", 5000));
            seriesPlanned.Points.Add(new SeriesPoint("Ulaşım", 3000));
            seriesPlanned.Points.Add(new SeriesPoint("Eğlence", 2000));
            
            seriesActual.Points.Add(new SeriesPoint("Gıda", 5500)); // Over budget
            seriesActual.Points.Add(new SeriesPoint("Ulaşım", 2800)); // Under budget
            seriesActual.Points.Add(new SeriesPoint("Eğlence", 1800)); // Under budget
            
            chartBudgetPerformance.Series.AddRange(new Series[] { seriesPlanned, seriesActual });
            
            chartBudgetPerformance.Titles.Add(new ChartTitle { Text = "Bütçe Performansı", TextColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
            chartBudgetPerformance.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chartBudgetPerformance.Legend.TextColor = Color.White;
            
            if (chartBudgetPerformance.Diagram is XYDiagram diag)
            {
                diag.AxisX.Label.TextColor = Color.White;
                diag.AxisY.Label.TextColor = Color.White;
                diag.AxisY.GridLines.Color = Color.FromArgb(60, 60, 60);
            }
        }


        private void btnAiAssist_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] HANDLER: btnAiAssist clicked, opening AIAssistantFormV4");
            var frm = new AIAssistantFormV4();
            frm.Show();
        }

        private void btnMoneyTransfer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TransferForm frm = new TransferForm();
            frm.ShowDialog();
            LoadDashboardData();
            LoadDashboardCharts();
        }

        private void btnAddCustomer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CustomerForm frm = new CustomerForm();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadCustomers();
                LoadDashboardData();
            }
        }

        private void btnEditCustomer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridViewCustomers == null)
            {
                XtraMessageBox.Show("Müşteri listesi yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var row = gridViewCustomers.GetFocusedRow();
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen düzenlemek için bir müşteri seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if(row is BankApp.Core.Entities.Customer customer)
            {
                CustomerForm frm = new CustomerForm(customer);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            }
        }

        private async void btnDeleteCustomer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridViewCustomers == null) return;

            var row = gridViewCustomers.GetFocusedRow();
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen silmek için bir müşteri seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if(row is BankApp.Core.Entities.Customer customer)
            {
                if(XtraMessageBox.Show("Müşteriyi silmek istediğinize emin misiniz?", "Onay", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        var context = new BankApp.Infrastructure.Data.DapperContext();
                        var repo = new BankApp.Infrastructure.Data.CustomerRepository(context);
                        await repo.DeleteAsync(customer.Id);
                        
                        var audit = new BankApp.Infrastructure.Data.AuditRepository(context);
                        await audit.AddLogAsync(new BankApp.Core.Entities.AuditLog 
                        { 
                            UserId = 1, 
                            Action = "DeleteCustomer", 
                            Details = $"Deleted Customer: {customer.FirstName} {customer.LastName}",
                            IpAddress = "127.0.0.1"
                        });

                        LoadCustomers();
                        LoadDashboardData();
                    }
                    catch(Exception ex)
                    {
                        XtraMessageBox.Show("Silme hatası: " + ex.Message);
                    }
                }
            }
        }

        private void btnCustomerAccounts_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridViewCustomers == null) return;

            var row = gridViewCustomers.GetFocusedRow();
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen bir müşteri seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if(row is BankApp.Core.Entities.Customer customer)
            {
                CustomerAccountsForm frm = new CustomerAccountsForm(customer.Id);
                frm.ShowDialog();
            }
        }

        private void btnExportExcel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridCustomers == null) return;

            try
            {
                string path = "Musteriler.xlsx";
                gridCustomers.ExportToXlsx(path);
                XtraMessageBox.Show($"Müşteri listesi Excel'e aktarıldı: {path}", "Başarılı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show($"Export Hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportPdf_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridCustomers == null) return;

            try
            {
                string path = "Musteriler.pdf";
                gridCustomers.ExportToPdf(path);
                XtraMessageBox.Show($"Müşteri listesi PDF'e aktarıldı: {path}", "Başarılı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show($"Export Hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAuditLogs_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            AuditLogsForm frm = new AuditLogsForm();
            frm.ShowDialog();
        }

        // YENİ: Trade Terminal - Aktif İşlem Ekranı
        private void btnStockMarket_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TradeTerminalForm frm = new TradeTerminalForm();
            frm.ShowDialog();
            RefreshDashboard(); // İşlem sonrası dashboard'u güncelle
        }

        // YENİ: Investment Dashboard - Portföy Yönetimi
        private void btnInvestmentDashboard_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            InvestmentDashboardForm frm = new InvestmentDashboardForm();
            frm.ShowDialog();
        }

        private void btnBES_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            BESForm frm = new BESForm();
            frm.ShowDialog();
        }

        // YENİ: Kartlarım butonu
        private void btnCards_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CardsForm frm = new CardsForm();
            frm.ShowDialog();
        }

        // YENİ: Vadeli Hesap
        private void btnTimeDeposit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TimeDepositForm frm = new TimeDepositForm();
            frm.ShowDialog();
            RefreshDashboard();
        }



        // YENİ: Kredi Başvurusu (User)
        private void btnLoanApplication_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoanApplicationForm frm = new LoanApplicationForm();
            frm.ShowDialog();
        }

        // YENİ: Kredi Onay (Admin)
        private void btnLoanApproval_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!AppEvents.CurrentSession.IsAdmin)
            {
                XtraMessageBox.Show("Bu özellik sadece Yöneticiler için!", "Yetki Hatası", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            LoanApprovalForm frm = new LoanApprovalForm();
            frm.ShowDialog();
            RefreshDashboard();
        }

        // YENİ: Çıkış Yap
        private void btnLogout_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var confirm = XtraMessageBox.Show("Çıkış yapmak istediğinize emin misiniz?", 
                "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                AppEvents.CurrentSession.Clear();
                this.Hide();
                
                // Login formunu yeniden aç
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Yeniden yükle
                    LoadDashboardData();
                    LoadDashboardCharts();
                    LoadCustomers();
                    UpdateMenuForRole();
                    this.Show();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        // Dashboard'u yenile
        private void RefreshDashboard()
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] RefreshDashboard called, this={GetType().FullName}");
            LoadDashboardData();
            LoadDashboardCharts();
        }

        // Rol bazlı menü güncelleme
        private void UpdateMenuForRole()
        {
            bool isAdmin = AppEvents.CurrentSession.IsAdmin;
            
            // Müşteriler sekmesi (Tab4) - SADECE Admin görebilir
            if (pageCustomers != null)
            {
                pageCustomers.Visible = isAdmin;
            }
            
            // Kredi Onay butonu - SADECE Admin görebilir
            if (btnLoanApproval != null)
            {
                btnLoanApproval.Visibility = isAdmin 
                    ? DevExpress.XtraBars.BarItemVisibility.Always 
                    : DevExpress.XtraBars.BarItemVisibility.Never;
            }
            
            // Kredi Başvuru butonu - SADECE Customer görebilir
            if (btnLoanApplication != null)
            {
                btnLoanApplication.Visibility = !isAdmin 
                    ? DevExpress.XtraBars.BarItemVisibility.Always 
                    : DevExpress.XtraBars.BarItemVisibility.Never;
            }
            
            // All users can see Tab1 (Genel Bakış), Tab2 (Portföy), Tab3 (Yatırım)
            // These tabs are always visible for customers
        }

        // Event sistemi için subscribe (form load'da çağrılmalı)
        private void SubscribeToEvents()
        {
            AppEvents.DataChanged += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] EVENT RECEIVED: AppEvents.DataChanged, Source={args.Source}, Action={args.Action}");
                // UI thread'e geç
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => RefreshDashboard()));
                    return;
                }
                RefreshDashboard();
            };
        }

        // COMPACT LAYOUT REDESIGN - HERO STYLE
        private void RedesignDashboardLayout()
        {
            try {
                if(layoutDashboard == null) return;
                
                layoutDashboard.BeginUpdate();
                
                // 1. Reset Root Groups
                layoutGroupRoot.Items.Clear();
                
                // 2. Add Top Cards Row (4 Columns)
                var groupCards = layoutGroupRoot.AddGroup();
                groupCards.GroupBordersVisible = false;
                groupCards.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
                groupCards.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
                groupCards.OptionsTableLayoutGroup.RowDefinitions.Clear();
                
                // 4 Equal Columns
                for(int i=0; i<4; i++) 
                    groupCards.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 25 });
                
                groupCards.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Absolute, Height = 90 });
                
                // Add Cards
                AddControlToGroup(groupCards, pnlTotalAssets, 0, 0);
                AddControlToGroup(groupCards, pnlDailyTransactions, 0, 1);
                AddControlToGroup(groupCards, pnlActiveCustomers, 0, 2);
                AddControlToGroup(groupCards, pnlExchangeRate, 0, 3);
                
                
                // Old campaign panel code removed - now using InvestmentOpportunitiesWidget


                // 4. Add 3 Key Charts in a Row (Large & Readable)
                var groupCharts = layoutGroupRoot.AddGroup();
                groupCharts.GroupBordersVisible = false;
                groupCharts.LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table;
                
                // 3 Columns
                for(int i=0; i<3; i++) 
                    groupCharts.OptionsTableLayoutGroup.ColumnDefinitions.Add(new ColumnDefinition { SizeType = SizeType.Percent, Width = 33 });
                
                // 1 Row (Fill remaining space)
                groupCharts.OptionsTableLayoutGroup.RowDefinitions.Add(new RowDefinition { SizeType = SizeType.Percent, Height = 100 });

                // Row 1: The 3 Most Important Charts
                AddControlToGroup(groupCharts, chartCurrency, 0, 0); // Harcama Dağılımı
                AddControlToGroup(groupCharts, chartTransactions, 0, 1); // İşlem Hacmi
                AddControlToGroup(groupCharts, chartAssetDistribution, 0, 2); // Varlık Dağılımı
                
                layoutDashboard.EndUpdate();
            }
            catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Layout Redesign Error: {ex.Message}");
            }
        }

        private void AddControlToGroup(LayoutControlGroup group, Control ctrl, int row, int col)
        {
             var item = group.AddItem();
             item.Control = ctrl;
             item.TextVisible = false;
             item.OptionsTableLayoutItem.RowIndex = row;
             item.OptionsTableLayoutItem.ColumnIndex = col;
        }
        
        /// <summary>
        /// Dashboard refresh event handler - tüm widget'ları günceller
        /// </summary>
        private void OnDashboardRefreshRequested(object sender, DashboardRefreshEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnDashboardRefreshRequested(sender, e)));
                return;
            }
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard Refresh: {e.Reason} at {e.Timestamp}");
                
                // Tab1 Dashboard Widgets
                UpdateHeroCard();
                heroCard?.Invalidate();
                assetChart?.RefreshData();
                recentTransactions?.RefreshData();
                
                // Tab2 Portfolio Dashboard Widgets
                UpdatePortfolioHeroCard();
                portfolioHeroCard?.Invalidate();
                portfolioAssetChart?.RefreshData();
                portfolioRecentTransactions?.RefreshData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnDashboardRefreshRequested Error: {ex.Message}");
            }
        }
    }
}

