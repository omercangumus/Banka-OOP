using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    public partial class LoginForm : XtraForm
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;

        public LoginForm()
        {
            InitializeComponent();
            
            // Dependency Injection (Manual for now)
            var context = new DapperContext();
            _userRepository = new UserRepository(context);
            var emailService = new SmtpEmailService(); // Configs are internal placeholders
            var auditRepo = new AuditRepository(context);
            _authService = new AuthService(_userRepository, emailService, auditRepo);
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

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
                    XtraMessageBox.Show("Hatalı şifre.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Bir hata oluştu: {ex.Message}", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkForgotPassword_Click(object sender, EventArgs e)
        {
            // Open Forgot Password Form
            ForgotPasswordForm forgotForm = new ForgotPasswordForm();
            forgotForm.ShowDialog();
        }
    }
}
