using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Müşteri hesapları formu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class CustomerAccountsForm : XtraForm
    {
        private readonly int _customerId;
        private readonly AccountRepository _accountRepo;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        /// <param name="customerId">Hesapları listelenecek müşteri ID</param>
        public CustomerAccountsForm(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            
            var context = new DapperContext();
            _accountRepo = new AccountRepository(context);

            LoadAccounts();
        }

        /// <summary>
        /// Hesapları yükler
        /// </summary>
        private async void LoadAccounts()
        {
            if (grdHesaplar == null || _accountRepo == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var accounts = await _accountRepo.GetByCustomerIdAsync(_customerId);
                grdHesaplar.DataSource = accounts ?? new List<Account>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hesaplar yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Yeni hesap butonu tıklama olayı
        /// </summary>
        private void btnYeniHesap_Click(object sender, EventArgs e)
        {
            NewAccountForm frm = new NewAccountForm(_customerId);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadAccounts();
            }
        }

        /// <summary>
        /// Hareketler butonu tıklama olayı
        /// </summary>
        private void btnHareketler_Click(object sender, EventArgs e)
        {
            if (grdwHesaplar == null)
            {
                XtraMessageBox.Show("Hesap listesi yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var row = grdwHesaplar.GetFocusedRow();
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
