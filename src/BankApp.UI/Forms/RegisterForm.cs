using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Kayıt formu - Yeni kullanıcı oluşturma
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class RegisterForm : XtraForm
    {
        private readonly AuthService _authService;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public RegisterForm()
        {
            InitializeComponent();
            
            var context = new DapperContext();
            var userRepo = new UserRepository(context);
            var emailService = new SmtpEmailService();
            var auditRepo = new AuditRepository(context);
            
            _authService = new AuthService(userRepo, emailService, auditRepo);
        }

        /// <summary>
        /// Kayıt ol butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnKayitOl_Click(object sender, EventArgs e)
        {
            if (txtKullaniciAdi == null || txtSifre == null || txtEposta == null || txtAdSoyad == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi. Lütfen uygulamayı yeniden başlatın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string username = txtKullaniciAdi.Text?.Trim() ?? "";
            string password = txtSifre.Text ?? "";
            string email = txtEposta.Text?.Trim() ?? "";
            string fullName = txtAdSoyad.Text?.Trim() ?? "";

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

                // AuthService artık string döndürüyor: null = başarılı, string = hata mesajı
                string registerResult = await _authService.RegisterAsync(newUser, password);
                
                if (registerResult == null)
                {
                    XtraMessageBox.Show("Kayıt başarılı! Lütfen e-posta adresinize gelen doğrulama kodunu giriniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    VerificationForm verifyForm = new VerificationForm(email);
                    this.Hide();
                    if(verifyForm.ShowDialog() == DialogResult.OK)
                    {
                        this.Close();
                    }
                    else
                    {
                        this.Show();
                    }
                }
                else
                {
                    XtraMessageBox.Show(registerResult, "Kayıt Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show("Kayıt hatası: " + ex.Message, "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Giriş yap linki tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private void llblGirisYap_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
