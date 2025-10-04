using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    public partial class NewAccountForm : XtraForm
    {
        private readonly int _customerId;
        private readonly AccountRepository _repository;

        public NewAccountForm(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            
            var context = new DapperContext();
            _repository = new AccountRepository(context);
            
            cmbCurrency.SelectedIndex = 0; // Default TRY
        }

        private async void btnCreate_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (cmbCurrency == null || txtInitialDeposit == null || _repository == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string currency = cmbCurrency.Text?.Trim() ?? "TRY";
            decimal balance = txtInitialDeposit.Value;

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
                // Generate Random Account No & IBAN
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
                XtraMessageBox.Show("Hesap açma hatası: " + ex.Message);
            }
        }
    }
}
