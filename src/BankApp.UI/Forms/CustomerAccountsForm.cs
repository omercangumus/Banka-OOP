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
            try
            {
                var accounts = await _accountRepo.GetByCustomerIdAsync(_customerId);
                gridAccounts.DataSource = accounts;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hesaplar yüklenemedi: " + ex.Message);
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
            var row = gridViewAccounts.GetFocusedRow();
            if (row == null) return;
            
            if (row is Account account)
            {
                // In a full implementation, we would query Transactions by AccountId
                // For now, let's assume we implement TransactionHistoryForm next
                TransactionHistoryForm frm = new TransactionHistoryForm(account.Id);
                frm.ShowDialog();
            }
        }
    }
}
