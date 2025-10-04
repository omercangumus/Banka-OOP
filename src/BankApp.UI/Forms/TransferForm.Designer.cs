using System.ComponentModel;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    public partial class TransferForm
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

            // Source Account - PROFESYONEL BOYUTLAR
            this.lblSourceAccount.Location = new System.Drawing.Point(50, 40);
            this.lblSourceAccount.Name = "lblSourceAccount";
            this.lblSourceAccount.Text = "Kaynak Hesap";
            this.lblSourceAccount.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            
            this.cmbSourceAccount.Location = new System.Drawing.Point(50, 70);
            this.cmbSourceAccount.Name = "cmbSourceAccount";
            this.cmbSourceAccount.Properties.NullText = "Hesap Seçiniz...";
            this.cmbSourceAccount.Properties.ValueMember = "Id";
            this.cmbSourceAccount.Properties.DisplayMember = "AccountNumber";
            this.cmbSourceAccount.Size = new System.Drawing.Size(580, 48);
            this.cmbSourceAccount.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 13F);

            // Target IBAN - PROFESYONEL BOYUTLAR
            this.lblTargetIban.Location = new System.Drawing.Point(50, 140);
            this.lblTargetIban.Name = "lblTargetIban";
            this.lblTargetIban.Text = "Hedef IBAN";
            this.lblTargetIban.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);

            this.txtTargetIban.Location = new System.Drawing.Point(50, 170);
            this.txtTargetIban.Name = "txtTargetIban";
            this.txtTargetIban.Size = new System.Drawing.Size(580, 48);
            this.txtTargetIban.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 13F);

            // Amount - PROFESYONEL BOYUTLAR
            this.lblAmount.Location = new System.Drawing.Point(50, 240);
            this.lblAmount.Name = "lblAmount";
            this.lblAmount.Text = "Tutar (TL)";
            this.lblAmount.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);

            this.txtAmount.Location = new System.Drawing.Point(50, 270);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.Size = new System.Drawing.Size(280, 48);
            this.txtAmount.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);

            // Description - PROFESYONEL BOYUTLAR
            this.lblDescription.Location = new System.Drawing.Point(50, 340);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Text = "Açıklama";
            this.lblDescription.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);

            this.txtDescription.Location = new System.Drawing.Point(50, 370);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(580, 80);
            this.txtDescription.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 13F);

            // Buttons - PROFESYONEL BOYUTLAR
            this.btnTransfer.Location = new System.Drawing.Point(50, 480);
            this.btnTransfer.Name = "btnTransfer";
            this.btnTransfer.Size = new Size(270, 55);
            this.btnTransfer.Text = "TRANSFER YAP";
            this.btnTransfer.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnTransfer.Appearance.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.btnTransfer.Appearance.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnTransfer.Appearance.Options.UseBackColor = true;
            this.btnTransfer.Appearance.Options.UseForeColor = true;
            this.btnTransfer.Click += new System.EventHandler(this.btnTransfer_Click);

            this.btnCancel.Location = new System.Drawing.Point(360, 480);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(270, 55);
            this.btnCancel.Text = "İPTAL";
            this.btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Appearance.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.btnCancel.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Appearance.Options.UseBackColor = true;
            this.btnCancel.Appearance.Options.UseForeColor = true;
            this.btnCancel.Click += (s, e) => this.Close();

            // TransferForm - PROFESYONEL BOYUTLAR
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 620);
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            ((ISupportInitialize)(this.cmbSourceAccount.Properties)).EndInit();
            ((ISupportInitialize)(this.txtTargetIban.Properties)).EndInit();
            ((ISupportInitialize)(this.txtAmount.Properties)).EndInit();
            ((ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
