using System;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using Dapper;

namespace BankApp.UI.Forms
{
    public partial class TransactionHistoryForm : XtraForm
    {
        private readonly int _accountId;

        public TransactionHistoryForm(int accountId)
        {
            InitializeComponent();
            _accountId = accountId;
            LoadTransactions();
        }

        private async void LoadTransactions()
        {
            try
            {
                var context = new DapperContext();
                using (var conn = context.CreateConnection())
                {
                    // Filter by AccountId logic (Adding simplified query here)
                    var query = "SELECT * FROM \"Transactions\" WHERE \"AccountId\" = @AccountId ORDER BY \"TransactionDate\" DESC";
                    var transactions = await conn.QueryAsync<BankApp.Core.Entities.Transaction>(query, new { AccountId = _accountId });
                    gridTransactions.DataSource = transactions;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hareketler yüklenemedi: " + ex.Message);
            }
        }
    }
}
