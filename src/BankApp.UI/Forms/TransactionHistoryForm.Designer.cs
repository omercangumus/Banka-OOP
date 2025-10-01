using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace BankApp.UI.Forms
{
    partial class TransactionHistoryForm
    {
        private IContainer components = null;
        private GridControl gridTransactions;
        private GridView gridViewTransactions;
        private SimpleButton btnExport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.gridTransactions = new GridControl();
            this.gridViewTransactions = new GridView();
            this.btnExport = new SimpleButton();

            ((ISupportInitialize)(this.gridTransactions)).BeginInit();
            ((ISupportInitialize)(this.gridViewTransactions)).BeginInit();
            this.SuspendLayout();

            // btnExport
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExport.Height = 40;
            this.btnExport.Text = "Listeyi Excel'e Aktar";
            this.btnExport.Click += (s,e) => 
            {
                try {
                    gridTransactions.ExportToXlsx("Hareketler.xlsx");
                    System.Diagnostics.Process.Start("Hareketler.xlsx");
                } catch {}
            };

            // gridTransactions
            this.gridTransactions.Dock = System.Windows.Forms.DockStyle.Fill;

            this.gridTransactions.MainView = this.gridViewTransactions;
            this.gridTransactions.Name = "gridTransactions";
            this.gridTransactions.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewTransactions});
            
            // gridViewTransactions
            this.gridViewTransactions.GridControl = this.gridTransactions;
            this.gridViewTransactions.Name = "gridViewTransactions";
            this.gridViewTransactions.OptionsBehavior.Editable = false;
            this.gridViewTransactions.OptionsView.ShowGroupPanel = false;

            // TransactionHistoryForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.gridTransactions);
            this.Controls.Add(this.btnExport);
            this.Name = "TransactionHistoryForm";

            this.Text = "Hesap Hareketleri";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.gridTransactions)).EndInit();
            ((ISupportInitialize)(this.gridViewTransactions)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
