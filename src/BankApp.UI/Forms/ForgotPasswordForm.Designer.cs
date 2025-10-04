using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    public partial class ForgotPasswordForm
    {
        private IContainer components = null;
        private Panel pnlGradient;
        private LabelControl lblTitle;
        private LabelControl lblInstruction;
        private TextEdit txtEmail;
        private SimpleButton btnSendCode;
        private TextEdit txtCode;
        private SimpleButton btnVerify;
        private TextEdit txtNewPassword;
        private SimpleButton btnChangePassword;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlGradient = new Panel();
            this.lblTitle = new LabelControl();
            this.lblInstruction = new LabelControl();
            this.txtEmail = new TextEdit();
            this.btnSendCode = new SimpleButton();
            this.txtCode = new TextEdit();
            this.btnVerify = new SimpleButton();
            this.txtNewPassword = new TextEdit();
            this.btnChangePassword = new SimpleButton();

            ((ISupportInitialize)(this.txtEmail.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtCode.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtNewPassword.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlGradient
            this.pnlGradient.BackColor = Color.FromArgb(26, 54, 93);
            this.pnlGradient.Dock = DockStyle.Fill;
            this.pnlGradient.Controls.Add(this.btnChangePassword);
            this.pnlGradient.Controls.Add(this.txtNewPassword);
            this.pnlGradient.Controls.Add(this.btnVerify);
            this.pnlGradient.Controls.Add(this.txtCode);
            this.pnlGradient.Controls.Add(this.btnSendCode);
            this.pnlGradient.Controls.Add(this.txtEmail);
            this.pnlGradient.Controls.Add(this.lblInstruction);
            this.pnlGradient.Controls.Add(this.lblTitle);

            // lblTitle
            this.lblTitle.Location = new Point(130, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "ŞİFRE SIFIRLAMA";
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.FromArgb(212, 175, 55);

            // lblInstruction
            this.lblInstruction.Location = new Point(80, 65);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Text = "Lütfen kayıtlı e-posta adresinizi giriniz.";
            this.lblInstruction.Appearance.Font = new Font("Segoe UI", 10F);
            this.lblInstruction.Appearance.ForeColor = Color.White;

            // txtEmail
            this.txtEmail.Location = new Point(80, 100);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Properties.NullValuePrompt = "E-Posta Adresi";
            this.txtEmail.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtEmail.Size = new Size(250, 34);
            this.txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 11F);

            // btnSendCode
            this.btnSendCode.Location = new Point(340, 100);
            this.btnSendCode.Name = "btnSendCode";
            this.btnSendCode.Size = new Size(100, 34);
            this.btnSendCode.Text = "KOD GÖNDER";
            this.btnSendCode.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnSendCode.Appearance.BackColor = Color.FromArgb(44, 82, 130);
            this.btnSendCode.Appearance.ForeColor = Color.White;
            this.btnSendCode.Appearance.Options.UseBackColor = true;
            this.btnSendCode.Appearance.Options.UseForeColor = true;
            this.btnSendCode.Click += new System.EventHandler(this.btnSendCode_Click);

            // txtCode
            this.txtCode.Location = new Point(80, 150);
            this.txtCode.Name = "txtCode";
            this.txtCode.Properties.NullValuePrompt = "Doğrulama Kodu";
            this.txtCode.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtCode.Size = new Size(150, 34);
            this.txtCode.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            this.txtCode.Enabled = false;

            // btnVerify
            this.btnVerify.Location = new Point(240, 150);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new Size(100, 34);
            this.btnVerify.Text = "DOĞRULA";
            this.btnVerify.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnVerify.Appearance.BackColor = Color.FromArgb(44, 82, 130);
            this.btnVerify.Appearance.ForeColor = Color.White;
            this.btnVerify.Appearance.Options.UseBackColor = true;
            this.btnVerify.Appearance.Options.UseForeColor = true;
            this.btnVerify.Enabled = false;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);

            // txtNewPassword
            this.txtNewPassword.Location = new Point(80, 200);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.Properties.NullValuePrompt = "Yeni Şifre";
            this.txtNewPassword.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtNewPassword.Properties.UseSystemPasswordChar = true;
            this.txtNewPassword.Size = new Size(260, 34);
            this.txtNewPassword.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            this.txtNewPassword.Enabled = false;

            // btnChangePassword
            this.btnChangePassword.Location = new Point(80, 250);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new Size(260, 45);
            this.btnChangePassword.Text = "ŞİFREYİ DEĞİŞTİR";
            this.btnChangePassword.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnChangePassword.Appearance.BackColor = Color.FromArgb(212, 175, 55);
            this.btnChangePassword.Appearance.ForeColor = Color.FromArgb(26, 54, 93);
            this.btnChangePassword.Appearance.Options.UseBackColor = true;
            this.btnChangePassword.Appearance.Options.UseForeColor = true;
            this.btnChangePassword.Appearance.Options.UseFont = true;
            this.btnChangePassword.Enabled = false;
            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);

            // ForgotPasswordForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(650, 450);
            this.Controls.Add(this.pnlGradient);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ForgotPasswordForm";
            this.Text = "NovaBank - Şifre Sıfırlama";
            this.StartPosition = FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.txtEmail.Properties)).EndInit();
            ((ISupportInitialize)(this.txtCode.Properties)).EndInit();
            ((ISupportInitialize)(this.txtNewPassword.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

