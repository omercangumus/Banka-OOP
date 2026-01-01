using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Denetim logları formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class AuditLogsForm
    {
        private IContainer components = null;
        private GridControl grdLoglar;
        private GridView grdwLoglar;
        private SimpleButton btnExcelAktar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grdLoglar = new GridControl();
            this.grdwLoglar = new GridView();
            this.btnExcelAktar = new SimpleButton();

            ((ISupportInitialize)(this.grdLoglar)).BeginInit();
            ((ISupportInitialize)(this.grdwLoglar)).BeginInit();
            this.SuspendLayout();

            // btnExcelAktar (Fırat Standardı: btn prefix, Tahoma font)
            this.btnExcelAktar.Dock = DockStyle.Bottom;
            this.btnExcelAktar.Height = 40;
            this.btnExcelAktar.Name = "btnExcelAktar";
            this.btnExcelAktar.Text = "Logları Excel'e Aktar";
            this.btnExcelAktar.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.btnExcelAktar.Click += (s, e) =>
            {
                try { 
                    grdLoglar.ExportToXlsx("SistemLoglari.xlsx");
                    System.Diagnostics.Process.Start("SistemLoglari.xlsx");
                } catch {}
            };

            // grdLoglar (Fırat Standardı: grd prefix for GridControl)
            this.grdLoglar.Dock = DockStyle.Fill;
            this.grdLoglar.MainView = this.grdwLoglar;
            this.grdLoglar.Name = "grdLoglar";
            this.grdLoglar.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.grdwLoglar});

            // grdwLoglar (Fırat Standardı: grdw prefix for GridView)
            this.grdwLoglar.GridControl = this.grdLoglar;
            this.grdwLoglar.Name = "grdwLoglar";
            this.grdwLoglar.OptionsBehavior.Editable = false;
            this.grdwLoglar.OptionsView.ShowAutoFilterRow = true;
            this.grdwLoglar.Appearance.Row.Font = new Font("Tahoma", 8.25F);
            this.grdwLoglar.Appearance.HeaderPanel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);

            // AuditLogsForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 600);
            this.Controls.Add(this.grdLoglar);
            this.Controls.Add(this.btnExcelAktar);
            this.Name = "AuditLogsForm";
            this.Text = "Sistem ve Güvenlik Logları";
            this.StartPosition = FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.grdLoglar)).EndInit();
            ((ISupportInitialize)(this.grdwLoglar)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
