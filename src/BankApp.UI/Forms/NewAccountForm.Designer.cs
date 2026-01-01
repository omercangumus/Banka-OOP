using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Yeni hesap açma formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class NewAccountForm
    {
        private IContainer components = null;
        private LayoutControl layoutControl1;
        private LayoutControlGroup Root;
        private ComboBoxEdit cmbParaBirimi;
        private CalcEdit calcIlkPara;
        private SimpleButton btnOlustur;
        private LayoutControlItem layoutControlItem1;
        private LayoutControlItem layoutControlItem2;
        private LayoutControlItem layoutControlItem3;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.layoutControl1 = new LayoutControl();
            this.cmbParaBirimi = new ComboBoxEdit();
            this.calcIlkPara = new CalcEdit();
            this.btnOlustur = new SimpleButton();
            this.Root = new LayoutControlGroup();
            this.layoutControlItem1 = new LayoutControlItem();
            this.layoutControlItem2 = new LayoutControlItem();
            this.layoutControlItem3 = new LayoutControlItem();

            ((ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((ISupportInitialize)(this.cmbParaBirimi.Properties)).BeginInit();
            ((ISupportInitialize)(this.calcIlkPara.Properties)).BeginInit();
            ((ISupportInitialize)(this.Root)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            this.SuspendLayout();

            // layoutControl1
            this.layoutControl1.Controls.Add(this.btnOlustur);
            this.layoutControl1.Controls.Add(this.calcIlkPara);
            this.layoutControl1.Controls.Add(this.cmbParaBirimi);
            this.layoutControl1.Dock = DockStyle.Fill;
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.Root;

            // cmbParaBirimi (Fırat Standardı: cmb prefix)
            this.cmbParaBirimi.Location = new Point(111, 12);
            this.cmbParaBirimi.Name = "cmbParaBirimi";
            this.cmbParaBirimi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbParaBirimi.Properties.Items.AddRange(new object[] {
            "TRY",
            "USD",
            "EUR"});
            this.cmbParaBirimi.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbParaBirimi.Properties.Appearance.Font = new Font("Tahoma", 10F);
            this.cmbParaBirimi.Size = new Size(261, 22);

            // calcIlkPara (Fırat Standardı: calc prefix)
            this.calcIlkPara.Location = new Point(111, 36);
            this.calcIlkPara.Name = "calcIlkPara";
            this.calcIlkPara.Properties.Appearance.Font = new Font("Tahoma", 10F);
            this.calcIlkPara.Size = new Size(261, 22);

            // btnOlustur (Fırat Standardı: btn prefix)
            this.btnOlustur.Location = new Point(12, 60);
            this.btnOlustur.Name = "btnOlustur";
            this.btnOlustur.Size = new Size(360, 40);
            this.btnOlustur.Text = "HESAP OLUŞTUR";
            this.btnOlustur.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.btnOlustur.Click += new System.EventHandler(this.btnOlustur_Click);

            // Root
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3});
            this.Root.Name = "Root";

            // Layout Items
            this.layoutControlItem1.Control = this.cmbParaBirimi;
            this.layoutControlItem1.Text = "Para Birimi:";
            this.layoutControlItem1.AppearanceItemCaption.Font = new Font("Tahoma", 10F);
            
            this.layoutControlItem2.Control = this.calcIlkPara;
            this.layoutControlItem2.Text = "İlk Bakiye:";
            this.layoutControlItem2.AppearanceItemCaption.Font = new Font("Tahoma", 10F);
            
            this.layoutControlItem3.Control = this.btnOlustur;
            this.layoutControlItem3.TextVisible = false;

            // NewAccountForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(400, 250);
            this.Controls.Add(this.layoutControl1);
            this.Name = "NewAccountForm";
            this.Text = "Yeni Hesap Aç";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            ((ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((ISupportInitialize)(this.cmbParaBirimi.Properties)).EndInit();
            ((ISupportInitialize)(this.calcIlkPara.Properties)).EndInit();
            ((ISupportInitialize)(this.Root)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem3)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
