using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Kayıt formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class RegisterForm
    {
        private IContainer components = null;
        private Panel pnlGradient;
        private LabelControl lblBaslik;
        private TextEdit txtKullaniciAdi;
        private TextEdit txtSifre;
        private TextEdit txtEposta;
        private TextEdit txtAdSoyad;
        private SimpleButton btnKayitOl;
        private HyperlinkLabelControl llblGirisYap;

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
            this.txtKullaniciAdi = new TextEdit();
            this.txtSifre = new TextEdit();
            this.txtEposta = new TextEdit();
            this.txtAdSoyad = new TextEdit();
            this.btnKayitOl = new SimpleButton();
            this.llblGirisYap = new HyperlinkLabelControl();

            ((ISupportInitialize)(this.txtKullaniciAdi.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtSifre.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtEposta.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtAdSoyad.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlGradient
            this.pnlGradient.BackColor = Color.FromArgb(15, 23, 42);
            this.pnlGradient.Dock = DockStyle.Fill;
            this.pnlGradient.Paint += (s, e) => {
                using (System.Drawing.Drawing2D.LinearGradientBrush gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0), new Point(0, this.pnlGradient.Height),
                    Color.FromArgb(15, 23, 42),
                    Color.FromArgb(30, 41, 59)))
                {
                    e.Graphics.FillRectangle(gradientBrush, this.pnlGradient.ClientRectangle);
                }
            };
            this.pnlGradient.Controls.Add(this.llblGirisYap);
            this.pnlGradient.Controls.Add(this.btnKayitOl);
            this.pnlGradient.Controls.Add(this.txtSifre);
            this.pnlGradient.Controls.Add(this.txtEposta);
            this.pnlGradient.Controls.Add(this.txtAdSoyad);
            this.pnlGradient.Controls.Add(this.txtKullaniciAdi);
            this.pnlGradient.Controls.Add(this.lblBaslik);

            // lblBaslik (Fırat Standardı: Tahoma font)
            this.lblBaslik.Location = new Point(150, 50);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.Text = "YENİ HESAP OLUŞTUR";
            this.lblBaslik.Appearance.Font = new Font("Tahoma", 20F, FontStyle.Bold);
            this.lblBaslik.Appearance.ForeColor = Color.FromArgb(212, 175, 55);
            this.lblBaslik.Size = new Size(520, 45);
            this.lblBaslik.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // txtKullaniciAdi (Fırat Standardı: txt prefix, Tahoma font)
            this.txtKullaniciAdi.Location = new Point(200, 130);
            this.txtKullaniciAdi.Name = "txtKullaniciAdi";
            this.txtKullaniciAdi.Properties.NullValuePrompt = "Kullanıcı Adı";
            this.txtKullaniciAdi.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtKullaniciAdi.Size = new Size(420, 56);
            this.txtKullaniciAdi.Properties.Appearance.Font = new Font("Tahoma", 12F);
            this.txtKullaniciAdi.Properties.Appearance.BackColor = Color.White;
            this.txtKullaniciAdi.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtKullaniciAdi.Properties.Appearance.Options.UseBackColor = true;
            this.txtKullaniciAdi.Properties.Appearance.Options.UseForeColor = true;
            this.txtKullaniciAdi.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtKullaniciAdi.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtKullaniciAdi.Properties.Appearance.Options.UseBorderColor = true;
            this.txtKullaniciAdi.Properties.AutoHeight = false;

            // txtAdSoyad (Fırat Standardı: txt prefix, Tahoma font)
            this.txtAdSoyad.Location = new Point(200, 205);
            this.txtAdSoyad.Name = "txtAdSoyad";
            this.txtAdSoyad.Properties.NullValuePrompt = "Ad Soyad";
            this.txtAdSoyad.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtAdSoyad.Size = new Size(420, 56);
            this.txtAdSoyad.Properties.Appearance.Font = new Font("Tahoma", 12F);
            this.txtAdSoyad.Properties.Appearance.BackColor = Color.White;
            this.txtAdSoyad.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtAdSoyad.Properties.Appearance.Options.UseBackColor = true;
            this.txtAdSoyad.Properties.Appearance.Options.UseForeColor = true;
            this.txtAdSoyad.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtAdSoyad.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtAdSoyad.Properties.Appearance.Options.UseBorderColor = true;
            this.txtAdSoyad.Properties.AutoHeight = false;

            // txtEposta (Fırat Standardı: txt prefix, Tahoma font)
            this.txtEposta.Location = new Point(200, 280);
            this.txtEposta.Name = "txtEposta";
            this.txtEposta.Properties.NullValuePrompt = "E-Posta Adresi";
            this.txtEposta.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtEposta.Size = new Size(420, 56);
            this.txtEposta.Properties.Appearance.Font = new Font("Tahoma", 12F);
            this.txtEposta.Properties.Appearance.BackColor = Color.White;
            this.txtEposta.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtEposta.Properties.Appearance.Options.UseBackColor = true;
            this.txtEposta.Properties.Appearance.Options.UseForeColor = true;
            this.txtEposta.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtEposta.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtEposta.Properties.Appearance.Options.UseBorderColor = true;
            this.txtEposta.Properties.AutoHeight = false;

            // txtSifre (Fırat Standardı: txt prefix, Tahoma font)
            this.txtSifre.Location = new Point(200, 355);
            this.txtSifre.Name = "txtSifre";
            this.txtSifre.Properties.NullValuePrompt = "Şifre";
            this.txtSifre.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtSifre.Properties.UseSystemPasswordChar = true;
            this.txtSifre.Size = new Size(420, 56);
            this.txtSifre.Properties.Appearance.Font = new Font("Tahoma", 12F);
            this.txtSifre.Properties.Appearance.BackColor = Color.White;
            this.txtSifre.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtSifre.Properties.Appearance.Options.UseBackColor = true;
            this.txtSifre.Properties.Appearance.Options.UseForeColor = true;
            this.txtSifre.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtSifre.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtSifre.Properties.Appearance.Options.UseBorderColor = true;
            this.txtSifre.Properties.AutoHeight = false;

            // btnKayitOl (Fırat Standardı: btn prefix, Tahoma font)
            this.btnKayitOl.Location = new Point(200, 435);
            this.btnKayitOl.Name = "btnKayitOl";
            this.btnKayitOl.Size = new Size(420, 60);
            this.btnKayitOl.Text = "KAYIT OL";
            this.btnKayitOl.Appearance.Font = new Font("Tahoma", 14F, FontStyle.Bold);
            this.btnKayitOl.Appearance.BackColor = Color.FromArgb(212, 175, 55);
            this.btnKayitOl.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.btnKayitOl.Appearance.Options.UseBackColor = true;
            this.btnKayitOl.Appearance.Options.UseForeColor = true;
            this.btnKayitOl.Appearance.Options.UseFont = true;
            this.btnKayitOl.AppearanceHovered.BackColor = Color.FromArgb(235, 195, 75);
            this.btnKayitOl.AppearanceHovered.Options.UseBackColor = true;
            this.btnKayitOl.AppearancePressed.BackColor = Color.FromArgb(189, 155, 45);
            this.btnKayitOl.AppearancePressed.Options.UseBackColor = true;
            this.btnKayitOl.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.btnKayitOl.Click += new System.EventHandler(this.btnKayitOl_Click);

            // llblGirisYap (Fırat Standardı: llbl prefix, Tahoma font)
            this.llblGirisYap.Location = new Point(235, 515);
            this.llblGirisYap.Name = "llblGirisYap";
            this.llblGirisYap.Text = "<href>Zaten hesabım var? Giriş Yap</href>";
            this.llblGirisYap.Appearance.ForeColor = Color.FromArgb(212, 175, 55);
            this.llblGirisYap.Appearance.Font = new Font("Tahoma", 10F);
            this.llblGirisYap.Size = new Size(350, 25);
            this.llblGirisYap.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.llblGirisYap.Click += new System.EventHandler(this.llblGirisYap_Click);
            
            // RegisterForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(820, 600);
            this.Controls.Add(this.pnlGradient);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "RegisterForm";
            this.Text = "NovaBank - Yeni Hesap";
            this.StartPosition = FormStartPosition.CenterScreen;

            ((ISupportInitialize)(this.txtKullaniciAdi.Properties)).EndInit();
            ((ISupportInitialize)(this.txtSifre.Properties)).EndInit();
            ((ISupportInitialize)(this.txtEposta.Properties)).EndInit();
            ((ISupportInitialize)(this.txtAdSoyad.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
