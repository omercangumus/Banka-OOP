using System.ComponentModel;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    partial class ForgotPasswordForm
    {
        private IContainer components = null;
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

            // lblInstruction
            this.lblInstruction.Location = new System.Drawing.Point(30, 20);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Text = "Lütfen kayıtlı e-posta adresinizi giriniz.";

            // txtEmail
            this.txtEmail.Location = new System.Drawing.Point(30, 50);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Properties.NullValuePrompt = "E-Posta Adresi";
            this.txtEmail.Size = new System.Drawing.Size(250, 30);

            // btnSendCode
            this.btnSendCode.Location = new System.Drawing.Point(290, 50);
            this.btnSendCode.Name = "btnSendCode";
            this.btnSendCode.Size = new System.Drawing.Size(80, 30);
            this.btnSendCode.Text = "Kod Gönder";
            this.btnSendCode.Click += new System.EventHandler(this.btnSendCode_Click);

            // txtCode
            this.txtCode.Location = new System.Drawing.Point(30, 100);
            this.txtCode.Name = "txtCode";
            this.txtCode.Properties.NullValuePrompt = "000000";
            this.txtCode.Size = new System.Drawing.Size(100, 30);
            this.txtCode.Enabled = false;

            // btnVerify
            this.btnVerify.Location = new System.Drawing.Point(140, 100);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(80, 30);
            this.btnVerify.Text = "Doğrula";
            this.btnVerify.Enabled = false;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);

            // txtNewPassword
            this.txtNewPassword.Location = new System.Drawing.Point(30, 150);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.Properties.NullValuePrompt = "Yeni Şifre";
            this.txtNewPassword.Properties.UseSystemPasswordChar = true;
            this.txtNewPassword.Size = new System.Drawing.Size(250, 30);
            this.txtNewPassword.Enabled = false;

            // btnChangePassword
            this.btnChangePassword.Location = new System.Drawing.Point(30, 200);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(250, 40);
            this.btnChangePassword.Text = "Şifreyi Değiştir";
            this.btnChangePassword.Enabled = false;
            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);

            // ForgotPasswordForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.btnChangePassword);
            this.Controls.Add(this.txtNewPassword);
            this.Controls.Add(this.btnVerify);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.btnSendCode);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblInstruction);
            this.Name = "ForgotPasswordForm";
            this.Text = "Şifre Sıfırlama";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.txtEmail.Properties)).EndInit();
            ((ISupportInitialize)(this.txtCode.Properties)).EndInit();
            ((ISupportInitialize)(this.txtNewPassword.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
