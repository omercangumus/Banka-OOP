using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Doğrulama formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class VerificationForm
    {
        private IContainer components = null;
        private Panel pnlGradient;
        private LabelControl lblBaslik;
        private LabelControl lblAciklama;
        private TextEdit txtKod;
        private SimpleButton btnDogrula;

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
            this.lblBaslik = new LabelControl();
            this.lblAciklama = new LabelControl();
            this.txtKod = new TextEdit();
            this.btnDogrula = new SimpleButton();

            ((ISupportInitialize)(this.txtKod.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlGradient
            this.pnlGradient.BackColor = Color.FromArgb(26, 54, 93);
            this.pnlGradient.Dock = DockStyle.Fill;
            this.pnlGradient.Controls.Add(this.btnDogrula);
            this.pnlGradient.Controls.Add(this.txtKod);
            this.pnlGradient.Controls.Add(this.lblAciklama);
            this.pnlGradient.Controls.Add(this.lblBaslik);

            // lblBaslik (Fırat Standardı: Tahoma font)
            this.lblBaslik.Location = new Point(110, 30);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.Text = "HESAP DOĞRULAMA";
            this.lblBaslik.Appearance.Font = new Font("Tahoma", 16F, FontStyle.Bold);
            this.lblBaslik.Appearance.ForeColor = Color.FromArgb(212, 175, 55);

            // lblAciklama (Fırat Standardı: Tahoma font)
            this.lblAciklama.Location = new Point(80, 80);
            this.lblAciklama.Name = "lblAciklama";
            this.lblAciklama.Text = "E-posta adresinize gönderilen 6 haneli kodu giriniz.";
            this.lblAciklama.Appearance.Font = new Font("Tahoma", 9F);
            this.lblAciklama.Appearance.ForeColor = Color.White;

            // txtKod (Fırat Standardı: txt prefix, Tahoma font)
            this.txtKod.Location = new Point(100, 130);
            this.txtKod.Name = "txtKod";
            this.txtKod.Properties.NullValuePrompt = "000000";
            this.txtKod.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtKod.Properties.MaxLength = 6;
            this.txtKod.Size = new Size(200, 40);
            this.txtKod.Properties.Appearance.Font = new Font("Tahoma", 14F, FontStyle.Bold);
            this.txtKod.Properties.Appearance.Options.UseTextOptions = true;
            this.txtKod.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // btnDogrula (Fırat Standardı: btn prefix, Tahoma font)
            this.btnDogrula.Location = new Point(100, 190);
            this.btnDogrula.Name = "btnDogrula";
            this.btnDogrula.Size = new Size(200, 45);
            this.btnDogrula.Text = "DOĞRULA";
            this.btnDogrula.Appearance.Font = new Font("Tahoma", 11F, FontStyle.Bold);
            this.btnDogrula.Appearance.BackColor = Color.FromArgb(212, 175, 55);
            this.btnDogrula.Appearance.ForeColor = Color.FromArgb(26, 54, 93);
            this.btnDogrula.Appearance.Options.UseBackColor = true;
            this.btnDogrula.Appearance.Options.UseForeColor = true;
            this.btnDogrula.Appearance.Options.UseFont = true;
            this.btnDogrula.Click += new System.EventHandler(this.btnDogrula_Click);
            
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

            ((ISupportInitialize)(this.txtKod.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
