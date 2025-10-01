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
            _authService = new AuthService(emailService);
        }

        private async void btnSendCode_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text;
            if (string.IsNullOrEmpty(email)) return;

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
            if (txtCode.Text == _generatedCode)
            {
                XtraMessageBox.Show("Kod doğrulandı! Yeni şifrenizi giriniz.");
                txtNewPassword.Enabled = true;
                btnChangePassword.Enabled = true;
            }
            else
            {
                XtraMessageBox.Show("Hatalı kod!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnChangePassword_Click(object sender, EventArgs e)
        {
            string newPass = txtNewPassword.Text;
            if (string.IsNullOrEmpty(newPass)) return;

            try
            {
                string newHash = _authService.HashPassword(newPass);
                _targetUser.PasswordHash = newHash;
                
                await _userRepository.UpdateAsync(_targetUser);
                
                XtraMessageBox.Show("Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.");
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Güncelleme hatası: " + ex.Message);
            }
        }
    }
}
