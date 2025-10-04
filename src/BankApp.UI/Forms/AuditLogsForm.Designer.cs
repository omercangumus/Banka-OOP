using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace BankApp.UI.Forms
{
    public partial class AuditLogsForm
    {
        private IContainer components = null;
        private GridControl gridLogs;
        private GridView gridViewLogs;
        private SimpleButton btnExport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.gridLogs = new GridControl();
            this.gridViewLogs = new GridView();
            this.btnExport = new SimpleButton();

            ((ISupportInitialize)(this.gridLogs)).BeginInit();
            ((ISupportInitialize)(this.gridViewLogs)).BeginInit();
            this.SuspendLayout();

            // btnExport
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExport.Height = 40;
            this.btnExport.Text = "Logları Excel'e Aktar";
            this.btnExport.Click += (s, e) =>
            {
                try { 
                    gridLogs.ExportToXlsx("SistemLoglari.xlsx");
                    System.Diagnostics.Process.Start("SistemLoglari.xlsx");
                } catch {}
            };

            // gridLogs
            this.gridLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLogs.MainView = this.gridViewLogs;
            this.gridLogs.Name = "gridLogs";
            this.gridLogs.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewLogs});

            // gridViewLogs
            this.gridViewLogs.GridControl = this.gridLogs;
            this.gridViewLogs.Name = "gridViewLogs";
            this.gridViewLogs.OptionsBehavior.Editable = false;
            this.gridViewLogs.OptionsView.ShowAutoFilterRow = true;

            // AuditLogsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.gridLogs);
            this.Controls.Add(this.btnExport);
            this.Name = "AuditLogsForm";
            this.Text = "Sistem ve Güvenlik Logları";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.gridLogs)).EndInit();
            ((ISupportInitialize)(this.gridViewLogs)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
