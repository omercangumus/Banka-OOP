using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Müşteri formu - Designer dosyası
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class CustomerForm
    {
        private IContainer components = null;
        private LayoutControl layoutMusteri;
        private LayoutControlGroup Root;
        private TextEdit txtTcKimlikNo;
        private TextEdit txtAd;
        private TextEdit txtSoyad;
        private TextEdit txtTelefon;
        private TextEdit txtEposta;
        private MemoEdit memoAdres;
        private SimpleButton btnKaydet;
        private SimpleButton btnIptal;
        
        // Layout Items
        private LayoutControlItem layoutControlItem1;
        private LayoutControlItem layoutControlItem2;
        private LayoutControlItem layoutControlItem3;
        private LayoutControlItem layoutControlItem4;
        private LayoutControlItem layoutControlItem5;
        private LayoutControlItem layoutControlItem6;
        private LayoutControlItem layoutControlItem7;
        private LayoutControlItem layoutControlItem8;
        private EmptySpaceItem emptySpaceItem1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.layoutMusteri = new LayoutControl();
            this.txtTcKimlikNo = new TextEdit();
            this.txtAd = new TextEdit();
            this.txtSoyad = new TextEdit();
            this.txtTelefon = new TextEdit();
            this.txtEposta = new TextEdit();
            this.memoAdres = new MemoEdit();
            this.btnKaydet = new SimpleButton();
            this.btnIptal = new SimpleButton();
            this.Root = new LayoutControlGroup();
            this.layoutControlItem1 = new LayoutControlItem();
            this.layoutControlItem2 = new LayoutControlItem();
            this.layoutControlItem3 = new LayoutControlItem();
            this.layoutControlItem4 = new LayoutControlItem();
            this.layoutControlItem5 = new LayoutControlItem();
            this.layoutControlItem6 = new LayoutControlItem();
            this.layoutControlItem7 = new LayoutControlItem();
            this.layoutControlItem8 = new LayoutControlItem();
            this.emptySpaceItem1 = new EmptySpaceItem();

            ((ISupportInitialize)(this.layoutMusteri)).BeginInit();
            this.layoutMusteri.SuspendLayout();
            ((ISupportInitialize)(this.txtTcKimlikNo.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtAd.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtSoyad.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtTelefon.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtEposta.Properties)).BeginInit();
            ((ISupportInitialize)(this.memoAdres.Properties)).BeginInit();
            ((ISupportInitialize)(this.Root)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem8)).BeginInit();
            ((ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            this.SuspendLayout();

            // layoutMusteri
            this.layoutMusteri.Controls.Add(this.btnIptal);
            this.layoutMusteri.Controls.Add(this.btnKaydet);
            this.layoutMusteri.Controls.Add(this.memoAdres);
            this.layoutMusteri.Controls.Add(this.txtEposta);
            this.layoutMusteri.Controls.Add(this.txtTelefon);
            this.layoutMusteri.Controls.Add(this.txtSoyad);
            this.layoutMusteri.Controls.Add(this.txtAd);
            this.layoutMusteri.Controls.Add(this.txtTcKimlikNo);
            this.layoutMusteri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutMusteri.Name = "layoutMusteri";
            this.layoutMusteri.Root = this.Root;

            // txtTcKimlikNo
            this.txtTcKimlikNo.Name = "txtTcKimlikNo";
            this.txtTcKimlikNo.Properties.Mask.EditMask = "d";
            this.txtTcKimlikNo.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.txtTcKimlikNo.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);
            
            // txtAd
            this.txtAd.Name = "txtAd";
            this.txtAd.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);

            // txtSoyad
            this.txtSoyad.Name = "txtSoyad";
            this.txtSoyad.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);

            // txtTelefon
            this.txtTelefon.Name = "txtTelefon";
            this.txtTelefon.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);

            // txtEposta
            this.txtEposta.Name = "txtEposta";
            this.txtEposta.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);

            // memoAdres
            this.memoAdres.Name = "memoAdres";
            this.memoAdres.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);

            // btnKaydet
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);

            // btnIptal
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Text = "İptal";
            this.btnIptal.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.btnIptal.Click += (s, e) => this.Close();

            // Root Group
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem6,
            this.layoutControlItem7,
            this.layoutControlItem8,
            this.emptySpaceItem1});
            this.Root.Name = "Root";

            // Layout Items
            this.layoutControlItem1.Control = this.txtTcKimlikNo;
            this.layoutControlItem1.Text = "TC Kimlik No";
            
            this.layoutControlItem2.Control = this.txtAd;
            this.layoutControlItem2.Text = "Ad";
            
            this.layoutControlItem3.Control = this.txtSoyad;
            this.layoutControlItem3.Text = "Soyad";
            
            this.layoutControlItem4.Control = this.txtTelefon;
            this.layoutControlItem4.Text = "Telefon";
            
            this.layoutControlItem5.Control = this.txtEposta;
            this.layoutControlItem5.Text = "E-Posta";
            
            this.layoutControlItem6.Control = this.memoAdres;
            this.layoutControlItem6.Text = "Adres";
            
            this.layoutControlItem7.Control = this.btnKaydet;
            this.layoutControlItem7.TextVisible = false;
            
            this.layoutControlItem8.Control = this.btnIptal;
            this.layoutControlItem8.TextVisible = false;
            
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(0, 50);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            // CustomerForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 650);
            this.Controls.Add(this.layoutMusteri);
            this.Name = "CustomerForm";
            this.Text = "Müşteri Kartı";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            ((ISupportInitialize)(this.layoutMusteri)).EndInit();
            this.layoutMusteri.ResumeLayout(false);
            ((ISupportInitialize)(this.txtTcKimlikNo.Properties)).EndInit();
            ((ISupportInitialize)(this.txtAd.Properties)).EndInit();
            ((ISupportInitialize)(this.txtSoyad.Properties)).EndInit();
            ((ISupportInitialize)(this.txtTelefon.Properties)).EndInit();
            ((ISupportInitialize)(this.txtEposta.Properties)).EndInit();
            ((ISupportInitialize)(this.memoAdres.Properties)).EndInit();
            ((ISupportInitialize)(this.Root)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem8)).EndInit();
            ((ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
