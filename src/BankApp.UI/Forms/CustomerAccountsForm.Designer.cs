using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Müşteri hesapları formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class CustomerAccountsForm
    {
        private IContainer components = null;
        private GroupControl grpAna;
        private GridControl grdHesaplar;
        private GridView grdwHesaplar;
        private SimpleButton btnYeniHesap;
        private SimpleButton btnHareketler;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpAna = new GroupControl();
            this.grdHesaplar = new GridControl();
            this.grdwHesaplar = new GridView();
            this.btnYeniHesap = new SimpleButton();
            this.btnHareketler = new SimpleButton();

            ((ISupportInitialize)(this.grpAna)).BeginInit();
            this.grpAna.SuspendLayout();
            ((ISupportInitialize)(this.grdHesaplar)).BeginInit();
            ((ISupportInitialize)(this.grdwHesaplar)).BeginInit();
            this.SuspendLayout();

            // grpAna (Fırat Standardı: grp prefix)
            this.grpAna.Controls.Add(this.btnHareketler);
            this.grpAna.Controls.Add(this.btnYeniHesap);
            this.grpAna.Controls.Add(this.grdHesaplar);
            this.grpAna.Dock = DockStyle.Fill;
            this.grpAna.Name = "grpAna";
            this.grpAna.Text = "HESAPLAR";
            this.grpAna.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);

            // grdHesaplar (Fırat Standardı: grd prefix)
            this.grdHesaplar.Dock = DockStyle.Top;
            this.grdHesaplar.Height = 350;
            this.grdHesaplar.MainView = this.grdwHesaplar;
            this.grdHesaplar.Name = "grdHesaplar";
            this.grdHesaplar.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.grdwHesaplar});

            // grdwHesaplar (Fırat Standardı: grdw prefix)
            this.grdwHesaplar.GridControl = this.grdHesaplar;
            this.grdwHesaplar.Name = "grdwHesaplar";
            this.grdwHesaplar.OptionsBehavior.Editable = false;
            this.grdwHesaplar.OptionsView.ShowGroupPanel = false;
            this.grdwHesaplar.Appearance.Row.Font = new Font("Tahoma", 8.25F);
            this.grdwHesaplar.Appearance.HeaderPanel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);

            // btnYeniHesap (Fırat Standardı: btn prefix)
            this.btnYeniHesap.Location = new Point(20, 370);
            this.btnYeniHesap.Name = "btnYeniHesap";
            this.btnYeniHesap.Size = new Size(150, 40);
            this.btnYeniHesap.Text = "YENİ HESAP AÇ";
            this.btnYeniHesap.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnYeniHesap.Click += new System.EventHandler(this.btnYeniHesap_Click);

            // btnHareketler (Fırat Standardı: btn prefix)
            this.btnHareketler.Location = new Point(190, 370);
            this.btnHareketler.Name = "btnHareketler";
            this.btnHareketler.Size = new Size(150, 40);
            this.btnHareketler.Text = "HAREKETLER";
            this.btnHareketler.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnHareketler.Click += new System.EventHandler(this.btnHareketler_Click);

            // CustomerAccountsForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(600, 430);
            this.Controls.Add(this.grpAna);
            this.Name = "CustomerAccountsForm";
            this.Text = "Müşteri Hesapları";
            this.StartPosition = FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.grpAna)).EndInit();
            this.grpAna.ResumeLayout(false);
            ((ISupportInitialize)(this.grdHesaplar)).EndInit();
            ((ISupportInitialize)(this.grdwHesaplar)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
