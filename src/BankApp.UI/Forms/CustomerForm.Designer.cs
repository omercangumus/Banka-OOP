using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;

namespace BankApp.UI.Forms
{
    public partial class CustomerForm
    {
        private IContainer components = null;
        private LayoutControl layoutControl1;
        private LayoutControlGroup Root;
        private TextEdit txtIdentity;
        private TextEdit txtFirstName;
        private TextEdit txtLastName;
        private TextEdit txtPhone;
        private TextEdit txtEmail;
        private MemoEdit txtAddress;
        private SimpleButton btnSave;
        private SimpleButton btnCancel;
        
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
            this.layoutControl1 = new LayoutControl();
            this.txtIdentity = new TextEdit();
            this.txtFirstName = new TextEdit();
            this.txtLastName = new TextEdit();
            this.txtPhone = new TextEdit();
            this.txtEmail = new TextEdit();
            this.txtAddress = new MemoEdit();
            this.btnSave = new SimpleButton();
            this.btnCancel = new SimpleButton();
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

            ((ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((ISupportInitialize)(this.txtIdentity.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtFirstName.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtLastName.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtPhone.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtEmail.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtAddress.Properties)).BeginInit();
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

            // layoutControl1
            this.layoutControl1.Controls.Add(this.btnCancel);
            this.layoutControl1.Controls.Add(this.btnSave);
            this.layoutControl1.Controls.Add(this.txtAddress);
            this.layoutControl1.Controls.Add(this.txtEmail);
            this.layoutControl1.Controls.Add(this.txtPhone);
            this.layoutControl1.Controls.Add(this.txtLastName);
            this.layoutControl1.Controls.Add(this.txtFirstName);
            this.layoutControl1.Controls.Add(this.txtIdentity);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.Root;

            // txtIdentity
            this.txtIdentity.Name = "txtIdentity";
            this.txtIdentity.Properties.Mask.EditMask = "d";
            this.txtIdentity.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            
            // Other TextEdits
            this.txtFirstName.Name = "txtFirstName";
            this.txtLastName.Name = "txtLastName";
            this.txtPhone.Name = "txtPhone";
            this.txtEmail.Name = "txtEmail";
            this.txtAddress.Name = "txtAddress";

            // Buttons
            this.btnSave.Name = "btnSave";
            this.btnSave.Text = "Kaydet";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Text = "İptal";
            this.btnCancel.Click += (s, e) => this.Close();

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
            this.layoutControlItem1.Control = this.txtIdentity;
            this.layoutControlItem1.Text = "TC Kimlik No";
            
            this.layoutControlItem2.Control = this.txtFirstName;
            this.layoutControlItem2.Text = "Ad";
            
            this.layoutControlItem3.Control = this.txtLastName;
            this.layoutControlItem3.Text = "Soyad";
            
            this.layoutControlItem4.Control = this.txtPhone;
            this.layoutControlItem4.Text = "Telefon";
            
            this.layoutControlItem5.Control = this.txtEmail;
            this.layoutControlItem5.Text = "E-Posta";
            
            this.layoutControlItem6.Control = this.txtAddress;
            this.layoutControlItem6.Text = "Adres";
            
            this.layoutControlItem7.Control = this.btnSave;
            this.layoutControlItem7.TextVisible = false;
            
            this.layoutControlItem8.Control = this.btnCancel;
            this.layoutControlItem8.TextVisible = false;
            
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(0, 50);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            // CustomerForm - PROFESYONEL BOYUTLAR
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 650);
            this.Controls.Add(this.layoutControl1);
            this.Name = "CustomerForm";
            this.Text = "Müşteri Kartı";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            ((ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((ISupportInitialize)(this.txtIdentity.Properties)).EndInit();
            ((ISupportInitialize)(this.txtFirstName.Properties)).EndInit();
            ((ISupportInitialize)(this.txtLastName.Properties)).EndInit();
            ((ISupportInitialize)(this.txtPhone.Properties)).EndInit();
            ((ISupportInitialize)(this.txtEmail.Properties)).EndInit();
            ((ISupportInitialize)(this.txtAddress.Properties)).EndInit();
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
