using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;
using BankApp.Core.Entities;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Şifre sıfırlama formu - E-posta ile şifre yenileme
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class ForgotPasswordForm : XtraForm
    {
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;
        
        private string _generatedCode;
        private User _targetUser;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public ForgotPasswordForm()
        {
            InitializeComponent();
            
            var context = new DapperContext();
            _userRepository = new UserRepository(context);
            var emailService = new SmtpEmailService();
            var auditRepo = new AuditRepository(context);
            _authService = new AuthService(_userRepository, emailService, auditRepo);
        }

        /// <summary>
        /// Kod gönder butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnKodGonder_Click(object sender, EventArgs e)
        {
            if (txtEposta == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string email = txtEposta.Text?.Trim() ?? "";
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
                    XtraMessageBox.Show("Bu e-posta adresi ile kayıtlı kullanıcı bulunamadı.", "Kullanıcı Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _generatedCode = _authService.GenerateOTP();
                
                await _authService.SendForgotPasswordEmailAsync(email, _generatedCode);
                
                XtraMessageBox.Show("Doğrulama kodu e-posta adresinize gönderildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtKod.Enabled = true;
                btnDogrula.Enabled = true;
            }
            catch (Exception ex)
            {
                 XtraMessageBox.Show("Mail gönderim hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Doğrula butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private void btnDogrula_Click(object sender, EventArgs e)
        {
            if (txtKod == null || _generatedCode == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtKod.Text?.Trim() == _generatedCode)
            {
                XtraMessageBox.Show("Kod doğrulandı! Yeni şifrenizi giriniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (txtYeniSifre != null) txtYeniSifre.Enabled = true;
                if (btnSifreDegistir != null) btnSifreDegistir.Enabled = true;
            }
            else
            {
                XtraMessageBox.Show("Hatalı kod!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Şifre değiştir butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnSifreDegistir_Click(object sender, EventArgs e)
        {
            if (txtYeniSifre == null || _targetUser == null || _authService == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string newPass = txtYeniSifre.Text?.Trim() ?? "";
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
