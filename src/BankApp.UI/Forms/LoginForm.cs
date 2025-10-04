using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    public partial class LoginForm : XtraForm
    {
        private UserRepository _userRepository;
        private AuthService _authService;
        private bool _isInitialized = false;

        public LoginForm()
        {
            InitializeComponent();
            InitializeServices();
        }

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

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (!_isInitialized || _authService == null)
            {
                XtraMessageBox.Show("Sistem başlatılamadı. Lütfen uygulamayı yeniden başlatın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (txtUsername == null || txtPassword == null)
            {
                XtraMessageBox.Show("Form bileşenleri yüklenemedi. Lütfen uygulamayı yeniden başlatın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string username = txtUsername.Text?.Trim() ?? "";
            string password = txtPassword.Text ?? "";

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

        private void lnkForgotPassword_Click(object sender, EventArgs e)
        {
            ForgotPasswordForm forgotForm = new ForgotPasswordForm();
            forgotForm.ShowDialog();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }
    }
}

