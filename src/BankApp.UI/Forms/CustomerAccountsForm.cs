using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    public partial class CustomerAccountsForm : XtraForm
    {
        private readonly int _customerId;
        private readonly AccountRepository _accountRepo;

        public CustomerAccountsForm(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            
            var context = new DapperContext();
            _accountRepo = new AccountRepository(context);

            LoadAccounts();
        }

        private async void LoadAccounts()
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (gridAccounts == null || _accountRepo == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var accounts = await _accountRepo.GetByCustomerIdAsync(_customerId);
                gridAccounts.DataSource = accounts ?? new List<Account>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hesaplar yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnNewAccount_Click(object sender, EventArgs e)
        {
            NewAccountForm frm = new NewAccountForm(_customerId);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadAccounts();
            }
        }

        private void btnTransactions_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (gridViewAccounts == null)
            {
                XtraMessageBox.Show("Hesap listesi yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var row = gridViewAccounts.GetFocusedRow();
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen bir hesap seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (row is Account account)
            {
                TransactionHistoryForm frm = new TransactionHistoryForm(account.Id);
                frm.ShowDialog();
            }
            else
            {
                XtraMessageBox.Show("Geçersiz hesap verisi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
