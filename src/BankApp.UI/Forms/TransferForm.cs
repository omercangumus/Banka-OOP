using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Core.Entities;

namespace BankApp.UI.Forms
{
    public partial class TransferForm : XtraForm
    {
        private readonly AccountRepository _accountRepo;
        private readonly TransactionRepository _transactionRepo;
        private readonly TransactionService _transactionService;

        public TransferForm()
        {
            InitializeComponent();
            
            // Manual DI setup
            var context = new DapperContext();
            _accountRepo = new AccountRepository(context);
            _transactionRepo = new TransactionRepository(context);
            var auditRepo = new AuditRepository(context);
            _transactionService = new TransactionService(_accountRepo, _transactionRepo, auditRepo);

            LoadAccounts();
        }

        private async void LoadAccounts()
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (cmbSourceAccount == null || _accountRepo == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // In a real app, we would get the logged-in User ID from a session
                // For this demo, let's assume valid Customer linked to default User (e.g., CustomerId=1)
                // Note: The Seed Data might have created Customer with ID 1.
                var accounts = await _accountRepo.GetAllAsync(); // Simplified: Load ALL accounts for demo
                
                if (accounts != null)
                {
                    cmbSourceAccount.Properties.DataSource = accounts;
                    cmbSourceAccount.Properties.DisplayMember = "AccountNumber";
                    cmbSourceAccount.Properties.ValueMember = "Id";
                    
                    // Add columns to LookUpEdit for better UX
                    cmbSourceAccount.Properties.Columns.Clear();
                    cmbSourceAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AccountNumber", "Hesap No"));
                    cmbSourceAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Balance", "Bakiye"));
                    cmbSourceAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("CurrencyCode", "Döviz"));
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hesaplar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnTransfer_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (cmbSourceAccount == null || txtTargetIban == null || txtAmount == null || txtDescription == null || _transactionService == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbSourceAccount.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen kaynak hesap seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int fromAccountId = (int)cmbSourceAccount.EditValue;
            string targetIban = txtTargetIban.Text?.Trim() ?? "";
            decimal amount = txtAmount.Value;
            string desc = txtDescription.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(targetIban))
            {
                XtraMessageBox.Show("Hedef IBAN boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (amount <= 0)
            {
                XtraMessageBox.Show("Tutar 0'dan büyük olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await _transactionService.TransferMoneyAsync(fromAccountId, targetIban, amount, desc);
                XtraMessageBox.Show("Transfer Başarılı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                 XtraMessageBox.Show("Transfer Hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
