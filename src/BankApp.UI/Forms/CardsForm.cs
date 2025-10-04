#nullable enable
using System;
using System.Collections.Generic;
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
    /// Kartlarım Formu - Apple Wallet Tarzı Görsel Kart Tasarımı
    /// </summary>
    public partial class CardsForm : XtraForm
    {
        private CardService _cardService;
        private int _customerId = 1;
        private CreditCard? _selectedCard;
        
        // UI Controls
        private FlowLayoutPanel pnlCards;
        private Panel pnlCardDetails;
        private SimpleButton btnNewCard;
        private SimpleButton btnPayDebt;
        private SimpleButton btnSimulateSpend;
        private LabelControl lblDebt;
        private LabelControl lblLimit;
        private DevExpress.XtraGrid.GridControl gridTransactions;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewTransactions;

        public CardsForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadCards();
        }

        private void InitializeServices()
        {
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var auditRepo = new AuditRepository(context);
            
            _cardService = new CardService(accountRepo, auditRepo);
            _cardService.CreateDemoCards(_customerId, "OMER CAN GUMUS");
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Title
            var lblTitle = new LabelControl();
            lblTitle.Text = "💳 Kartlarım";
            lblTitle.Location = new Point(30, 20);
            lblTitle.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;

            // Cards panel (horizontal scroll)
            this.pnlCards = new FlowLayoutPanel();
            this.pnlCards.Location = new Point(20, 80);
            this.pnlCards.Size = new Size(950, 260);
            this.pnlCards.AutoScroll = true;
            this.pnlCards.FlowDirection = FlowDirection.LeftToRight;
            this.pnlCards.WrapContents = false;
            this.pnlCards.BackColor = Color.Transparent;
            this.pnlCards.Padding = new Padding(10);

            // Card details panel
            this.pnlCardDetails = new Panel();
            this.pnlCardDetails.Location = new Point(20, 360);
            this.pnlCardDetails.Size = new Size(950, 320);
            this.pnlCardDetails.BackColor = Color.FromArgb(30, 32, 40);
            this.pnlCardDetails.Visible = false;

            // Labels for details
            this.lblDebt = new LabelControl();
            this.lblDebt.Location = new Point(30, 20);
            this.lblDebt.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblDebt.Appearance.ForeColor = Color.FromArgb(244, 67, 54);
            this.pnlCardDetails.Controls.Add(lblDebt);

            this.lblLimit = new LabelControl();
            this.lblLimit.Location = new Point(30, 55);
            this.lblLimit.Appearance.Font = new Font("Segoe UI", 12F);
            this.lblLimit.Appearance.ForeColor = Color.FromArgb(180, 180, 190);
            this.pnlCardDetails.Controls.Add(lblLimit);

            // Action buttons
            this.btnPayDebt = new SimpleButton();
            this.btnPayDebt.Text = "💵 Borç Öde";
            this.btnPayDebt.Location = new Point(30, 100);
            this.btnPayDebt.Size = new Size(150, 45);
            this.btnPayDebt.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnPayDebt.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnPayDebt.Appearance.ForeColor = Color.White;
            this.btnPayDebt.Appearance.Options.UseBackColor = true;
            this.btnPayDebt.Appearance.Options.UseForeColor = true;
            this.btnPayDebt.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnPayDebt.Click += BtnPayDebt_Click;
            this.pnlCardDetails.Controls.Add(btnPayDebt);

            this.btnSimulateSpend = new SimpleButton();
            this.btnSimulateSpend.Text = "🛒 Harcama Simüle";
            this.btnSimulateSpend.Location = new Point(190, 100);
            this.btnSimulateSpend.Size = new Size(170, 45);
            this.btnSimulateSpend.Appearance.Font = new Font("Segoe UI", 11F);
            this.btnSimulateSpend.Appearance.BackColor = Color.FromArgb(33, 150, 243);
            this.btnSimulateSpend.Appearance.ForeColor = Color.White;
            this.btnSimulateSpend.Appearance.Options.UseBackColor = true;
            this.btnSimulateSpend.Appearance.Options.UseForeColor = true;
            this.btnSimulateSpend.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnSimulateSpend.Click += BtnSimulateSpend_Click;
            this.pnlCardDetails.Controls.Add(btnSimulateSpend);

            // Transactions grid
            this.gridTransactions = new DevExpress.XtraGrid.GridControl();
            this.gridViewTransactions = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridTransactions.Location = new Point(30, 160);
            this.gridTransactions.Size = new Size(890, 140);
            this.gridTransactions.MainView = this.gridViewTransactions;
            this.gridViewTransactions.OptionsView.ShowGroupPanel = false;
            this.gridViewTransactions.OptionsBehavior.Editable = false;
            this.gridViewTransactions.Appearance.Row.Font = new Font("Segoe UI", 10F);
            this.gridViewTransactions.Appearance.Row.ForeColor = Color.White;
            this.pnlCardDetails.Controls.Add(gridTransactions);

            // New card button
            this.btnNewCard = new SimpleButton();
            this.btnNewCard.Text = "➕ Yeni Kart Oluştur";
            this.btnNewCard.Location = new Point(20, 700);
            this.btnNewCard.Size = new Size(200, 50);
            this.btnNewCard.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnNewCard.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            this.btnNewCard.Appearance.ForeColor = Color.White;
            this.btnNewCard.Appearance.Options.UseBackColor = true;
            this.btnNewCard.Appearance.Options.UseForeColor = true;
            this.btnNewCard.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnNewCard.Click += BtnNewCard_Click;

            // Form
            this.Controls.AddRange(new Control[] { lblTitle, pnlCards, pnlCardDetails, btnNewCard });
            this.ClientSize = new Size(1000, 770);
            this.Text = "💳 Kartlarım";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);
        }

        private void LoadCards()
        {
            pnlCards.Controls.Clear();
            var cards = _cardService.GetCustomerCards(_customerId);
            
            foreach (var card in cards)
            {
                var cardPanel = CreateCardVisual(card);
                pnlCards.Controls.Add(cardPanel);
            }
        }

        /// <summary>
        /// Görsel kredi kartı oluştur - Gradient arka plan, kabartmalı yazı
        /// </summary>
        private Panel CreateCardVisual(CreditCard card)
        {
            var panel = new Panel();
            panel.Size = new Size(320, 200);
            panel.Margin = new Padding(10);
            panel.Cursor = Cursors.Hand;
            panel.Tag = card;
            
            // Gradient colors based on theme
            Color color1, color2;
            switch (card.ColorTheme)
            {
                case "Gold":
                    color1 = Color.FromArgb(255, 193, 7);
                    color2 = Color.FromArgb(181, 137, 0);
                    break;
                case "Black":
                    color1 = Color.FromArgb(50, 50, 55);
                    color2 = Color.FromArgb(25, 25, 30);
                    break;
                case "Blue":
                    color1 = Color.FromArgb(33, 150, 243);
                    color2 = Color.FromArgb(21, 101, 192);
                    break;
                default: // Purple
                    color1 = Color.FromArgb(156, 39, 176);
                    color2 = Color.FromArgb(74, 20, 140);
                    break;
            }

            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Rounded rectangle path
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                var path = CreateRoundedRect(rect, 20);
                
                // Gradient background
                using (var brush = new LinearGradientBrush(rect, color1, color2, 45F))
                {
                    g.FillPath(brush, path);
                }
                
                // Border
                using (var pen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
                {
                    g.DrawPath(pen, path);
                }

                // Bank logo / name
                using (var font = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString("NOVABANK", font, brush, 20, 15);
                }

                // Card type badge
                using (var font = new Font("Segoe UI", 8F))
                using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                {
                    g.DrawString(card.CardType.ToUpper(), font, brush, 250, 18);
                }

                // Chip
                var chipRect = new Rectangle(25, 55, 50, 40);
                using (var brush = new LinearGradientBrush(chipRect, Color.FromArgb(218, 165, 32), Color.FromArgb(255, 215, 0), 45F))
                {
                    g.FillRoundedRectangle(brush, chipRect, 5);
                }
                
                // Chip lines
                using (var pen = new Pen(Color.FromArgb(150, 139, 90, 43), 1))
                {
                    g.DrawLine(pen, chipRect.X + 10, chipRect.Y + 10, chipRect.Right - 10, chipRect.Y + 10);
                    g.DrawLine(pen, chipRect.X + 10, chipRect.Y + 20, chipRect.Right - 10, chipRect.Y + 20);
                    g.DrawLine(pen, chipRect.X + 10, chipRect.Y + 30, chipRect.Right - 10, chipRect.Y + 30);
                }

                // Card number (masked, embossed look)
                using (var font = new Font("Consolas", 16F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
                {
                    g.DrawString(card.MaskedCardNumber, font, brush, 20, 110);
                }

                // Card holder name
                using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                {
                    g.DrawString(card.CardHolderName, font, brush, 20, 155);
                }

                // Expiry
                using (var font = new Font("Segoe UI", 9F))
                using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                {
                    g.DrawString("VALID THRU", font, brush, 200, 145);
                    g.DrawString(card.ExpiryDate.ToString("MM/yy"), font, brush, 200, 165);
                }
            };

            panel.Click += (s, e) => SelectCard(card);
            
            return panel;
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void SelectCard(CreditCard card)
        {
            _selectedCard = card;
            pnlCardDetails.Visible = true;
            
            lblDebt.Text = $"💰 Güncel Borç: {card.CurrentDebt:N2} ₺";
            lblLimit.Text = $"Kullanılabilir Limit: {card.AvailableLimit:N2} ₺ / Toplam: {card.TotalLimit:N2} ₺";
            
            // Load transactions
            var transactions = _cardService.GetCardTransactions(card.Id);
            gridTransactions.DataSource = transactions;
            gridViewTransactions.RefreshData();
        }

        private void BtnPayDebt_Click(object? sender, EventArgs e)
        {
            if (_selectedCard == null || _selectedCard.CurrentDebt <= 0)
            {
                XtraMessageBox.Show("Ödeme yapılacak borç yok!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var payForm = new PayDebtPopup(_selectedCard, _cardService))
            {
                if (payForm.ShowDialog() == DialogResult.OK)
                {
                    SelectCard(_selectedCard);
                    LoadCards();
                }
            }
        }

        private void BtnSimulateSpend_Click(object? sender, EventArgs e)
        {
            if (_selectedCard == null) return;

            var merchants = new[] { "Amazon.com.tr", "Migros", "Netflix", "Spotify", "Steam", "Trendyol", "Hepsiburada" };
            var random = new Random();
            var merchant = merchants[random.Next(merchants.Length)];
            var amount = random.Next(50, 500);

            var result = _cardService.SimulateSpending(_selectedCard.Id, amount, merchant);
            
            if (result.Success)
            {
                XtraMessageBox.Show(result.Message, "Harcama", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SelectCard(_selectedCard);
            }
            else
            {
                XtraMessageBox.Show(result.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNewCard_Click(object? sender, EventArgs e)
        {
            var result = XtraMessageBox.Show("Yeni kart türünü seçin:\n\nYES = Sanal Kart\nNO = Fiziksel Kart", 
                "Yeni Kart", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                _cardService.CreateVirtualCard(_customerId, "OMER CAN GUMUS", 15000);
                LoadCards();
            }
            else if (result == DialogResult.No)
            {
                _cardService.CreatePhysicalCard(_customerId, "OMER CAN GUMUS", 30000, "Gold");
                LoadCards();
            }
        }
    }

    /// <summary>
    /// Borç Ödeme Popup'ı
    /// </summary>
    public class PayDebtPopup : XtraForm
    {
        private CreditCard _card;
        private CardService _cardService;
        private LookUpEdit cmbAccount;
        private CalcEdit txtAmount;

        public PayDebtPopup(CreditCard card, CardService cardService)
        {
            _card = card;
            _cardService = cardService;
            InitializeComponent();
            LoadAccounts();
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            var lblTitle = new LabelControl();
            lblTitle.Text = "💵 Borç Öde";
            lblTitle.Location = new Point(25, 20);
            lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;

            var lblDebt = new LabelControl();
            lblDebt.Text = $"Ödenmesi Gereken: {_card.CurrentDebt:N2} ₺";
            lblDebt.Location = new Point(25, 65);
            lblDebt.Appearance.Font = new Font("Segoe UI", 12F);
            lblDebt.Appearance.ForeColor = Color.FromArgb(244, 67, 54);

            this.cmbAccount = new LookUpEdit();
            this.cmbAccount.Location = new Point(25, 110);
            this.cmbAccount.Size = new Size(250, 35);
            this.cmbAccount.Properties.NullText = "Hesap Seçin...";

            this.txtAmount = new CalcEdit();
            this.txtAmount.Location = new Point(25, 160);
            this.txtAmount.Size = new Size(250, 45);
            this.txtAmount.Value = _card.CurrentDebt;
            this.txtAmount.Properties.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.txtAmount.Properties.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            this.txtAmount.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.txtAmount.Properties.DisplayFormat.FormatString = "₺ #,##0.00";
            this.txtAmount.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;

            var btnPay = new SimpleButton();
            btnPay.Text = "✅ ÖDE";
            btnPay.Location = new Point(25, 230);
            btnPay.Size = new Size(120, 50);
            btnPay.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            btnPay.Appearance.ForeColor = Color.White;
            btnPay.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPay.Appearance.Options.UseBackColor = true;
            btnPay.Appearance.Options.UseForeColor = true;
            btnPay.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnPay.Click += BtnPay_Click;

            var btnCancel = new SimpleButton();
            btnCancel.Text = "İptal";
            btnCancel.Location = new Point(155, 230);
            btnCancel.Size = new Size(120, 50);
            btnCancel.Appearance.BackColor = Color.FromArgb(60, 60, 70);
            btnCancel.Appearance.ForeColor = Color.White;
            btnCancel.Appearance.Options.UseBackColor = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblTitle, lblDebt, cmbAccount, txtAmount, btnPay, btnCancel });
            
            this.ClientSize = new Size(300, 300);
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
        }

        private async void LoadAccounts()
        {
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var accounts = await accountRepo.GetAllAsync();
            
            cmbAccount.Properties.DataSource = accounts;
            cmbAccount.Properties.DisplayMember = "AccountNumber";
            cmbAccount.Properties.ValueMember = "Id";
        }

        private async void BtnPay_Click(object? sender, EventArgs e)
        {
            if (cmbAccount.EditValue == null)
            {
                XtraMessageBox.Show("Hesap seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int accountId = (int)cmbAccount.EditValue;
            decimal amount = txtAmount.Value;

            var result = await _cardService.PayDebtAsync(_card.Id, accountId, amount);
            
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

    // Extension method for rounded rectangle fill
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = new GraphicsPath())
            {
                int d = radius * 2;
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}
