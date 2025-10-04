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
using Dapper;

namespace BankApp.UI.Forms
{
    public partial class MainForm : RibbonForm
    {
        private readonly IAIService _aiService;

        public MainForm()
        {
            InitializeComponent();
            _aiService = new MockAIService();
            
            this.ribbonControl1.SelectedPageChanged += RibbonControl1_SelectedPageChanged;
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadDashboardData();
            LoadDashboardCharts();
            LoadCustomers();
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

        private void RibbonControl1_SelectedPageChanged(object sender, EventArgs e)
        {
            if (ribbonControl1 == null || pageDashboard == null) return;

            bool isDashboard = (ribbonControl1.SelectedPage == pageDashboard);
            
            if (pnlDashboard != null) pnlDashboard.Visible = isDashboard;
            if (gridCustomers != null)
            {
                gridCustomers.Visible = !isDashboard;
                if(!isDashboard)
                {
                    gridCustomers.Dock = DockStyle.Fill;
                    gridCustomers.BringToFront();
                    LoadCustomers();
                }
            }
        }

        private async void LoadDashboardCharts()
        {
            try
            {
                var context = new BankApp.Infrastructure.Data.DapperContext();
                using (var conn = context.CreateConnection())
                {
                    conn.Open();
                    
                    // 1. Mevduat Dağılımı - Doughnut (Pie) Chart
                    var currencyData = await conn.QueryAsync<dynamic>(
                        @"SELECT ""CurrencyCode"", SUM(""Balance"") as Total 
                          FROM ""Accounts"" 
                          GROUP BY ""CurrencyCode""");
                    
                    Series seriesPie = new Series("Mevduat Dağılımı", ViewType.Doughnut);
                    foreach (var item in currencyData)
                    {
                        string currency = item.CurrencyCode?.ToString() ?? "TRY";
                        decimal total = item.Total ?? 0m;
                        seriesPie.Points.Add(new SeriesPoint(currency, (double)total));
                    }
                    
                    // Eğer veri yoksa örnek veri ekle
                    if (seriesPie.Points.Count == 0)
                    {
                        seriesPie.Points.Add(new SeriesPoint("TRY", 750000));
                        seriesPie.Points.Add(new SeriesPoint("USD", 125000));
                        seriesPie.Points.Add(new SeriesPoint("EUR", 85000));
                    }
                    
                    // Doughnut görünüm ayarları
                    var doughnutView = (DoughnutSeriesView)seriesPie.View;
                    doughnutView.HoleRadiusPercent = 40;
                    seriesPie.Label.TextPattern = "{A}: {VP:P1}";
                    
                    chartCurrency.Series.Clear();
                    chartCurrency.Series.Add(seriesPie);
                    chartCurrency.Titles.Clear();
                    chartCurrency.Titles.Add(new ChartTitle() { Text = "Mevduat Dağılımı", TextColor = Color.White });

                    // 2. İşlem Hacmi - Bar Chart (Son 5 Gün)
                    var transactionData = await conn.QueryAsync<dynamic>(
                        @"SELECT DATE(""TransactionDate"") as TransDate, 
                                 SUM(""Amount"") as Total
                          FROM ""Transactions"" 
                          WHERE ""TransactionDate"" >= CURRENT_DATE - INTERVAL '5 days'
                          GROUP BY DATE(""TransactionDate"")
                          ORDER BY TransDate");
                    
                    Series seriesBar = new Series("İşlem Hacmi (Son 5 Gün)", ViewType.Bar);
                    
                    foreach (var item in transactionData)
                    {
                        DateTime date = item.TransDate ?? DateTime.Now;
                        decimal total = item.Total ?? 0m;
                        seriesBar.Points.Add(new SeriesPoint(date.ToString("dd.MM"), (double)total));
                    }
                    
                    // Eğer veri yoksa örnek veri ekle
                    if (seriesBar.Points.Count == 0)
                    {
                        var today = DateTime.Now;
                        seriesBar.Points.Add(new SeriesPoint(today.AddDays(-4).ToString("dd.MM"), 120000));
                        seriesBar.Points.Add(new SeriesPoint(today.AddDays(-3).ToString("dd.MM"), 180000));
                        seriesBar.Points.Add(new SeriesPoint(today.AddDays(-2).ToString("dd.MM"), 95000));
                        seriesBar.Points.Add(new SeriesPoint(today.AddDays(-1).ToString("dd.MM"), 210000));
                        seriesBar.Points.Add(new SeriesPoint(today.ToString("dd.MM"), 175000));
                    }
                    
                    // Bar görünüm ayarları
                    var barView = (BarSeriesView)seriesBar.View;
                    barView.Color = Color.FromArgb(33, 150, 243);
                    
                    chartTransactions.Series.Clear();
                    chartTransactions.Series.Add(seriesBar);
                    chartTransactions.Titles.Clear();
                    chartTransactions.Titles.Add(new ChartTitle() { Text = "İşlem Hacmi (Son 5 Gün)", TextColor = Color.White });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart Error: {ex.Message}");
                
                // Fallback: Örnek veri ile grafikleri doldur
                LoadSampleChartData();
            }
        }

        private void LoadSampleChartData()
        {
            // Pie Chart
            Series seriesPie = new Series("Mevduat Dağılımı", ViewType.Doughnut);
            seriesPie.Points.Add(new SeriesPoint("TRY", 750000));
            seriesPie.Points.Add(new SeriesPoint("USD", 125000));
            seriesPie.Points.Add(new SeriesPoint("EUR", 85000));
            seriesPie.Points.Add(new SeriesPoint("Altın", 45000));
            
            var doughnutView = (DoughnutSeriesView)seriesPie.View;
            doughnutView.HoleRadiusPercent = 40;
            seriesPie.Label.TextPattern = "{A}: {VP:P1}";
            
            chartCurrency.Series.Clear();
            chartCurrency.Series.Add(seriesPie);
            chartCurrency.Titles.Clear();
            chartCurrency.Titles.Add(new ChartTitle() { Text = "Mevduat Dağılımı", TextColor = Color.White });

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
            chartTransactions.Titles.Add(new ChartTitle() { Text = "İşlem Hacmi (Son 5 Gün)", TextColor = Color.White });
        }

        private async void btnAiAssist_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                string advice = await _aiService.GetResponseAsync("Genel Durum");
                XtraMessageBox.Show(advice, "AI Finansal Asistan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("AI Servisine ulaşılamadı: " + ex.Message);
            }
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
        }

        private void btnBES_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            BESForm frm = new BESForm();
            frm.ShowDialog();
        }
    }
}
