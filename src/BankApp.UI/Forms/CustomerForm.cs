using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    public partial class CustomerForm : XtraForm
    {
        private readonly CustomerRepository _repository;
        private Customer _customer;
        private bool _isEditMode;

        public CustomerForm(Customer customer = null)
        {
            InitializeComponent();
            
            // Manual DI
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

        private void LoadCustomerData()
        {
            txtIdentity.Text = _customer.IdentityNumber;
            txtFirstName.Text = _customer.FirstName;
            txtLastName.Text = _customer.LastName;
            txtPhone.Text = _customer.PhoneNumber;
            txtEmail.Text = _customer.Email;
            txtAddress.Text = _customer.Address;
            txtIdentity.Enabled = false; // Cannot change ID in edit
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (txtIdentity == null || txtFirstName == null || _repository == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Simple Validation
            string identity = txtIdentity.Text?.Trim() ?? "";
            string firstName = txtFirstName.Text?.Trim() ?? "";
            
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
            _customer.LastName = txtLastName?.Text?.Trim();
            _customer.PhoneNumber = txtPhone?.Text?.Trim();
            _customer.Email = txtEmail?.Text?.Trim();
            _customer.Address = txtAddress?.Text?.Trim();

            try
            {
                // Audit Repo
                var auditRepo = new AuditRepository(new DapperContext());

                if (_isEditMode)
                {
                    await _repository.UpdateAsync(_customer);
                    await auditRepo.AddLogAsync(new AuditLog { UserId = 1, Action = "UpdateCustomer", Details = $"Updated: {_customer.IdentityNumber}", IpAddress = "127.0.0.1" });
                    XtraMessageBox.Show("Müşteri güncellendi.");
                }
                else
                {
                    // Assuming UserId 1 (Admin) creates this customer or linked to current user
                    // In real app, we might create a new User for this Customer or link to existing.
                    // For this simple CRUD, let's hardcode UserId = 1 (System)
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
