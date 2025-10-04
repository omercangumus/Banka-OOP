using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    public partial class VerificationForm
    {
        private IContainer components = null;
        private Panel pnlGradient;
        private LabelControl lblTitle;
        private LabelControl lblInfo;
        private TextEdit txtCode;
        private SimpleButton btnVerify;

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
            this.lblInfo = new LabelControl();
            this.txtCode = new TextEdit();
            this.btnVerify = new SimpleButton();

            ((ISupportInitialize)(this.txtCode.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlGradient
            this.pnlGradient.BackColor = Color.FromArgb(26, 54, 93);
            this.pnlGradient.Dock = DockStyle.Fill;
            this.pnlGradient.Controls.Add(this.btnVerify);
            this.pnlGradient.Controls.Add(this.txtCode);
            this.pnlGradient.Controls.Add(this.lblInfo);
            this.pnlGradient.Controls.Add(this.lblTitle);

            // lblTitle
            this.lblTitle.Location = new Point(110, 30);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "HESAP DOĞRULAMA";
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.FromArgb(212, 175, 55);

            // lblInfo
            this.lblInfo.Location = new Point(80, 80);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Text = "E-posta adresinize gönderilen 6 haneli kodu giriniz.";
            this.lblInfo.Appearance.Font = new Font("Segoe UI", 10F);
            this.lblInfo.Appearance.ForeColor = Color.White;

            // txtCode
            this.txtCode.Location = new Point(100, 130);
            this.txtCode.Name = "txtCode";
            this.txtCode.Properties.NullValuePrompt = "000000";
            this.txtCode.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtCode.Properties.MaxLength = 6;
            this.txtCode.Size = new Size(200, 40);
            this.txtCode.Properties.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.txtCode.Properties.Appearance.Options.UseTextOptions = true;
            this.txtCode.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // btnVerify
            this.btnVerify.Location = new Point(100, 190);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new Size(200, 45);
            this.btnVerify.Text = "DOĞRULA";
            this.btnVerify.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnVerify.Appearance.BackColor = Color.FromArgb(212, 175, 55);
            this.btnVerify.Appearance.ForeColor = Color.FromArgb(26, 54, 93);
            this.btnVerify.Appearance.Options.UseBackColor = true;
            this.btnVerify.Appearance.Options.UseForeColor = true;
            this.btnVerify.Appearance.Options.UseFont = true;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
            
            // VerificationForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(550, 400);
            this.Controls.Add(this.pnlGradient);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "VerificationForm";
            this.Text = "NovaBank - Doğrulama";
            this.StartPosition = FormStartPosition.CenterScreen;

            ((ISupportInitialize)(this.txtCode.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

