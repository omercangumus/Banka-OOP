using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace BankApp.UI.Forms
{
    partial class CustomerAccountsForm
    {
        private IContainer components = null;
        private GroupControl groupControl1;
        private GridControl gridAccounts;
        private GridView gridViewAccounts;
        private SimpleButton btnNewAccount;
        private SimpleButton btnTransactions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.groupControl1 = new GroupControl();
            this.gridAccounts = new GridControl();
            this.gridViewAccounts = new GridView();
            this.btnNewAccount = new SimpleButton();
            this.btnTransactions = new SimpleButton();

            ((ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((ISupportInitialize)(this.gridAccounts)).BeginInit();
            ((ISupportInitialize)(this.gridViewAccounts)).BeginInit();
            this.SuspendLayout();

            // groupControl1
            this.groupControl1.Controls.Add(this.btnTransactions);
            this.groupControl1.Controls.Add(this.btnNewAccount);
            this.groupControl1.Controls.Add(this.gridAccounts);
            this.groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Text = "Hesaplar";

            // gridAccounts
            this.gridAccounts.Dock = System.Windows.Forms.DockStyle.Top;
            this.gridAccounts.Height = 350;
            this.gridAccounts.MainView = this.gridViewAccounts;
            this.gridAccounts.Name = "gridAccounts";
            this.gridAccounts.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewAccounts});
            
            // gridViewAccounts
            this.gridViewAccounts.GridControl = this.gridAccounts;
            this.gridViewAccounts.Name = "gridViewAccounts";
            this.gridViewAccounts.OptionsBehavior.Editable = false;
            this.gridViewAccounts.OptionsView.ShowGroupPanel = false;

            // btnNewAccount
            this.btnNewAccount.Location = new System.Drawing.Point(20, 370);
            this.btnNewAccount.Name = "btnNewAccount";
            this.btnNewAccount.Size = new System.Drawing.Size(150, 40);
            this.btnNewAccount.Text = "Yeni Hesap Aç";
            this.btnNewAccount.Click += new System.EventHandler(this.btnNewAccount_Click);

            // btnTransactions
            this.btnTransactions.Location = new System.Drawing.Point(190, 370);
            this.btnTransactions.Name = "btnTransactions";
            this.btnTransactions.Size = new System.Drawing.Size(150, 40);
            this.btnTransactions.Text = "Hareketler";
            this.btnTransactions.Click += new System.EventHandler(this.btnTransactions_Click);

            // CustomerAccountsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 430);
            this.Controls.Add(this.groupControl1);
            this.Name = "CustomerAccountsForm";
            this.Text = "Müşteri Hesapları";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((ISupportInitialize)(this.gridAccounts)).EndInit();
            ((ISupportInitialize)(this.gridViewAccounts)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
