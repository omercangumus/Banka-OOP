using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    public partial class VerificationForm : XtraForm
    {
        private readonly AuthService _authService;
        private readonly string _email;

        public VerificationForm(string email)
        {
            InitializeComponent();
            _email = email;
            
            // Manual DI
            var context = new DapperContext();
            var userRepo = new UserRepository(context);
            var emailService = new SmtpEmailService();
            var auditRepo = new AuditRepository(context);
            
            _authService = new AuthService(userRepo, emailService, auditRepo);
        }

        private async void btnVerify_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (txtCode == null || _authService == null || string.IsNullOrWhiteSpace(_email))
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string code = txtCode.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(code))
            {
                XtraMessageBox.Show("Lütfen doğrulama kodunu giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await _authService.VerifyAccountAsync(_email, code);
                XtraMessageBox.Show("Hesabınız doğrulandı! Giriş yapabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Doğrulama hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
