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

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Hisse Alım Popup'ı - Modern tasarım
    /// </summary>
    public class StockBuyPopup : XtraForm
    {
        private Stock _stock;
        private InvestmentService _investmentService;
        private int _customerId;
        
        private LabelControl lblTitle;
        private LabelControl lblStock;
        private LabelControl lblPrice;
        private LabelControl lblTotal;
        private LookUpEdit cmbAccount;
        private SpinEdit spinQuantity;
        private SimpleButton btnBuy;
        private SimpleButton btnCancel;

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
            
            this.lblTitle = new LabelControl();
            this.lblStock = new LabelControl();
            this.lblPrice = new LabelControl();
            this.lblTotal = new LabelControl();
            this.cmbAccount = new LookUpEdit();
            this.spinQuantity = new SpinEdit();
            this.btnBuy = new SimpleButton();
            this.btnCancel = new SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.cmbAccount.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQuantity.Properties)).BeginInit();
            this.SuspendLayout();

            // Title
            this.lblTitle.Text = "🛒 Hisse Al";
            this.lblTitle.Location = new Point(25, 20);
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.White;

            // Stock info
            this.lblStock.Text = $"{_stock.Symbol} - {_stock.Name}";
            this.lblStock.Location = new Point(25, 70);
            this.lblStock.Appearance.Font = new Font("Segoe UI", 14F);
            this.lblStock.Appearance.ForeColor = Color.FromArgb(33, 150, 243);

            this.lblPrice.Text = $"Güncel Fiyat: {_stock.CurrentPrice:N2} ₺";
            this.lblPrice.Location = new Point(25, 100);
            this.lblPrice.Appearance.Font = new Font("Segoe UI", 12F);
            this.lblPrice.Appearance.ForeColor = Color.FromArgb(180, 180, 190);

            // Account selection
            var lblAccount = new LabelControl();
            lblAccount.Text = "Kaynak Hesap:";
            lblAccount.Location = new Point(25, 140);
            lblAccount.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblAccount.Appearance.ForeColor = Color.White;
            this.Controls.Add(lblAccount);

            this.cmbAccount.Location = new Point(25, 165);
            this.cmbAccount.Size = new Size(300, 35);
            this.cmbAccount.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            this.cmbAccount.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.cmbAccount.Properties.Appearance.ForeColor = Color.White;
            this.cmbAccount.Properties.NullText = "Hesap Seçiniz...";

            // Quantity
            var lblQuantityTitle = new LabelControl();
            lblQuantityTitle.Text = "Adet:";
            lblQuantityTitle.Location = new Point(25, 215);
            lblQuantityTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblQuantityTitle.Appearance.ForeColor = Color.White;
            this.Controls.Add(lblQuantityTitle);

            this.spinQuantity.Location = new Point(25, 240);
            this.spinQuantity.Size = new Size(150, 40);
            this.spinQuantity.Properties.MinValue = 1;
            this.spinQuantity.Properties.MaxValue = 10000;
            this.spinQuantity.Properties.Increment = 1;
            this.spinQuantity.Value = 1;
            this.spinQuantity.Properties.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.spinQuantity.Properties.Appearance.ForeColor = Color.White;
            this.spinQuantity.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.spinQuantity.EditValueChanged += (s, e) => UpdateTotal();

            // Total
            this.lblTotal.Text = "Toplam: 0.00 ₺";
            this.lblTotal.Location = new Point(25, 300);
            this.lblTotal.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            this.lblTotal.Appearance.ForeColor = Color.FromArgb(76, 175, 80);

            // Buttons
            this.btnBuy.Text = "✅ SATIN AL";
            this.btnBuy.Location = new Point(25, 360);
            this.btnBuy.Size = new Size(145, 50);
            this.btnBuy.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnBuy.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnBuy.Appearance.ForeColor = Color.White;
            this.btnBuy.Appearance.Options.UseBackColor = true;
            this.btnBuy.Appearance.Options.UseForeColor = true;
            this.btnBuy.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnBuy.Click += BtnBuy_Click;

            this.btnCancel.Text = "İptal";
            this.btnCancel.Location = new Point(180, 360);
            this.btnCancel.Size = new Size(145, 50);
            this.btnCancel.Appearance.Font = new Font("Segoe UI", 12F);
            this.btnCancel.Appearance.BackColor = Color.FromArgb(60, 60, 70);
            this.btnCancel.Appearance.ForeColor = Color.White;
            this.btnCancel.Appearance.Options.UseBackColor = true;
            this.btnCancel.Appearance.Options.UseForeColor = true;
            this.btnCancel.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Form
            this.Controls.AddRange(new Control[] { 
                lblTitle, lblStock, lblPrice, lblTotal, 
                cmbAccount, spinQuantity, btnBuy, btnCancel 
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

            ((System.ComponentModel.ISupportInitialize)(this.cmbAccount.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQuantity.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private async void LoadAccounts()
        {
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var accounts = await accountRepo.GetAllAsync();
            
            cmbAccount.Properties.DataSource = accounts;
            cmbAccount.Properties.DisplayMember = "AccountNumber";
            cmbAccount.Properties.ValueMember = "Id";
            
            cmbAccount.Properties.Columns.Clear();
            cmbAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AccountNumber", "Hesap No"));
            cmbAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Balance", "Bakiye"));
        }

        private void UpdateTotal()
        {
            decimal quantity = spinQuantity.Value;
            decimal total = quantity * _stock.CurrentPrice;
            lblTotal.Text = $"Toplam: {total:N2} ₺";
        }

        private async void BtnBuy_Click(object? sender, EventArgs e)
        {
            if (cmbAccount.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen bir hesap seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int accountId = (int)cmbAccount.EditValue;
            decimal quantity = spinQuantity.Value;

            var result = await _investmentService.BuyStockAsync(_customerId, accountId, _stock.Symbol, quantity);
            
            if (result.Success)
            {
                XtraMessageBox.Show(result.Message, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                XtraMessageBox.Show(result.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        
        private SpinEdit spinQuantity;
        private LabelControl lblTotal;

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
            
            var lblTitle = new LabelControl();
            lblTitle.Text = $"💰 {_portfolioItem.StockSymbol} Sat";
            lblTitle.Location = new Point(25, 20);
            lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;

            var lblInfo = new LabelControl();
            lblInfo.Text = $"Mevcut: {_portfolioItem.Quantity:N0} adet | Fiyat: {_portfolioItem.CurrentPrice:N2} ₺";
            lblInfo.Location = new Point(25, 70);
            lblInfo.Appearance.Font = new Font("Segoe UI", 11F);
            lblInfo.Appearance.ForeColor = Color.FromArgb(180, 180, 190);

            this.spinQuantity = new SpinEdit();
            this.spinQuantity.Location = new Point(25, 120);
            this.spinQuantity.Size = new Size(150, 40);
            this.spinQuantity.Properties.MinValue = 1;
            this.spinQuantity.Properties.MaxValue = _portfolioItem.Quantity;
            this.spinQuantity.Value = 1;
            this.spinQuantity.Properties.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.spinQuantity.Properties.Appearance.ForeColor = Color.White;
            this.spinQuantity.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.spinQuantity.EditValueChanged += (s, e) => UpdateTotal();

            this.lblTotal = new LabelControl();
            this.lblTotal.Text = "Alacağınız: 0.00 ₺";
            this.lblTotal.Location = new Point(25, 180);
            this.lblTotal.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTotal.Appearance.ForeColor = Color.FromArgb(76, 175, 80);

            var btnSell = new SimpleButton();
            btnSell.Text = "💵 SAT";
            btnSell.Location = new Point(25, 240);
            btnSell.Size = new Size(130, 50);
            btnSell.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSell.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            btnSell.Appearance.ForeColor = Color.White;
            btnSell.Appearance.Options.UseBackColor = true;
            btnSell.Appearance.Options.UseForeColor = true;
            btnSell.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnSell.Click += BtnSell_Click;

            var btnCancel = new SimpleButton();
            btnCancel.Text = "İptal";
            btnCancel.Location = new Point(165, 240);
            btnCancel.Size = new Size(110, 50);
            btnCancel.Appearance.BackColor = Color.FromArgb(60, 60, 70);
            btnCancel.Appearance.ForeColor = Color.White;
            btnCancel.Appearance.Options.UseBackColor = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblTitle, lblInfo, spinQuantity, lblTotal, btnSell, btnCancel });
            
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
            decimal total = spinQuantity.Value * _portfolioItem.CurrentPrice;
            lblTotal.Text = $"Alacağınız: {total:N2} ₺";
        }

        private async void BtnSell_Click(object? sender, EventArgs e)
        {
            decimal quantity = spinQuantity.Value;
            var result = await _investmentService.SellStockAsync(_customerId, 1, _portfolioItem.StockSymbol, quantity);
            
            if (result.Success)
            {
                XtraMessageBox.Show(result.Message, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                XtraMessageBox.Show(result.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
