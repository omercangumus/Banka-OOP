using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using System;
using System.Windows.Forms;
using BankApp.Core.Interfaces;
using BankApp.Infrastructure.Services;

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
            LoadDashboardCharts();
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
             // Simple Data Access for Grid
             try
             {
                 var context = new BankApp.Infrastructure.Data.DapperContext();
                 var repo = new BankApp.Infrastructure.Data.AccountRepository(context); // Could use CustomerRepository too
                 // For now, let's just query customers directly or use a known repo
                 // Or create a quick CustomerRepository instance if it exists, or just use AccountRepo.GetAllAsync for now?
                 // Wait, I didn't create CustomerRepository implementation in Step 2, I only created Interface?
                 // Let's check task.md or files. I created AccountRepository and UserRepository.
                 // I will create a quick inline query or dummy load if CustomerRepo is missing, 
                 // BUT to be professional I should probably use a proper way.
                 // Let's use Dapper directly here for speed as per "no spaghetti" but "fast" rule? 
                 // Actually, let's just query Accounts for now as it has data linked to customers.
                 // Or better: Create CustomerRepository class quickly? No, I'll use DapperContext directly here to fetch Customers.
                 
                 using(var conn = context.CreateConnection())
                 {
                    var customers = await conn.QueryAsync<BankApp.Core.Entities.Customer>("SELECT * FROM \"Customers\"");
                    gridCustomers.DataSource = customers;
                 }
             }
             catch(Exception ex) 
             {
                 // Silent fail or log
                 System.Diagnostics.Debug.WriteLine(ex.Message);
             }
        }

        private void RibbonControl1_SelectedPageChanged(object sender, EventArgs e)
        {
            bool isDashboard = (ribbonControl1.SelectedPage == pageDashboard);
            pnlDashboard.Visible = isDashboard;
            gridCustomers.Visible = !isDashboard; // Assuming only 2 tabs for now
            
            if(!isDashboard)
            {
                gridCustomers.Dock = DockStyle.Fill;
                gridCustomers.BringToFront();
            }
        }

        private void LoadDashboardCharts()
        {
            // 1. Currency Pie Chart
            Series seriesPie = new Series("Döviz Dağılımı", ViewType.Pie);
            seriesPie.Points.Add(new SeriesPoint("TRY", 750000));
            seriesPie.Points.Add(new SeriesPoint("USD", 500000));
            seriesPie.Points.Add(new SeriesPoint("EUR", 250000));
            
            // Add to chart
            chartCurrency.Series.Clear();
            chartCurrency.Series.Add(seriesPie);
            chartCurrency.Titles.Clear();
            chartCurrency.Titles.Add(new ChartTitle() { Text = "Mevduat Dağılımı" });

            // 2. Transactions Bar Chart
            Series seriesBar = new Series("Haftalık İşlemler", ViewType.Bar);
            seriesBar.Points.Add(new SeriesPoint("Pazartesi", 120));
            seriesBar.Points.Add(new SeriesPoint("Salı", 150));
            seriesBar.Points.Add(new SeriesPoint("Çarşamba", 80));
            seriesBar.Points.Add(new SeriesPoint("Perşembe", 200));
            seriesBar.Points.Add(new SeriesPoint("Cuma", 250));

            chartTransactions.Series.Clear();
            chartTransactions.Series.Add(seriesBar);
            chartTransactions.Titles.Clear();
            chartTransactions.Titles.Add(new ChartTitle() { Text = "İşlem Hacmi (Son 5 Gün)" });
        }

        private async void btnAiAssist_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Show AI Advice
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
            // Optionally refresh dashboard after transfer
            LoadDashboardCharts();
        }

        private async void btnAddCustomer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CustomerForm frm = new CustomerForm();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadCustomers();
            }
        }

        private async void btnEditCustomer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var row = gridViewCustomers.GetFocusedRow();
            if (row == null) return;

            // In real app, cast to Customer object. 
            // Since we used Dapper Query dynamic/Customer in LoadCustomers, it should be a Customer object.
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
            var row = gridViewCustomers.GetFocusedRow();
            if (row == null) return;

             if(row is BankApp.Core.Entities.Customer customer)
             {
                 if(XtraMessageBox.Show("Müşteriyi silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                 {
                     try
                     {
                         var context = new BankApp.Infrastructure.Data.DapperContext();
                         var repo = new BankApp.Infrastructure.Data.CustomerRepository(context);
                         await repo.DeleteAsync(customer.Id);
                         
                         // Audit
                         var audit = new BankApp.Infrastructure.Data.AuditRepository(context);
                         await audit.AddLogAsync(new BankApp.Core.Entities.AuditLog 
                         { 
                            UserId = 1, 
                            Action = "DeleteCustomer", 
                            Details = $"Deleted Customer: {customer.FirstName} {customer.LastName} ({customer.IdentityNumber})",
                            IpAddress = "127.0.0.1"
                         });

                         LoadCustomers();
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
            var row = gridViewCustomers.GetFocusedRow();
            if (row == null) return;

             if(row is BankApp.Core.Entities.Customer customer)
             {
                 CustomerAccountsForm frm = new CustomerAccountsForm(customer.Id);
                 frm.ShowDialog();
             }
        }

        private void btnExportExcel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
             try
             {
                 string path = "Musteriler.xlsx";
                 gridCustomers.ExportToXlsx(path);
                 System.Diagnostics.Process.Start(path);
             }
             catch(Exception ex)
             {
                 XtraMessageBox.Show("Export Hatası: " + ex.Message);
             }
        }

        private void btnExportPdf_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
             try
             {
                 string path = "Musteriler.pdf";
                 gridCustomers.ExportToPdf(path);
                 System.Diagnostics.Process.Start(path);
             }
             catch(Exception ex)
             {
                 XtraMessageBox.Show("Export Hatası: " + ex.Message);
             }
        }

        private void btnAuditLogs_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            AuditLogsForm frm = new AuditLogsForm();
            frm.ShowDialog();
        }
    }
}
