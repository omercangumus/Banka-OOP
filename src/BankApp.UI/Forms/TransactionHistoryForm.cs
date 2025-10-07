using System;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using BankApp.Core.Interfaces;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Hesap hareketleri formu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class TransactionHistoryForm : XtraForm
    {
        private readonly int _accountId;
        private readonly ITransactionRepository _transactionRepository;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        /// <param name="accountId">Hareketleri görüntülenecek hesap ID</param>
        public TransactionHistoryForm(int accountId)
        {
            InitializeComponent();
            _accountId = accountId;
            
            var context = new DapperContext();
            _transactionRepository = new TransactionRepository(context);
            
            LoadTransactions();
        }

        /// <summary>
        /// İşlemleri yükler
        /// </summary>
        private async void LoadTransactions()
        {
            if (grdIslemler == null || _transactionRepository == null)
            {
                return;
            }

            try
            {
                var transactions = await _transactionRepository.GetByAccountIdAsync(_accountId);
                grdIslemler.DataSource = transactions?.ToList() ?? new List<BankApp.Core.Entities.Transaction>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hareketler yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
