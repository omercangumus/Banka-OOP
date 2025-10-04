using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    public partial class RegisterForm : XtraForm
    {
        private readonly AuthService _authService;

        public RegisterForm()
        {
            InitializeComponent();
            
            // Manual DI
            var context = new DapperContext();
            var userRepo = new UserRepository(context);
            var emailService = new SmtpEmailService();
            var auditRepo = new AuditRepository(context);
            
            _authService = new AuthService(userRepo, emailService, auditRepo);
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (txtUsername == null || txtPassword == null || txtEmail == null || txtFullName == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi. Lütfen uygulamayı yeniden başlatın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string username = txtUsername.Text?.Trim() ?? "";
            string password = txtPassword.Text ?? "";
            string email = txtEmail.Text?.Trim() ?? "";
            string fullName = txtFullName.Text?.Trim() ?? "";

            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                XtraMessageBox.Show("Lütfen zorunlu alanları doldurunuz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    FullName = fullName,
                    Role = "Customer"
                };

                await _authService.RegisterAsync(newUser, password);
                
                XtraMessageBox.Show("Kayıt başarılı! Lütfen e-posta adresinize gelen doğrulama kodunu giriniz.");
                
                // Open Verification Form
                VerificationForm verifyForm = new VerificationForm(email);
                this.Hide();
                if(verifyForm.ShowDialog() == DialogResult.OK)
                {
                    this.Close(); // Success
                }
                else
                {
                    this.Show(); // Failed or Cancelled, show register again
                }
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show("Kayıt hatası: " + ex.Message);
            }
        }

        private void lnkLogin_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
