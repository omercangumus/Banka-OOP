using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Core.Entities;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Para transferi formu - EFT/Havale işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class TransferForm : XtraForm
    {
        private readonly AccountRepository _accountRepo;
        private readonly TransactionRepository _transactionRepo;
        private readonly TransactionService _transactionService;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public TransferForm()
        {
            InitializeComponent();
            
            var context = new DapperContext();
            _accountRepo = new AccountRepository(context);
            _transactionRepo = new TransactionRepository(context);
            var auditRepo = new AuditRepository(context);
            _transactionService = new TransactionService(_accountRepo, _transactionRepo, auditRepo);

            LoadAccounts();
        }

        /// <summary>
        /// Hesapları yükler
        /// </summary>
        private async void LoadAccounts()
        {
            if (lueKaynakHesap == null || _accountRepo == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var accounts = await _accountRepo.GetAllAsync();
                
                if (accounts != null)
                {
                    lueKaynakHesap.Properties.DataSource = accounts;
                    lueKaynakHesap.Properties.DisplayMember = "AccountNumber";
                    lueKaynakHesap.Properties.ValueMember = "Id";
                    
                    lueKaynakHesap.Properties.Columns.Clear();
                    lueKaynakHesap.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AccountNumber", "Hesap No"));
                    lueKaynakHesap.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Balance", "Bakiye"));
                    lueKaynakHesap.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("CurrencyCode", "Döviz"));
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hesaplar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gönder butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnGonder_Click(object sender, EventArgs e)
        {
            if (lueKaynakHesap == null || txtHedefIban == null || calcTutar == null || memoAciklama == null || _transactionService == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (lueKaynakHesap.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen kaynak hesap seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int fromAccountId = (int)lueKaynakHesap.EditValue;
            string targetIban = txtHedefIban.Text?.Trim() ?? "";
            decimal amount = calcTutar.Value;
            string desc = memoAciklama.Text?.Trim() ?? "";

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
                // TransactionService artık string döndürüyor: null = başarılı, string = hata mesajı
                string transferResult = await _transactionService.TransferMoneyAsync(fromAccountId, targetIban, amount, desc);
                
                if (transferResult == null)
                {
                    XtraMessageBox.Show("Transfer Başarılı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show(transferResult, "Transfer Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                 XtraMessageBox.Show("Transfer Hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
