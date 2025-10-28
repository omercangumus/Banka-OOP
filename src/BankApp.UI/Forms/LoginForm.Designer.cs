using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Giriş formu - Designer dosyası
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class LoginForm
    {
        private IContainer components = null;
        private Panel pnlArkaPlan;
        private PictureBox pctLogo;
        private LabelControl lblBaslik;
        private LabelControl lblAltBaslik;
        private TextEdit txtKullaniciAdi;
        private TextEdit txtSifre;
        private SimpleButton btnGiris;
        private SimpleButton btnKayitOl;
        private HyperlinkLabelControl llblSifremiUnuttum;
        private LabelControl lblDurum;

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
            this.pnlArkaPlan = new Panel();
            this.pctLogo = new PictureBox();
            this.lblBaslik = new LabelControl();
            this.lblAltBaslik = new LabelControl();
            this.txtKullaniciAdi = new TextEdit();
            this.txtSifre = new TextEdit();
            this.btnGiris = new SimpleButton();
            this.btnKayitOl = new SimpleButton();
            this.llblSifremiUnuttum = new HyperlinkLabelControl();
            this.lblDurum = new LabelControl();

            ((ISupportInitialize)(this.pctLogo)).BeginInit();
            ((ISupportInitialize)(this.txtKullaniciAdi.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtSifre.Properties)).BeginInit();
            this.SuspendLayout();

            // pnlArkaPlan - Ana kapsayıcı gradient efekti ile
            this.pnlArkaPlan.BackColor = Color.FromArgb(15, 23, 42);
            this.pnlArkaPlan.Dock = DockStyle.Fill;
            // pnlArkaPlan - Ana kapsayıcı 
            this.pnlArkaPlan.BackColor = Color.FromArgb(15, 23, 42);
            this.pnlArkaPlan.Dock = DockStyle.Fill;
            // Gradient paint removed for stability
            // this.pnlArkaPlan.Paint += ...
            this.pnlArkaPlan.Controls.Add(this.lblDurum);
            this.pnlArkaPlan.Controls.Add(this.llblSifremiUnuttum);
            this.pnlArkaPlan.Controls.Add(this.btnKayitOl);
            this.pnlArkaPlan.Controls.Add(this.btnGiris);
            this.pnlArkaPlan.Controls.Add(this.txtSifre);
            this.pnlArkaPlan.Controls.Add(this.txtKullaniciAdi);
            this.pnlArkaPlan.Controls.Add(this.lblAltBaslik);
            this.pnlArkaPlan.Controls.Add(this.lblBaslik);
            this.pnlArkaPlan.Controls.Add(this.pctLogo);

            // pctLogo
            this.pctLogo.Location = new Point(325, 60);
            this.pctLogo.Name = "pctLogo";
            this.pctLogo.Size = new Size(180, 180);
            this.pctLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.pctLogo.BackColor = Color.Transparent;
            try
            {
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "..", "..", "..", "Resources", "novabank_logo.png");
                if (System.IO.File.Exists(logoPath))
                    this.pctLogo.Image = Image.FromFile(logoPath);
            }
            catch { }

            // lblBaslik
            this.lblBaslik.Location = new Point(200, 260);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.Text = "NOVABANK";
            this.lblBaslik.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.lblBaslik.Appearance.ForeColor = Color.FromArgb(212, 175, 55);
            this.lblBaslik.AutoSizeMode = LabelAutoSizeMode.None;
            this.lblBaslik.Size = new Size(420, 60);
            this.lblBaslik.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // lblAltBaslik
            this.lblAltBaslik.Location = new Point(180, 330);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.Text = "Güvenli Kurumsal Bankacılık";
            this.lblAltBaslik.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Italic);
            this.lblAltBaslik.Appearance.ForeColor = Color.FromArgb(200, 200, 200);
            this.lblAltBaslik.Size = new Size(460, 25);
            this.lblAltBaslik.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // txtKullaniciAdi
            this.txtKullaniciAdi.Location = new Point(200, 390);
            this.txtKullaniciAdi.Name = "txtKullaniciAdi";
            this.txtKullaniciAdi.Properties.NullValuePrompt = "Kullanıcı Adı";
            this.txtKullaniciAdi.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtKullaniciAdi.Size = new Size(420, 56);
            this.txtKullaniciAdi.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.txtKullaniciAdi.Properties.Appearance.BackColor = Color.White;
            this.txtKullaniciAdi.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtKullaniciAdi.Properties.Appearance.Options.UseBackColor = true;
            this.txtKullaniciAdi.Properties.Appearance.Options.UseForeColor = true;
            this.txtKullaniciAdi.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtKullaniciAdi.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtKullaniciAdi.Properties.Appearance.Options.UseBorderColor = true;
            this.txtKullaniciAdi.Properties.AutoHeight = false;

            // txtSifre
            this.txtSifre.Location = new Point(200, 465);
            this.txtSifre.Name = "txtSifre";
            this.txtSifre.Properties.NullValuePrompt = "Şifre";
            this.txtSifre.Properties.NullValuePromptShowForEmptyValue = true;
            this.txtSifre.Properties.UseSystemPasswordChar = true;
            this.txtSifre.Size = new Size(420, 56);
            this.txtSifre.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.txtSifre.Properties.Appearance.BackColor = Color.White;
            this.txtSifre.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtSifre.Properties.Appearance.Options.UseBackColor = true;
            this.txtSifre.Properties.Appearance.Options.UseForeColor = true;
            this.txtSifre.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtSifre.Properties.Appearance.BorderColor = Color.FromArgb(212, 175, 55);
            this.txtSifre.Properties.Appearance.Options.UseBorderColor = true;
            this.txtSifre.Properties.AutoHeight = false;

            // btnGiris
            this.btnGiris.Location = new Point(200, 545);
            this.btnGiris.Name = "btnGiris";
            this.btnGiris.Size = new Size(420, 60);
            this.btnGiris.Text = "GİRİŞ YAP";
            this.btnGiris.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnGiris.Appearance.BackColor = Color.FromArgb(212, 175, 55);
            this.btnGiris.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            this.btnGiris.Appearance.Options.UseBackColor = true;
            this.btnGiris.Appearance.Options.UseForeColor = true;
            this.btnGiris.Appearance.Options.UseFont = true;
            this.btnGiris.AppearanceHovered.BackColor = Color.FromArgb(235, 195, 75);
            this.btnGiris.AppearanceHovered.Options.UseBackColor = true;
            this.btnGiris.AppearancePressed.BackColor = Color.FromArgb(189, 155, 45);
            this.btnGiris.AppearancePressed.Options.UseBackColor = true;
            this.btnGiris.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.btnGiris.Click += new System.EventHandler(this.btnGiris_Click);

            // btnKayitOl
            this.btnKayitOl.Location = new Point(200, 625);
            this.btnKayitOl.Name = "btnKayitOl";
            this.btnKayitOl.Size = new Size(420, 56);
            this.btnKayitOl.Text = "YENİ HESAP OLUŞTUR";
            this.btnKayitOl.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnKayitOl.Appearance.BackColor = Color.FromArgb(30, 41, 59);
            this.btnKayitOl.Appearance.ForeColor = Color.White;
            this.btnKayitOl.Appearance.Options.UseBackColor = true;
            this.btnKayitOl.Appearance.Options.UseForeColor = true;
            this.btnKayitOl.Appearance.Options.UseFont = true;
            this.btnKayitOl.AppearanceHovered.BackColor = Color.FromArgb(51, 65, 85);
            this.btnKayitOl.AppearanceHovered.Options.UseBackColor = true;
            this.btnKayitOl.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.btnKayitOl.Click += new System.EventHandler(this.btnKayitOl_Click);

            // llblSifremiUnuttum
            this.llblSifremiUnuttum.Location = new Point(295, 695);
            this.llblSifremiUnuttum.Name = "llblSifremiUnuttum";
            this.llblSifremiUnuttum.Text = "<href>Şifremi Unuttum</href>";
            this.llblSifremiUnuttum.Appearance.ForeColor = Color.FromArgb(212, 175, 55);
            this.llblSifremiUnuttum.Appearance.Font = new Font("Tahoma", 8.25F);
            this.llblSifremiUnuttum.Size = new Size(230, 25);
            this.llblSifremiUnuttum.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.llblSifremiUnuttum.Click += new System.EventHandler(this.llblSifremiUnuttum_Click);

            // lblDurum
            this.lblDurum.Location = new Point(275, 730);
            this.lblDurum.Name = "lblDurum";
            this.lblDurum.Text = "v1.0 - Güvenli Bağlantı | SSL Şifreli";
            this.lblDurum.Appearance.ForeColor = Color.FromArgb(150, 150, 150);
            this.lblDurum.Appearance.Font = new Font("Tahoma", 8.25F);
            this.lblDurum.Size = new Size(270, 20);
            this.lblDurum.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            
            // LoginForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(820, 780);
            this.Controls.Add(this.pnlArkaPlan);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "LoginForm";
            this.Text = "NovaBank - Güvenli Giriş";
            this.StartPosition = FormStartPosition.CenterScreen;

            ((ISupportInitialize)(this.pctLogo)).EndInit();
            ((ISupportInitialize)(this.txtKullaniciAdi.Properties)).EndInit();
            ((ISupportInitialize)(this.txtSifre.Properties)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
