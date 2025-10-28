using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.LookAndFeel;

namespace BankApp.UI.Forms
{
    partial class InvestmentForm
    {
        private IContainer components = null;
        
        // Layout
        private Panel pnlHeader;
        private LabelControl lblHeader;
        
        // Tiles (Market Data)
        private TileControl tileControlMarket;
        private TileGroup tileGroup1; // "Piyasalar"
        
        // Portfolio
        private GroupControl grpPortfolio;
        private GridControl grdPortfoy;
        private GridView grdwPortfoy;
        
        // Buttons
        private Panel pnlButtons;
        private SimpleButton btnHisseAl;
        private SimpleButton btnHisseSat;
        private SimpleButton btnYenile;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Header
            this.pnlHeader = new Panel();
            this.lblHeader = new LabelControl();
            
            // Tiles
            this.tileControlMarket = new TileControl();
            this.tileGroup1 = new TileGroup();
            
            // Portfolio
            this.grpPortfolio = new GroupControl();
            this.grdPortfoy = new GridControl();
            this.grdwPortfoy = new GridView();
            
            // Buttons
            this.pnlButtons = new Panel();
            this.btnHisseAl = new SimpleButton();
            this.btnHisseSat = new SimpleButton();
            this.btnYenile = new SimpleButton();

            ((ISupportInitialize)(this.grpPortfolio)).BeginInit();
            this.grpPortfolio.SuspendLayout();
            ((ISupportInitialize)(this.grdPortfoy)).BeginInit();
            ((ISupportInitialize)(this.grdwPortfoy)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // ---------------------------------------------------------
            // HEADER
            // ---------------------------------------------------------
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 80;
            this.pnlHeader.BackColor = Color.FromArgb(20, 20, 20);
            this.pnlHeader.Controls.Add(this.lblHeader);
            
            this.lblHeader.Text = "YATIRIM DÜNYASI";
            this.lblHeader.Appearance.Font = new Font("Segoe UI Light", 28F);
            this.lblHeader.Appearance.ForeColor = Color.Gold;
            this.lblHeader.Location = new Point(20, 15);
            this.lblHeader.Text = "YATIRIM DÜNYASI"; // Fix duplicate

            // ---------------------------------------------------------
            // TILE CONTROL (MODERN UI)
            // ---------------------------------------------------------
            this.tileControlMarket.Dock = DockStyle.Top;
            this.tileControlMarket.Height = 250;
            this.tileControlMarket.BackColor = Color.FromArgb(30, 30, 35);
            this.tileControlMarket.Groups.Add(this.tileGroup1);
            this.tileControlMarket.AllowDrag = false;
            this.tileControlMarket.AllowItemHover = true; // Hover efekti
            this.tileControlMarket.ItemCheckMode = TileItemCheckMode.None;
            this.tileControlMarket.Orientation = Orientation.Horizontal;
            this.tileControlMarket.Name = "tileControlMarket";
            
            this.tileGroup1.Name = "tileGroupMarkets";
            this.tileGroup1.Text = "Canlı Piyasa Verileri";
            // Items logic in code-behind

            // ---------------------------------------------------------
            // PORTFOLIO GROUP
            // ---------------------------------------------------------
            this.grpPortfolio.Dock = DockStyle.Fill;
            this.grpPortfolio.AppearanceCaption.Font = new Font("Tahoma", 12F, FontStyle.Bold);
            this.grpPortfolio.Text = "Varlıklarım & Portföy Özeti";
            this.grpPortfolio.Controls.Add(this.grdPortfoy);

            this.grdPortfoy.Dock = DockStyle.Fill;
            this.grdPortfoy.MainView = this.grdwPortfoy;
            
            this.grdwPortfoy.OptionsView.ShowGroupPanel = false;
            this.grdwPortfoy.OptionsView.RowAutoHeight = true;
            this.grdwPortfoy.Appearance.HeaderPanel.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            this.grdwPortfoy.Appearance.Row.Font = new Font("Tahoma", 11F);

            // ---------------------------------------------------------
            // BUTTONS
            // ---------------------------------------------------------
            this.pnlButtons.Dock = DockStyle.Bottom;
            this.pnlButtons.Height = 80;
            this.pnlButtons.BackColor = Color.FromArgb(20, 20, 20);
            this.pnlButtons.Controls.Add(this.btnYenile);
            this.pnlButtons.Controls.Add(this.btnHisseSat);
            this.pnlButtons.Controls.Add(this.btnHisseAl);
            this.pnlButtons.Padding = new Padding(20);

            this.btnHisseAl.Text = "YENİ İŞLEM YAP (AL)";
            this.btnHisseAl.Appearance.BackColor = Color.FromArgb(0, 150, 136); // Android Green
            this.btnHisseAl.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnHisseAl.Dock = DockStyle.Left;
            this.btnHisseAl.Width = 250;
            this.btnHisseAl.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            this.btnHisseSat.Text = "POZİSYON KAPAT (SAT)";
            this.btnHisseSat.Appearance.BackColor = Color.FromArgb(211, 47, 47); // Material Red
            this.btnHisseSat.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnHisseSat.Dock = DockStyle.Left;
            this.btnHisseSat.Width = 250;
            this.btnHisseSat.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            
            // Spacer
            Panel spacer = new Panel { Width = 20, Dock = DockStyle.Left };
            this.pnlButtons.Controls.Add(spacer);
            this.pnlButtons.Controls.SetChildIndex(spacer, 1); // After Al

            this.btnYenile.Text = "Yenile";
            this.btnYenile.Dock = DockStyle.Right;
            this.btnYenile.Width = 100;

            // ---------------------------------------------------------
            // FORM
            // ---------------------------------------------------------
            this.Controls.Add(this.grpPortfolio);
            this.Controls.Add(this.tileControlMarket);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlHeader);
            
            this.ClientSize = new Size(1200, 800);
            this.Text = "BankApp Trader Pro";
            this.Name = "InvestmentForm";
            this.WindowState = FormWindowState.Maximized;

            ((ISupportInitialize)(this.grpPortfolio)).EndInit();
            this.grpPortfolio.ResumeLayout(false);
            ((ISupportInitialize)(this.grdPortfoy)).EndInit();
            ((ISupportInitialize)(this.grdwPortfoy)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
