using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.LookAndFeel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using BankApp.Core.Interfaces;
using BankApp.Infrastructure.Services;
using BankApp.UI.Controls;
using Dapper;

namespace BankApp.UI.Forms
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly IAIService _aiService;
        private InvestmentDashboard investmentDashboard;

        public MainForm()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeComponent Error: {ex.Message}");
                // Continue execution to attempt showing the form
            }
            // Use real AI service with new key
            _aiService = new OpenRouterAIService("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");
            
            InitializeInvestmentDashboard();

            this.ribbonControl1.SelectedPageChanged += RibbonControl1_SelectedPageChanged;
            this.Load += MainForm_Load;
            
            // Event sistemine abone ol
            SubscribeToEvents();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadDashboardData();
            LoadDashboardCharts();
            LoadCustomers();
            UpdateMenuForRole(); // Rol bazlı menü güncelle
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
                    var totalAssets = await conn.ExecuteScalarAsync<decimal?>(
                        "SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\"") ?? 0;
                    lblTotalAssetsValue.Text = totalAssets.ToString("N2");
                    
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

        private void InitializeInvestmentDashboard()
        {
            investmentDashboard = new InvestmentDashboard();
            investmentDashboard.Visible = false;
            investmentDashboard.Dock = DockStyle.Fill;
            this.Controls.Add(investmentDashboard);
        }

        private void RibbonControl1_SelectedPageChanged(object sender, EventArgs e)
        {
            if (ribbonControl1 == null || pageDashboard == null) return;

            bool isDashboard = (ribbonControl1.SelectedPage == pageDashboard);
            bool isInvestments = (ribbonControl1.SelectedPage == pageInvestments);
            bool isCustomers = (ribbonControl1.SelectedPage == pageCustomers);

            if (pnlDashboard != null) pnlDashboard.Visible = isDashboard;
            
            if (investmentDashboard != null)
            {
                investmentDashboard.Visible = isInvestments;
                if (isInvestments)
                {
                    investmentDashboard.BringToFront();
                    investmentDashboard.LoadDummyData();
                }
            }

            if (gridCustomers != null)
            {
                gridCustomers.Visible = isCustomers;
                if(isCustomers)
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
            
            // Click Event (Popup)
            chartCurrency.MouseClick += Chart_MouseClick;
            chartTransactions.MouseClick += Chart_MouseClick;
            chartBalanceHistory.MouseClick += Chart_MouseClick;
            chartAssetDistribution.MouseClick += Chart_MouseClick;
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
                        ((DoughnutSeriesView)s.View).HoleRadiusPercent = 40;
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
                        ((SplineSeriesView)s.View).Color = Color.FromArgb(76, 175, 80);
                        ((SplineSeriesView)s.View).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
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
        private void LoadDashboardCharts()
        {
             // Initial Load
             InitializeDynamicCharts();
        }

        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            // Only Left Click triggers Popup
            if (e.Button == MouseButtons.Left && sender is ChartControl chart)
            {
                ChartPopupForm popup = new ChartPopupForm(chart);
                popup.ShowDialog();
            }
        }


        private void LoadSampleChartData()
        {
            // Harcamalar - Doughnut Chart
            Series seriesPie = new Series("Harcamalar", ViewType.Doughnut);
            seriesPie.Points.Add(new SeriesPoint("Market", 1200));
            seriesPie.Points.Add(new SeriesPoint("Faturalar", 850));
            seriesPie.Points.Add(new SeriesPoint("Giyim", 450));
            seriesPie.Points.Add(new SeriesPoint("Eğlence", 600));
            
            var doughnutView = (DoughnutSeriesView)seriesPie.View;
            doughnutView.HoleRadiusPercent = 45;
            seriesPie.Label.TextPattern = "{A}: {VP:P1}";
            
            // Label pozisyonu - TwoColumns
            if (seriesPie.Label is DoughnutSeriesLabel doughnutLabel)
            {
                doughnutLabel.Position = PieSeriesLabelPosition.TwoColumns;
            }
            
            chartCurrency.Series.Clear();
            chartCurrency.Series.Add(seriesPie);
            chartCurrency.PaletteName = "Nature Colors";
            chartCurrency.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chartCurrency.Legend.TextColor = Color.White;
            chartCurrency.Titles.Clear();
            chartCurrency.Titles.Add(new ChartTitle() { Text = "Harcama Dağılımı", TextColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });

            // Bar Chart
            Series seriesBar = new Series("İşlem Hacmi", ViewType.Bar);
            var today = DateTime.Now;
            seriesBar.Points.Add(new SeriesPoint(today.AddDays(-4).ToString("dd.MM"), 120000));
            seriesBar.Points.Add(new SeriesPoint(today.AddDays(-3).ToString("dd.MM"), 180000));
            seriesBar.Points.Add(new SeriesPoint(today.AddDays(-2).ToString("dd.MM"), 95000));
            seriesBar.Points.Add(new SeriesPoint(today.AddDays(-1).ToString("dd.MM"), 210000));
            seriesBar.Points.Add(new SeriesPoint(today.ToString("dd.MM"), 175000));
            
            var barView = (BarSeriesView)seriesBar.View;
            barView.Color = Color.FromArgb(33, 150, 243);
            
            chartTransactions.Series.Clear();
            chartTransactions.Series.Add(seriesBar);
            chartTransactions.Titles.Clear();
            chartTransactions.Titles.Add(new ChartTitle() { Text = "İşlem Hacmi (Son 5 Gün)", TextColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
        }

        private void btnAiAssist_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            AIAssistantForm frm = new AIAssistantForm();
            frm.ShowDialog();
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

        // YENİ: Yatırım İşlemleri
        private void btnStockMarket_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            StockMarketForm frm = new StockMarketForm();
            frm.ShowDialog();
            RefreshDashboard(); // İşlem sonrası dashboard'u güncelle
        }

        private void btnBES_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try 
            {
                var frm = new BESForm(); 
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("BES Ekranı açılırken hata: " + ex.Message);
            }
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
            LoadDashboardData();
            LoadDashboardCharts();
        }

        // Rol bazlı menü güncelleme
        private void UpdateMenuForRole()
        {
            bool isAdmin = AppEvents.CurrentSession.IsAdmin;
            
            // Müşteriler sekmesi - SADECE Admin görebilir
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
        }

        // Event sistemi için subscribe (form load'da çağrılmalı)
        private void SubscribeToEvents()
        {
            AppEvents.DataChanged += (sender, args) =>
            {
                // UI thread'e geç
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => RefreshDashboard()));
                    return;
                }
                RefreshDashboard();
            };
        }
    }
}

