#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.LookAndFeel;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;
using System.Threading.Tasks;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Hisse Alım Popup'ı - Modern tasarım
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class StockBuyPopup : XtraForm
    {
        private Stock _stock;
        private InvestmentService _investmentService;
        private int _customerId;
        
        private LabelControl lblBaslik;
        private LabelControl lblHisse;
        private LabelControl lblFiyat;
        private LabelControl lblToplam;
        private LookUpEdit cmbHesap;
        private SpinEdit spinAdet;
        private SimpleButton btnSatinAl;
        private SimpleButton btnIptal;

        /// <summary>
        /// Popup yapıcı metodu
        /// </summary>
        public StockBuyPopup(Stock stock, InvestmentService investmentService, int customerId)
        {
            _stock = stock;
            _investmentService = investmentService;
            _customerId = customerId;
            InitializeComponent();
            LoadAccounts();
            UpdateTotal();
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            this.lblBaslik = new LabelControl();
            this.lblHisse = new LabelControl();
            this.lblFiyat = new LabelControl();
            this.lblToplam = new LabelControl();
            this.cmbHesap = new LookUpEdit();
            this.spinAdet = new SpinEdit();
            this.btnSatinAl = new SimpleButton();
            this.btnIptal = new SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.cmbHesap.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinAdet.Properties)).BeginInit();
            this.SuspendLayout();

            // Title
            this.lblBaslik.Text = "🛒 Hisse Al";
            this.lblBaslik.Location = new Point(25, 20);
            this.lblBaslik.Appearance.Font = new Font("Tahoma", 18F, FontStyle.Bold);
            this.lblBaslik.Appearance.ForeColor = Color.White;

            // Stock info
            this.lblHisse.Text = $"{_stock.Symbol} - {_stock.Name}";
            this.lblHisse.Location = new Point(25, 70);
            this.lblHisse.Appearance.Font = new Font("Tahoma", 14F);
            this.lblHisse.Appearance.ForeColor = Color.FromArgb(33, 150, 243);

            this.lblFiyat.Text = $"Güncel Fiyat: {_stock.CurrentPrice:N2} ₺";
            this.lblFiyat.Location = new Point(25, 100);
            this.lblFiyat.Appearance.Font = new Font("Tahoma", 12F);
            this.lblFiyat.Appearance.ForeColor = Color.FromArgb(180, 180, 190);

            // Account selection
            var lblAccount = new LabelControl();
            lblAccount.Text = "Kaynak Hesap:";
            lblAccount.Location = new Point(25, 140);
            lblAccount.Appearance.Font = new Font("Tahoma", 11F);
            lblAccount.Appearance.ForeColor = Color.White;
            this.Controls.Add(lblAccount);

            this.cmbHesap.Location = new Point(25, 165);
            this.cmbHesap.Size = new Size(300, 35);
            this.cmbHesap.Properties.Appearance.Font = new Font("Tahoma", 11F);
            this.cmbHesap.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.cmbHesap.Properties.Appearance.ForeColor = Color.White;
            this.cmbHesap.Properties.NullText = "Hesap Seçiniz...";

            // Quantity
            var lblQuantityTitle = new LabelControl();
            lblQuantityTitle.Text = "Adet:";
            lblQuantityTitle.Location = new Point(25, 215);
            lblQuantityTitle.Appearance.Font = new Font("Tahoma", 11F);
            lblQuantityTitle.Appearance.ForeColor = Color.White;
            this.Controls.Add(lblQuantityTitle);

            this.spinAdet.Location = new Point(25, 240);
            this.spinAdet.Size = new Size(150, 40);
            this.spinAdet.Properties.MinValue = 1;
            this.spinAdet.Properties.MaxValue = 10000;
            this.spinAdet.Properties.Increment = 1;
            this.spinAdet.Value = 1;
            this.spinAdet.Properties.Appearance.Font = new Font("Tahoma", 14F, FontStyle.Bold);
            this.spinAdet.Properties.Appearance.ForeColor = Color.White;
            this.spinAdet.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.spinAdet.EditValueChanged += (s, e) => UpdateTotal();

            // Total
            this.lblToplam.Text = "Toplam: 0.00 ₺";
            this.lblToplam.Location = new Point(25, 300);
            this.lblToplam.Appearance.Font = new Font("Tahoma", 20F, FontStyle.Bold);
            this.lblToplam.Appearance.ForeColor = Color.FromArgb(76, 175, 80);

            // Buttons
            this.btnSatinAl.Text = "✅ SATIN AL";
            this.btnSatinAl.Location = new Point(25, 360);
            this.btnSatinAl.Size = new Size(145, 50);
            this.btnSatinAl.Appearance.Font = new Font("Tahoma", 12F, FontStyle.Bold);
            this.btnSatinAl.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnSatinAl.Appearance.ForeColor = Color.White;
            this.btnSatinAl.Appearance.Options.UseBackColor = true;
            this.btnSatinAl.Appearance.Options.UseForeColor = true;
            this.btnSatinAl.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnSatinAl.Click += BtnSatinAl_Click;

            this.btnIptal.Text = "İptal";
            this.btnIptal.Location = new Point(180, 360);
            this.btnIptal.Size = new Size(145, 50);
            this.btnIptal.Appearance.Font = new Font("Tahoma", 12F);
            this.btnIptal.Appearance.BackColor = Color.FromArgb(60, 60, 70);
            this.btnIptal.Appearance.ForeColor = Color.White;
            this.btnIptal.Appearance.Options.UseBackColor = true;
            this.btnIptal.Appearance.Options.UseForeColor = true;
            this.btnIptal.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnIptal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Form
            this.Controls.AddRange(new Control[] { 
                lblBaslik, lblHisse, lblFiyat, lblToplam, 
                cmbHesap, spinAdet, btnSatinAl, btnIptal 
            });
            
            this.ClientSize = new Size(350, 430);
            this.Text = "Hisse Al";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(25, 28, 35);
            
            // Rounded corners
            this.Load += (s, e) => {
                var path = new GraphicsPath();
                int radius = 20;
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(this.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(this.Width - radius * 2, this.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, this.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                this.Region = new Region(path);
            };

            ((System.ComponentModel.ISupportInitialize)(this.cmbHesap.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinAdet.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private async void LoadAccounts()
        {
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var accounts = await accountRepo.GetAllAsync();
            
            cmbHesap.Properties.DataSource = accounts;
            cmbHesap.Properties.DisplayMember = "AccountNumber";
            cmbHesap.Properties.ValueMember = "Id";
            
            cmbHesap.Properties.Columns.Clear();
            cmbHesap.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AccountNumber", "Hesap No"));
            cmbHesap.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Balance", "Bakiye"));
        }

        private void UpdateTotal()
        {
            decimal quantity = spinAdet.Value;
            decimal total = quantity * _stock.CurrentPrice;
            lblToplam.Text = $"Toplam: {total:N2} ₺";
        }

        private async void BtnSatinAl_Click(object? sender, EventArgs e)
        {
            if (cmbHesap.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen bir hesap seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int accountId = (int)cmbHesap.EditValue;
            decimal quantity = spinAdet.Value;

            // Updated to handle string return type
            var resultMessage = await _investmentService.BuyStockAsync(_customerId, accountId, _stock.Symbol, quantity);
            
            if (resultMessage == null) // Success
            {
                XtraMessageBox.Show($"{quantity} adet {_stock.Symbol} başarıyla alındı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                XtraMessageBox.Show(resultMessage, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Hisse Satış Popup'ı
    /// </summary>
    public class StockSellPopup : XtraForm
    {
        private PortfolioItem _portfolioItem;
        private InvestmentService _investmentService;
        private int _customerId;
        
        private SpinEdit spinAdet;
        private LabelControl lblToplam2;

        /// <summary>
        /// Popup yapıcı metodu
        /// </summary>
        public StockSellPopup(PortfolioItem item, InvestmentService investmentService, int customerId)
        {
            _portfolioItem = item;
            _investmentService = investmentService;
            _customerId = customerId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            var lblBaslik = new LabelControl();
            lblBaslik.Text = $"💰 {_portfolioItem.StockSymbol} Sat";
            lblBaslik.Location = new Point(25, 20);
            lblBaslik.Appearance.Font = new Font("Tahoma", 18F, FontStyle.Bold);
            lblBaslik.Appearance.ForeColor = Color.White;

            var lblBilgi = new LabelControl();
            lblBilgi.Text = $"Mevcut: {_portfolioItem.Quantity:N0} adet | Fiyat: {_portfolioItem.CurrentPrice:N2} ₺";
            lblBilgi.Location = new Point(25, 70);
            lblBilgi.Appearance.Font = new Font("Tahoma", 11F);
            lblBilgi.Appearance.ForeColor = Color.FromArgb(180, 180, 190);

            this.spinAdet = new SpinEdit();
            this.spinAdet.Location = new Point(25, 120);
            this.spinAdet.Size = new Size(150, 40);
            this.spinAdet.Properties.MinValue = 1;
            this.spinAdet.Properties.MaxValue = _portfolioItem.Quantity;
            this.spinAdet.Value = 1;
            this.spinAdet.Properties.Appearance.Font = new Font("Tahoma", 14F, FontStyle.Bold);
            this.spinAdet.Properties.Appearance.ForeColor = Color.White;
            this.spinAdet.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.spinAdet.EditValueChanged += (s, e) => UpdateTotal();

            this.lblToplam2 = new LabelControl();
            this.lblToplam2.Text = "Alacağınız: 0.00 ₺";
            this.lblToplam2.Location = new Point(25, 180);
            this.lblToplam2.Appearance.Font = new Font("Tahoma", 18F, FontStyle.Bold);
            this.lblToplam2.Appearance.ForeColor = Color.FromArgb(76, 175, 80);

            var btnSat = new SimpleButton();
            btnSat.Text = "💵 SAT";
            btnSat.Location = new Point(25, 240);
            btnSat.Size = new Size(130, 50);
            btnSat.Appearance.Font = new Font("Tahoma", 12F, FontStyle.Bold);
            btnSat.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            btnSat.Appearance.ForeColor = Color.White;
            btnSat.Appearance.Options.UseBackColor = true;
            btnSat.Appearance.Options.UseForeColor = true;
            btnSat.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnSat.Click += BtnSat_Click;

            var btnIptal = new SimpleButton();
            btnIptal.Text = "İptal";
            btnIptal.Location = new Point(165, 240);
            btnIptal.Size = new Size(110, 50);
            btnIptal.Appearance.BackColor = Color.FromArgb(60, 60, 70);
            btnIptal.Appearance.ForeColor = Color.White;
            btnIptal.Appearance.Options.UseBackColor = true;
            btnIptal.Appearance.Options.UseForeColor = true;
            btnIptal.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnIptal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblBaslik, lblBilgi, spinAdet, lblToplam2, btnSat, btnIptal });
            
            this.ClientSize = new Size(300, 310);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(25, 28, 35);
            
            this.Load += (s, e) => {
                var path = new GraphicsPath();
                int radius = 20;
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(this.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(this.Width - radius * 2, this.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, this.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                this.Region = new Region(path);
            };

            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = spinAdet.Value * _portfolioItem.CurrentPrice;
            lblToplam2.Text = $"Alacağınız: {total:N2} ₺";
        }

        private async void BtnSat_Click(object? sender, EventArgs e)
        {
            decimal quantity = spinAdet.Value;
            // Updated to handle string return type
            var resultMessage = await _investmentService.SellStockAsync(_customerId, 1, _portfolioItem.StockSymbol, quantity);
            
            if (resultMessage == null) // Success
            {
                XtraMessageBox.Show($"{quantity} adet {_portfolioItem.StockSymbol} satıldı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                XtraMessageBox.Show(resultMessage, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
