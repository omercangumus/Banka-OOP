using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    public partial class RegisterForm
    {
        private IContainer components = null;
        private Panel pnlGradient;
        private LabelControl lblTitle;
        private TextEdit txtUsername;
        private TextEdit txtPassword;
        private TextEdit txtEmail;
        private TextEdit txtFullName;
        private SimpleButton btnRegister;
        private HyperlinkLabelControl lnkLogin;

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
            this.lblTitle = new LabelControl();
            this.txtUsername = new TextEdit();
            this.txtPassword = new TextEdit();
            this.txtEmail = new TextEdit();
            this.txtFullName = new TextEdit();
            this.btnRegister = new SimpleButton();
            this.lnkLogin = new HyperlinkLabelControl();

            ((ISupportInitialize)(this.txtUsername.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtEmail.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtFullName.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlGradient - TASARIM İYİLEŞTİRİLDİ
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
            this.pnlGradient.Controls.Add(this.lnkLogin);
            this.pnlGradient.Controls.Add(this.btnRegister);
            this.pnlGradient.Controls.Add(this.txtPassword);
            this.pnlGradient.Controls.Add(this.txtEmail);
            this.pnlGradient.Controls.Add(this.txtFullName);
            this.pnlGradient.Controls.Add(this.txtUsername);
            this.pnlGradient.Controls.Add(this.lblTitle);

            // lblTitle - PROFESYONEL BOYUTLAR
            this.lblTitle.Location = new Point(150, 50);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "YENİ HESAP OLUŞTUR";
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.FromArgb(212, 175, 55);
            this.lblTitle.Size = new Size(520, 45);
            this.lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // txtUsername - PROFESYONEL BOYUTLAR
            this.txtUsername.Location = new Point(200, 130);
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

            // txtFullName - PROFESYONEL BOYUTLAR
            this.txtFullName.Location = new Point(200, 205);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Properties.NullValuePrompt = "Ad Soyad";
            this.txtFullName.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtFullName.Size = new Size(420, 56);
            this.txtFullName.Properties.Appearance.Font = new Font("Segoe UI", 14F);
            this.txtFullName.Properties.Appearance.BackColor = Color.White;
            this.txtFullName.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtFullName.Properties.Appearance.Options.UseBackColor = true;
            this.txtFullName.Properties.Appearance.Options.UseForeColor = true;
            this.txtFullName.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtFullName.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtFullName.Properties.Appearance.Options.UseBorderColor = true;
            this.txtFullName.Properties.AutoHeight = false;

            // txtEmail - PROFESYONEL BOYUTLAR
            this.txtEmail.Location = new Point(200, 280);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Properties.NullValuePrompt = "E-Posta Adresi";
            this.txtEmail.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtEmail.Size = new Size(420, 56);
            this.txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 14F);
            this.txtEmail.Properties.Appearance.BackColor = Color.White;
            this.txtEmail.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtEmail.Properties.Appearance.Options.UseBackColor = true;
            this.txtEmail.Properties.Appearance.Options.UseForeColor = true;
            this.txtEmail.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtEmail.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtEmail.Properties.Appearance.Options.UseBorderColor = true;
            this.txtEmail.Properties.AutoHeight = false;

            // txtPassword - PROFESYONEL BOYUTLAR
            this.txtPassword.Location = new Point(200, 355);
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

            // btnRegister - PROFESYONEL BOYUTLAR
            this.btnRegister.Location = new Point(200, 435);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new Size(420, 60);
            this.btnRegister.Text = "KAYIT OL";
            this.btnRegister.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.btnRegister.Appearance.BackColor = Color.FromArgb(212, 175, 55); // Gold
            this.btnRegister.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.btnRegister.Appearance.Options.UseBackColor = true;
            this.btnRegister.Appearance.Options.UseForeColor = true;
            this.btnRegister.Appearance.Options.UseFont = true;
            this.btnRegister.AppearanceHovered.BackColor = Color.FromArgb(235, 195, 75);
            this.btnRegister.AppearanceHovered.Options.UseBackColor = true;
            this.btnRegister.AppearancePressed.BackColor = Color.FromArgb(189, 155, 45);
            this.btnRegister.AppearancePressed.Options.UseBackColor = true;
            this.btnRegister.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);

            // lnkLogin - PROFESYONEL BOYUTLAR
            this.lnkLogin.Location = new Point(235, 515);
            this.lnkLogin.Name = "lnkLogin";
            this.lnkLogin.Text = "<href>Zaten hesabım var? Giriş Yap</href>";
            this.lnkLogin.Appearance.ForeColor = Color.FromArgb(212, 175, 55);
            this.lnkLogin.Appearance.Font = new Font("Segoe UI", 12F);
            this.lnkLogin.Size = new Size(350, 25);
            this.lnkLogin.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lnkLogin.Click += new System.EventHandler(this.lnkLogin_Click);
            
            // RegisterForm - PROFESYONEL BOYUTLAR
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(820, 600);
            this.Controls.Add(this.pnlGradient);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "RegisterForm";
            this.Text = "NovaBank - Yeni Hesap";
            this.StartPosition = FormStartPosition.CenterScreen;

            ((ISupportInitialize)(this.txtUsername.Properties)).EndInit();
            ((ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            ((ISupportInitialize)(this.txtEmail.Properties)).EndInit();
            ((ISupportInitialize)(this.txtFullName.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

