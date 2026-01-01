using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Hesap hareketleri formu tasarımcı kodu
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class TransactionHistoryForm
    {
        private IContainer components = null;
        private GridControl grdIslemler;
        private GridView grdwIslemler;
        private SimpleButton btnExcelAktar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grdIslemler = new GridControl();
            this.grdwIslemler = new GridView();
            this.btnExcelAktar = new SimpleButton();

            ((ISupportInitialize)(this.grdIslemler)).BeginInit();
            ((ISupportInitialize)(this.grdwIslemler)).BeginInit();
            this.SuspendLayout();

            // btnExcelAktar (Fırat Standardı: btn prefix)
            this.btnExcelAktar.Dock = DockStyle.Bottom;
            this.btnExcelAktar.Height = 40;
            this.btnExcelAktar.Text = "Listeyi Excel'e Aktar";
            this.btnExcelAktar.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.btnExcelAktar.Click += (s,e) => 
            {
                try {
                    grdIslemler.ExportToXlsx("Hareketler.xlsx");
                    System.Diagnostics.Process.Start("Hareketler.xlsx");
                } catch {}
            };

            // grdIslemler (Fırat Standardı: grd prefix)
            this.grdIslemler.Dock = DockStyle.Fill;
            this.grdIslemler.MainView = this.grdwIslemler;
            this.grdIslemler.Name = "grdIslemler";
            this.grdIslemler.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.grdwIslemler});
            
            // grdwIslemler (Fırat Standardı: grdw prefix)
            this.grdwIslemler.GridControl = this.grdIslemler;
            this.grdwIslemler.Name = "grdwIslemler";
            this.grdwIslemler.OptionsBehavior.Editable = false;
            this.grdwIslemler.OptionsView.ShowGroupPanel = false;
            this.grdwIslemler.Appearance.Row.Font = new Font("Tahoma", 8.25F);
            this.grdwIslemler.Appearance.HeaderPanel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);

            // TransactionHistoryForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 500);
            this.Controls.Add(this.grdIslemler);
            this.Controls.Add(this.btnExcelAktar);
            this.Name = "TransactionHistoryForm";
            this.Text = "Hesap Hareketleri";
            this.StartPosition = FormStartPosition.CenterParent;

            ((ISupportInitialize)(this.grdIslemler)).EndInit();
            ((ISupportInitialize)(this.grdwIslemler)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
