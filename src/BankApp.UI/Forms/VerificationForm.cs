using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Doğrulama formu - E-posta doğrulama işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class VerificationForm : XtraForm
    {
        private readonly AuthService _authService;
        private readonly string _email;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        /// <param name="email">Doğrulanacak e-posta adresi</param>
        public VerificationForm(string email)
        {
            InitializeComponent();
            _email = email;
            
            var context = new DapperContext();
            var userRepo = new UserRepository(context);
            var emailService = new SmtpEmailService();
            var auditRepo = new AuditRepository(context);
            
            _authService = new AuthService(userRepo, emailService, auditRepo);
        }

        /// <summary>
        /// Doğrula butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnDogrula_Click(object sender, EventArgs e)
        {
            if (txtKod == null || _authService == null || string.IsNullOrWhiteSpace(_email))
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string code = txtKod.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(code))
            {
                XtraMessageBox.Show("Lütfen doğrulama kodunu giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // AuthService artık string döndürüyor: null = başarılı, string = hata mesajı
                string verifyResult = await _authService.VerifyAccountAsync(_email, code);
                
                if (verifyResult == null)
                {
                    XtraMessageBox.Show("Hesabınız doğrulandı! Giriş yapabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show(verifyResult, "Doğrulama Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Doğrulama hatası: {ex.Message}", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
