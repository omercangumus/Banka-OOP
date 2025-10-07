using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Yeni hesap açma formu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class NewAccountForm : XtraForm
    {
        private readonly int _customerId;
        private readonly AccountRepository _repository;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        /// <param name="customerId">Hesap açılacak müşteri ID</param>
        public NewAccountForm(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            
            var context = new DapperContext();
            _repository = new AccountRepository(context);
            
            cmbParaBirimi.SelectedIndex = 0; // Varsayılan: TRY
        }

        /// <summary>
        /// Hesap oluştur butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnOlustur_Click(object sender, EventArgs e)
        {
            if (cmbParaBirimi == null || calcIlkPara == null || _repository == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string currency = cmbParaBirimi.Text?.Trim() ?? "TRY";
            decimal balance = calcIlkPara.Value;

            if (string.IsNullOrWhiteSpace(currency))
            {
                XtraMessageBox.Show("Lütfen döviz cinsi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if(balance < 0) 
            {
               XtraMessageBox.Show("Bakiye negatif olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               return;
            }

            try
            {
                // Rastgele Hesap No ve IBAN oluştur
                var rand = new Random();
                string accNo = rand.Next(10000000, 99999999).ToString();
                string iban = $"TR{rand.Next(10, 99)}00000{accNo}";

                var newAcc = new Account
                {
                    CustomerId = _customerId,
                    AccountNumber = accNo,
                    IBAN = iban,
                    Balance = balance,
                    CurrencyCode = currency
                };

                await _repository.AddAsync(newAcc);
                XtraMessageBox.Show($"Hesap açıldı.\nIBAN: {iban}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hesap açma hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
