using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    public partial class LoginForm
    {
        private IContainer components = null;
        private Panel pnlGradient;
        private PictureBox picLogo;
        private LabelControl lblTitle;
        private LabelControl lblSubtitle;
        private TextEdit txtUsername;
        private TextEdit txtPassword;
        private SimpleButton btnLogin;
        private SimpleButton btnRegister;
        private HyperlinkLabelControl lnkForgotPassword;
        private LabelControl lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlGradient = new Panel();
            this.picLogo = new PictureBox();
            this.lblTitle = new LabelControl();
            this.lblSubtitle = new LabelControl();
            this.txtUsername = new TextEdit();
            this.txtPassword = new TextEdit();
            this.btnLogin = new SimpleButton();
            this.btnRegister = new SimpleButton();
            this.lnkForgotPassword = new HyperlinkLabelControl();
            this.lblStatus = new LabelControl();

            ((ISupportInitialize)(this.picLogo)).BeginInit();
            ((ISupportInitialize)(this.txtUsername.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlGradient - Main container with gradient effect - TASARIM İYİLEŞTİRİLDİ
            this.pnlGradient.BackColor = Color.FromArgb(15, 23, 42); // Modern dark blue
            this.pnlGradient.Dock = DockStyle.Fill;
            // Gradient efekti için Paint event
            this.pnlGradient.Paint += (s, e) => {
                using (System.Drawing.Drawing2D.LinearGradientBrush gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0), new Point(0, this.pnlGradient.Height),
                    Color.FromArgb(15, 23, 42),
                    Color.FromArgb(30, 41, 59)))
                {
                    e.Graphics.FillRectangle(gradientBrush, this.pnlGradient.ClientRectangle);
                }
            };
            this.pnlGradient.Controls.Add(this.lblStatus);
            this.pnlGradient.Controls.Add(this.lnkForgotPassword);
            this.pnlGradient.Controls.Add(this.btnRegister);
            this.pnlGradient.Controls.Add(this.btnLogin);
            this.pnlGradient.Controls.Add(this.txtPassword);
            this.pnlGradient.Controls.Add(this.txtUsername);
            this.pnlGradient.Controls.Add(this.lblSubtitle);
            this.pnlGradient.Controls.Add(this.lblTitle);
            this.pnlGradient.Controls.Add(this.picLogo);

            // picLogo - PROFESYONEL BOYUTLAR
            this.picLogo.Location = new Point(325, 60);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new Size(180, 180);
            this.picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.picLogo.BackColor = Color.Transparent;
            try
            {
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "..", "..", "..", "Resources", "novabank_logo.png");
                if (System.IO.File.Exists(logoPath))
                    this.picLogo.Image = Image.FromFile(logoPath);
            }
            catch { }

            // lblTitle - PROFESYONEL BOYUTLAR
            this.lblTitle.Location = new Point(200, 260);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "NOVABANK";
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.FromArgb(212, 175, 55); // Gold
            this.lblTitle.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblTitle.Size = new Size(420, 60);
            this.lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // lblSubtitle - PROFESYONEL BOYUTLAR
            this.lblSubtitle.Location = new Point(180, 330);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "Güvenli Kurumsal Bankacılık";
            this.lblSubtitle.Appearance.Font = new Font("Segoe UI Light", 14F, FontStyle.Italic);
            this.lblSubtitle.Appearance.ForeColor = Color.FromArgb(200, 200, 200);
            this.lblSubtitle.Size = new Size(460, 25);
            this.lblSubtitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // txtUsername - PROFESYONEL BOYUTLAR
            this.txtUsername.Location = new Point(200, 390);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Properties.NullValuePrompt = "Kullanıcı Adı";
            this.txtUsername.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtUsername.Size = new Size(420, 56);
            this.txtUsername.Properties.Appearance.Font = new Font("Segoe UI", 14F);
            this.txtUsername.Properties.Appearance.BackColor = Color.White;
            this.txtUsername.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtUsername.Properties.Appearance.Options.UseBackColor = true;
            this.txtUsername.Properties.Appearance.Options.UseForeColor = true;
            this.txtUsername.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtUsername.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtUsername.Properties.Appearance.Options.UseBorderColor = true;
            this.txtUsername.Properties.AutoHeight = false;

            // txtPassword - PROFESYONEL BOYUTLAR
            this.txtPassword.Location = new Point(200, 465);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Properties.NullValuePrompt = "Şifre";
            this.txtPassword.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtPassword.Properties.UseSystemPasswordChar = true;
            this.txtPassword.Size = new Size(420, 56);
            this.txtPassword.Properties.Appearance.Font = new Font("Segoe UI", 14F);
            this.txtPassword.Properties.Appearance.BackColor = Color.White;
            this.txtPassword.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtPassword.Properties.Appearance.Options.UseBackColor = true;
            this.txtPassword.Properties.Appearance.Options.UseForeColor = true;
            this.txtPassword.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtPassword.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtPassword.Properties.Appearance.Options.UseBorderColor = true;
            this.txtPassword.Properties.AutoHeight = false;

            // btnLogin - PROFESYONEL BOYUTLAR
            this.btnLogin.Location = new Point(200, 545);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new Size(420, 60);
            this.btnLogin.Text = "GİRİŞ YAP";
            this.btnLogin.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.btnLogin.Appearance.BackColor = Color.FromArgb(212, 175, 55); // Gold
            this.btnLogin.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.btnLogin.Appearance.Options.UseBackColor = true;
            this.btnLogin.Appearance.Options.UseForeColor = true;
            this.btnLogin.Appearance.Options.UseFont = true;
            this.btnLogin.AppearanceHovered.BackColor = Color.FromArgb(235, 195, 75);
            this.btnLogin.AppearanceHovered.Options.UseBackColor = true;
            this.btnLogin.AppearancePressed.BackColor = Color.FromArgb(189, 155, 45);
            this.btnLogin.AppearancePressed.Options.UseBackColor = true;
            this.btnLogin.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            // btnRegister - PROFESYONEL BOYUTLAR
            this.btnRegister.Location = new Point(200, 625);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new Size(420, 56);
            this.btnRegister.Text = "YENİ HESAP OLUŞTUR";
            this.btnRegister.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.btnRegister.Appearance.BackColor = Color.FromArgb(30, 41, 59);
            this.btnRegister.Appearance.ForeColor = Color.White;
            this.btnRegister.Appearance.Options.UseBackColor = true;
            this.btnRegister.Appearance.Options.UseForeColor = true;
            this.btnRegister.Appearance.Options.UseFont = true;
            this.btnRegister.AppearanceHovered.BackColor = Color.FromArgb(51, 65, 85);
            this.btnRegister.AppearanceHovered.Options.UseBackColor = true;
            this.btnRegister.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);

            // lnkForgotPassword - PROFESYONEL BOYUTLAR
            this.lnkForgotPassword.Location = new Point(295, 695);
            this.lnkForgotPassword.Name = "lnkForgotPassword";
            this.lnkForgotPassword.Text = "<href>Şifremi Unuttum</href>";
            this.lnkForgotPassword.Appearance.ForeColor = Color.FromArgb(212, 175, 55);
            this.lnkForgotPassword.Appearance.Font = new Font("Segoe UI", 12F);
            this.lnkForgotPassword.Size = new Size(230, 25);
            this.lnkForgotPassword.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lnkForgotPassword.Click += new System.EventHandler(this.lnkForgotPassword_Click);

            // lblStatus - PROFESYONEL BOYUTLAR
            this.lblStatus.Location = new Point(275, 730);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "v1.0 - Güvenli Bağlantı | SSL Şifreli";
            this.lblStatus.Appearance.ForeColor = Color.FromArgb(150, 150, 150);
            this.lblStatus.Appearance.Font = new Font("Segoe UI", 10F);
            this.lblStatus.Size = new Size(270, 20);
            this.lblStatus.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            
            // LoginForm - PROFESYONEL BOYUTLAR
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(820, 780);
            this.Controls.Add(this.pnlGradient);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "LoginForm";
            this.Text = "NovaBank - Güvenli Giriş";
            this.StartPosition = FormStartPosition.CenterScreen;

            ((ISupportInitialize)(this.picLogo)).EndInit();
            ((ISupportInitialize)(this.txtUsername.Properties)).EndInit();
            ((ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

