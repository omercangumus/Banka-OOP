using System.ComponentModel;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    partial class LoginForm
    {
        private IContainer components = null;
        private GroupControl groupControl1;
        private TextEdit txtUsername;
        private TextEdit txtPassword;
        private SimpleButton btnLogin;
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
            this.groupControl1 = new GroupControl();
            this.txtUsername = new TextEdit();
            this.txtPassword = new TextEdit();
            this.btnLogin = new SimpleButton();
            this.lnkForgotPassword = new HyperlinkLabelControl();
            this.lblStatus = new LabelControl();

            ((ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((ISupportInitialize)(this.txtUsername.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            this.SuspendLayout();

            // groupControl1
            this.groupControl1.Controls.Add(this.lblStatus);
            this.groupControl1.Controls.Add(this.lnkForgotPassword);
            this.groupControl1.Controls.Add(this.btnLogin);
            this.groupControl1.Controls.Add(this.txtPassword);
            this.groupControl1.Controls.Add(this.txtUsername);
            this.groupControl1.Location = new System.Drawing.Point(100, 80); 
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(400, 300);
            this.groupControl1.Text = "NovaBank Kurumsal";

            // txtUsername
            this.txtUsername.Location = new System.Drawing.Point(50, 50);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Properties.NullValuePrompt = "Kullanıcı Adı";
            this.txtUsername.Size = new System.Drawing.Size(300, 30);

            // txtPassword
            this.txtPassword.Location = new System.Drawing.Point(50, 100);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Properties.NullValuePrompt = "Şifre";
            this.txtPassword.Properties.UseSystemPasswordChar = true;
            this.txtPassword.Size = new System.Drawing.Size(300, 30);

            // btnLogin
            this.btnLogin.Location = new System.Drawing.Point(50, 150);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(300, 40);
            this.btnLogin.Text = "Giriş Yap";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            // lnkForgotPassword
            this.lnkForgotPassword.Location = new System.Drawing.Point(50, 200);
            this.lnkForgotPassword.Name = "lnkForgotPassword";
            this.lnkForgotPassword.Text = "<href>Şifremi Unuttum</href>";
            this.lnkForgotPassword.Click += new System.EventHandler(this.lnkForgotPassword_Click);

            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(50, 230);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "Sistem Hazır";
            
            // LoginForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Controls.Add(this.groupControl1);
            this.Name = "LoginForm";
            this.Text = "NovaBank - Güvenli Giriş";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            ((ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((ISupportInitialize)(this.txtUsername.Properties)).EndInit();
            ((ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
