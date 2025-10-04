using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;
using BankApp.Core.Entities;

namespace BankApp.UI.Forms
{
    public partial class ForgotPasswordForm : XtraForm
    {
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;
        
        private string _generatedCode;
        private User _targetUser;

        public ForgotPasswordForm()
        {
            InitializeComponent();
            
            // Manual DI
            var context = new DapperContext();
            _userRepository = new UserRepository(context);
            var emailService = new SmtpEmailService();
            var auditRepo = new AuditRepository(context);
            _authService = new AuthService(_userRepository, emailService, auditRepo);
        }

        private async void btnSendCode_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (txtEmail == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string email = txtEmail.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(email))
            {
                XtraMessageBox.Show("Lütfen e-posta adresinizi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _targetUser = await _userRepository.GetByEmailAsync(email);
                if (_targetUser == null)
                {
                    XtraMessageBox.Show("Bu e-posta adresi ile kayıtlı kullanıcı bulunamadı.");
                    return;
                }

                _generatedCode = _authService.GenerateOTP();
                
                // In a real app, waiting for email sending might take time
                await _authService.SendForgotPasswordEmailAsync(email, _generatedCode);
                
                XtraMessageBox.Show("Doğrulama kodu e-posta adresinize gönderildi.");
                txtCode.Enabled = true;
                btnVerify.Enabled = true;
            }
            catch (Exception ex)
            {
                 XtraMessageBox.Show("Mail gönderim hatası: " + ex.Message);
            }
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (txtCode == null || _generatedCode == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtCode.Text?.Trim() == _generatedCode)
            {
                XtraMessageBox.Show("Kod doğrulandı! Yeni şifrenizi giriniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (txtNewPassword != null) txtNewPassword.Enabled = true;
                if (btnChangePassword != null) btnChangePassword.Enabled = true;
            }
            else
            {
                XtraMessageBox.Show("Hatalı kod!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnChangePassword_Click(object sender, EventArgs e)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (txtNewPassword == null || _targetUser == null || _authService == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string newPass = txtNewPassword.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(newPass))
            {
                XtraMessageBox.Show("Lütfen yeni şifrenizi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string newHash = _authService.HashPassword(newPass);
                _targetUser.PasswordHash = newHash;
                
                await _userRepository.UpdateAsync(_targetUser);
                
                XtraMessageBox.Show("Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Güncelleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
