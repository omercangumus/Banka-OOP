using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Giriş formu - Kullanıcı girişi ve kayıt işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class LoginForm : XtraForm
    {
        private UserRepository _userRepository;
        private AuthService _authService;
        private bool _isInitialized = false;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
            InitializeServices();
        }

        /// <summary>
        /// Servisleri başlatır
        /// </summary>
        private void InitializeServices()
        {
            try
            {
                var context = new DapperContext();
                _userRepository = new UserRepository(context);
                var emailService = new SmtpEmailService();
                var auditRepo = new AuditRepository(context);
                _authService = new AuthService(_userRepository, emailService, auditRepo);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Servis başlatma hatası: {ex.Message}", "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Giriş butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private async void btnGiris_Click(object sender, EventArgs e)
        {
            if (!_isInitialized || _authService == null)
            {
                XtraMessageBox.Show("Sistem başlatılamadı. Lütfen uygulamayı yeniden başlatın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtKullaniciAdi == null || txtSifre == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi. Lütfen uygulamayı yeniden başlatın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string username = txtKullaniciAdi.Text?.Trim() ?? "";
            string password = txtSifre.Text ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                XtraMessageBox.Show("Lütfen kullanıcı adı ve şifre giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success = await _authService.LoginAsync(username, password);
                if (success)
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null)
                    {
                        AppEvents.CurrentSession.Set(user.Id, user.Username, user.Role);
                        AppEvents.NotifyUserLoggedIn(user.Id, user.Username, user.Role);
                    }
                    
                    this.DialogResult = DialogResult.OK;
                    MainForm mainForm = new MainForm();
                    this.Hide();
                    mainForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Giriş Başarısız", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Giriş hatası: {ex.Message}\n\nDetay: {ex.InnerException?.Message ?? "Yok"}", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Şifremi unuttum linki tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private void llblSifremiUnuttum_Click(object sender, EventArgs e)
        {
            ForgotPasswordForm forgotForm = new ForgotPasswordForm();
            forgotForm.ShowDialog();
        }

        /// <summary>
        /// Kayıt ol butonu tıklama olayı
        /// </summary>
        /// <param name="sender">Olay kaynağı</param>
        /// <param name="e">Olay argümanları</param>
        private void btnKayitOl_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }
    }
}
