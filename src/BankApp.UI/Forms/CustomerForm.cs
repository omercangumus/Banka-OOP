using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Müşteri formu - Müşteri CRUD işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class CustomerForm : XtraForm
    {
        private readonly CustomerRepository _repository;
        private Customer _customer;
        private bool _isEditMode;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        /// <param name="customer">Düzenlenecek müşteri (null ise yeni kayıt)</param>
        public CustomerForm(Customer customer = null)
        {
            InitializeComponent();
            
            var context = new DapperContext();
            _repository = new CustomerRepository(context);

            if (customer != null)
            {
                _customer = customer;
                _isEditMode = true;
                LoadCustomerData();
            }
            else
            {
                _customer = new Customer();
                _isEditMode = false;
            }
        }

        /// <summary>
        /// Müşteri verilerini form kontrollerine yükler
        /// </summary>
        private void LoadCustomerData()
        {
            txtTcKimlikNo.Text = _customer.IdentityNumber;
            txtAd.Text = _customer.FirstName;
            txtSoyad.Text = _customer.LastName;
            txtTelefon.Text = _customer.PhoneNumber;
            txtEposta.Text = _customer.Email;
            memoAdres.Text = _customer.Address;
            txtTcKimlikNo.Enabled = false; // Düzenleme modunda TC değiştirilemez
        }

        /// <summary>
        /// Kaydet butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnKaydet_Click(object sender, EventArgs e)
        {
            if (txtTcKimlikNo == null || txtAd == null || _repository == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string identity = txtTcKimlikNo.Text?.Trim() ?? "";
            string firstName = txtAd.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(identity) || string.IsNullOrWhiteSpace(firstName))
            {
                XtraMessageBox.Show("TC No ve Ad alanları zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (identity.Length != 11)
            {
                XtraMessageBox.Show("TC Kimlik No 11 haneli olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _customer.IdentityNumber = identity;
            _customer.FirstName = firstName;
            _customer.LastName = txtSoyad?.Text?.Trim();
            _customer.PhoneNumber = txtTelefon?.Text?.Trim();
            _customer.Email = txtEposta?.Text?.Trim();
            _customer.Address = memoAdres?.Text?.Trim();

            try
            {
                var auditRepo = new AuditRepository(new DapperContext());

                if (_isEditMode)
                {
                    await _repository.UpdateAsync(_customer);
                    await auditRepo.AddLogAsync(new AuditLog { UserId = 1, Action = "UpdateCustomer", Details = $"Updated: {_customer.IdentityNumber}", IpAddress = "127.0.0.1" });
                    XtraMessageBox.Show("Müşteri güncellendi.");
                }
                else
                {
                    _customer.UserId = 1; 
                    await _repository.AddAsync(_customer);
                    await auditRepo.AddLogAsync(new AuditLog { UserId = 1, Action = "AddCustomer", Details = $"Created: {_customer.IdentityNumber}", IpAddress = "127.0.0.1" });
                    XtraMessageBox.Show("Müşteri eklendi.");
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Kaydetme hatası: " + ex.Message);
            }
        }
    }
}
