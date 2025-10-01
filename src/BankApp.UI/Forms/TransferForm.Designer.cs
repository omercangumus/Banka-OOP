using System.ComponentModel;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    partial class TransferForm
    {
        private IContainer components = null;
        private LabelControl lblSourceAccount;
        private LookUpEdit cmbSourceAccount;
        private LabelControl lblTargetIban;
        private TextEdit txtTargetIban;
        private LabelControl lblAmount;
        private CalcEdit txtAmount;
        private LabelControl lblDescription;
        private MemoEdit txtDescription;
        private SimpleButton btnTransfer;
        private SimpleButton btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblSourceAccount = new LabelControl();
            this.cmbSourceAccount = new LookUpEdit();
            this.lblTargetIban = new LabelControl();
            this.txtTargetIban = new TextEdit();
            this.lblAmount = new LabelControl();
            this.txtAmount = new CalcEdit();
            this.lblDescription = new LabelControl();
            this.txtDescription = new MemoEdit();
            this.btnTransfer = new SimpleButton();
            this.btnCancel = new SimpleButton();

            ((ISupportInitialize)(this.cmbSourceAccount.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtTargetIban.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtAmount.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtDescription.Properties)).BeginInit();
            this.SuspendLayout();

            // Source Account
            this.lblSourceAccount.Location = new System.Drawing.Point(30, 30);
            this.lblSourceAccount.Name = "lblSourceAccount";
            this.lblSourceAccount.Text = "Kaynak Hesap (Test Customer: ID 2)";
            
            this.cmbSourceAccount.Location = new System.Drawing.Point(30, 50);
            this.cmbSourceAccount.Name = "cmbSourceAccount";
            this.cmbSourceAccount.Properties.NullText = "Hesap Seçiniz...";
            this.cmbSourceAccount.Properties.ValueMember = "Id"; // Important for binding
            this.cmbSourceAccount.Properties.DisplayMember = "AccountNumber"; // Show Account No
            this.cmbSourceAccount.Size = new System.Drawing.Size(300, 30);

            // Target IBAN
            this.lblTargetIban.Location = new System.Drawing.Point(30, 100);
            this.lblTargetIban.Name = "lblTargetIban";
            this.lblTargetIban.Text = "Hedef IBAN";

            this.txtTargetIban.Location = new System.Drawing.Point(30, 120);
            this.txtTargetIban.Name = "txtTargetIban";
            this.txtTargetIban.Size = new System.Drawing.Size(300, 30);

            // Amount
            this.lblAmount.Location = new System.Drawing.Point(30, 170);
            this.lblAmount.Name = "lblAmount";
            this.lblAmount.Text = "Tutar";

            this.txtAmount.Location = new System.Drawing.Point(30, 190);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.Size = new System.Drawing.Size(150, 30);

            // Description
            this.lblDescription.Location = new System.Drawing.Point(30, 240);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Text = "Açıklama";

            this.txtDescription.Location = new System.Drawing.Point(30, 260);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(300, 60);

            // Buttons
            this.btnTransfer.Location = new System.Drawing.Point(30, 340);
            this.btnTransfer.Name = "btnTransfer";
            this.btnTransfer.Size = new System.Drawing.Size(140, 40);
            this.btnTransfer.Text = "Transfer Yap";
            this.btnTransfer.Click += new System.EventHandler(this.btnTransfer_Click);

            this.btnCancel.Location = new System.Drawing.Point(190, 340);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(140, 40);
            this.btnCancel.Text = "İptal";
            this.btnCancel.Click += (s, e) => this.Close();

            // TransferForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 420);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnTransfer);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtAmount);
            this.Controls.Add(this.lblAmount);
            this.Controls.Add(this.txtTargetIban);
            this.Controls.Add(this.lblTargetIban);
            this.Controls.Add(this.cmbSourceAccount);
            this.Controls.Add(this.lblSourceAccount);
            this.Name = "TransferForm";
            this.Text = "Para Transferi";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.cmbSourceAccount.Properties)).EndInit();
            ((ISupportInitialize)(this.txtTargetIban.Properties)).EndInit();
            ((ISupportInitialize)(this.txtAmount.Properties)).EndInit();
            ((ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
