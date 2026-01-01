using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Şifre sıfırlama formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class ForgotPasswordForm
    {
        private IContainer components = null;
        private Panel pnlGradient;
        private LabelControl lblBaslik;
        private LabelControl lblAciklama;
        private TextEdit txtEposta;
        private SimpleButton btnKodGonder;
        private TextEdit txtKod;
        private SimpleButton btnDogrula;
        private TextEdit txtYeniSifre;
        private SimpleButton btnSifreDegistir;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlGradient = new Panel();
            this.lblBaslik = new LabelControl();
            this.lblAciklama = new LabelControl();
            this.txtEposta = new TextEdit();
            this.btnKodGonder = new SimpleButton();
            this.txtKod = new TextEdit();
            this.btnDogrula = new SimpleButton();
            this.txtYeniSifre = new TextEdit();
            this.btnSifreDegistir = new SimpleButton();

            ((ISupportInitialize)(this.txtEposta.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtKod.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtYeniSifre.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlGradient
            this.pnlGradient.BackColor = Color.FromArgb(26, 54, 93);
            this.pnlGradient.Dock = DockStyle.Fill;
            this.pnlGradient.Controls.Add(this.btnSifreDegistir);
            this.pnlGradient.Controls.Add(this.txtYeniSifre);
            this.pnlGradient.Controls.Add(this.btnDogrula);
            this.pnlGradient.Controls.Add(this.txtKod);
            this.pnlGradient.Controls.Add(this.btnKodGonder);
            this.pnlGradient.Controls.Add(this.txtEposta);
            this.pnlGradient.Controls.Add(this.lblAciklama);
            this.pnlGradient.Controls.Add(this.lblBaslik);

            // lblBaslik (Fırat Standardı: Tahoma font)
            this.lblBaslik.Location = new Point(130, 20);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.Text = "ŞİFRE SIFIRLAMA";
            this.lblBaslik.Appearance.Font = new Font("Tahoma", 16F, FontStyle.Bold);
            this.lblBaslik.Appearance.ForeColor = Color.FromArgb(212, 175, 55);

            // lblAciklama (Fırat Standardı: Tahoma font)
            this.lblAciklama.Location = new Point(80, 65);
            this.lblAciklama.Name = "lblAciklama";
            this.lblAciklama.Text = "Lütfen kayıtlı e-posta adresinizi giriniz.";
            this.lblAciklama.Appearance.Font = new Font("Tahoma", 9F);
            this.lblAciklama.Appearance.ForeColor = Color.White;

            // txtEposta (Fırat Standardı: txt prefix, Tahoma font)
            this.txtEposta.Location = new Point(80, 100);
            this.txtEposta.Name = "txtEposta";
            this.txtEposta.Properties.NullValuePrompt = "E-Posta Adresi";
            this.txtEposta.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtEposta.Size = new Size(250, 34);
            this.txtEposta.Properties.Appearance.Font = new Font("Tahoma", 10F);

            // btnKodGonder (Fırat Standardı: btn prefix, Tahoma font)
            this.btnKodGonder.Location = new Point(340, 100);
            this.btnKodGonder.Name = "btnKodGonder";
            this.btnKodGonder.Size = new Size(100, 34);
            this.btnKodGonder.Text = "KOD GÖNDER";
            this.btnKodGonder.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnKodGonder.Appearance.BackColor = Color.FromArgb(44, 82, 130);
            this.btnKodGonder.Appearance.ForeColor = Color.White;
            this.btnKodGonder.Appearance.Options.UseBackColor = true;
            this.btnKodGonder.Appearance.Options.UseForeColor = true;
            this.btnKodGonder.Click += new System.EventHandler(this.btnKodGonder_Click);

            // txtKod (Fırat Standardı: txt prefix, Tahoma font)
            this.txtKod.Location = new Point(80, 150);
            this.txtKod.Name = "txtKod";
            this.txtKod.Properties.NullValuePrompt = "Doğrulama Kodu";
            this.txtKod.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtKod.Size = new Size(150, 34);
            this.txtKod.Properties.Appearance.Font = new Font("Tahoma", 10F);
            this.txtKod.Enabled = false;

            // btnDogrula (Fırat Standardı: btn prefix, Tahoma font)
            this.btnDogrula.Location = new Point(240, 150);
            this.btnDogrula.Name = "btnDogrula";
            this.btnDogrula.Size = new Size(100, 34);
            this.btnDogrula.Text = "DOĞRULA";
            this.btnDogrula.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnDogrula.Appearance.BackColor = Color.FromArgb(44, 82, 130);
            this.btnDogrula.Appearance.ForeColor = Color.White;
            this.btnDogrula.Appearance.Options.UseBackColor = true;
            this.btnDogrula.Appearance.Options.UseForeColor = true;
            this.btnDogrula.Enabled = false;
            this.btnDogrula.Click += new System.EventHandler(this.btnDogrula_Click);

            // txtYeniSifre (Fırat Standardı: txt prefix, Tahoma font)
            this.txtYeniSifre.Location = new Point(80, 200);
            this.txtYeniSifre.Name = "txtYeniSifre";
            this.txtYeniSifre.Properties.NullValuePrompt = "Yeni Şifre";
            this.txtYeniSifre.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtYeniSifre.Properties.UseSystemPasswordChar = true;
            this.txtYeniSifre.Size = new Size(260, 34);
            this.txtYeniSifre.Properties.Appearance.Font = new Font("Tahoma", 10F);
            this.txtYeniSifre.Enabled = false;

            // btnSifreDegistir (Fırat Standardı: btn prefix, Tahoma font)
            this.btnSifreDegistir.Location = new Point(80, 250);
            this.btnSifreDegistir.Name = "btnSifreDegistir";
            this.btnSifreDegistir.Size = new Size(260, 45);
            this.btnSifreDegistir.Text = "ŞİFREYİ DEĞİŞTİR";
            this.btnSifreDegistir.Appearance.Font = new Font("Tahoma", 11F, FontStyle.Bold);
            this.btnSifreDegistir.Appearance.BackColor = Color.FromArgb(212, 175, 55);
            this.btnSifreDegistir.Appearance.ForeColor = Color.FromArgb(26, 54, 93);
            this.btnSifreDegistir.Appearance.Options.UseBackColor = true;
            this.btnSifreDegistir.Appearance.Options.UseForeColor = true;
            this.btnSifreDegistir.Appearance.Options.UseFont = true;
            this.btnSifreDegistir.Enabled = false;
            this.btnSifreDegistir.Click += new System.EventHandler(this.btnSifreDegistir_Click);

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

            ((ISupportInitialize)(this.txtEposta.Properties)).EndInit();
            ((ISupportInitialize)(this.txtKod.Properties)).EndInit();
            ((ISupportInitialize)(this.txtYeniSifre.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
