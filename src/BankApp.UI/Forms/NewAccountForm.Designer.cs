using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;

namespace BankApp.UI.Forms
{
    public partial class NewAccountForm
    {
        private IContainer components = null;
        private LayoutControl layoutControl1;
        private LayoutControlGroup Root;
        private ComboBoxEdit cmbCurrency;
        private CalcEdit txtInitialDeposit;
        private SimpleButton btnCreate;
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
            this.cmbCurrency = new ComboBoxEdit();
            this.txtInitialDeposit = new CalcEdit();
            this.btnCreate = new SimpleButton();
            this.Root = new LayoutControlGroup();
            this.layoutControlItem1 = new LayoutControlItem();
            this.layoutControlItem2 = new LayoutControlItem();
            this.layoutControlItem3 = new LayoutControlItem();

            ((ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((ISupportInitialize)(this.cmbCurrency.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtInitialDeposit.Properties)).BeginInit();
            ((ISupportInitialize)(this.Root)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            this.SuspendLayout();

            // layoutControl1
            this.layoutControl1.Controls.Add(this.btnCreate);
            this.layoutControl1.Controls.Add(this.txtInitialDeposit);
            this.layoutControl1.Controls.Add(this.cmbCurrency);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.Root;

            // cmbCurrency
            this.cmbCurrency.Location = new System.Drawing.Point(111, 12);
            this.cmbCurrency.Name = "cmbCurrency";
            this.cmbCurrency.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbCurrency.Properties.Items.AddRange(new object[] {
            "TRY",
            "USD",
            "EUR"});
            this.cmbCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbCurrency.Size = new System.Drawing.Size(261, 20);
            
            // txtInitialDeposit
            this.txtInitialDeposit.Location = new System.Drawing.Point(111, 36);
            this.txtInitialDeposit.Name = "txtInitialDeposit";
            this.txtInitialDeposit.Size = new System.Drawing.Size(261, 20);

            // btnCreate
            this.btnCreate.Location = new System.Drawing.Point(12, 60);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(360, 22);
            this.btnCreate.Text = "Hesap Oluştur";
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);

            // Root
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3});
            this.Root.Name = "Root";

            // Layout Items
            this.layoutControlItem1.Control = this.cmbCurrency;
            this.layoutControlItem1.Text = "Para Birimi";
            
            this.layoutControlItem2.Control = this.txtInitialDeposit;
            this.layoutControlItem2.Text = "İlk Bakiye";
            
            this.layoutControlItem3.Control = this.btnCreate;
            this.layoutControlItem3.TextVisible = false;

            // NewAccountForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 350);
            this.Controls.Add(this.layoutControl1);
            this.Name = "NewAccountForm";
            this.Text = "Yeni Hesap Aç";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            ((ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((ISupportInitialize)(this.cmbCurrency.Properties)).EndInit();
            ((ISupportInitialize)(this.txtInitialDeposit.Properties)).EndInit();
            ((ISupportInitialize)(this.Root)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((ISupportInitialize)(this.layoutControlItem3)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
