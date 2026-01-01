#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.LookAndFeel;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Vadeli Hesap Formu - Faiz Hesaplayıcı
    /// </summary>
    public class TimeDepositForm : XtraForm
    {
        private int _customerId = 1;
        
        // UI Controls
        private CalcEdit txtPrincipal;
        private TrackBarControl trackDays;
        private LabelControl lblDays;
        private LabelControl lblInterestRate;
        private LabelControl lblEstimatedReturn;
        private LabelControl lblTotal;
        private LookUpEdit cmbAccount;
        private SimpleButton btnOpen;

        // Interest rates by term
        private readonly (int MinDays, int MaxDays, decimal Rate)[] _interestRates = new[]
        {
            (32, 92, 42.0m),
            (93, 180, 44.0m),
            (181, 270, 46.0m),
            (271, 365, 48.0m)
        };

        public TimeDepositForm()
        {
            InitializeComponent();
            LoadAccounts();
            UpdateCalculation();
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Title
            var lblTitle = new LabelControl();
            lblTitle.Text = "🏦 Vadeli Hesap Aç";
            lblTitle.Location = new Point(30, 25);
            lblTitle.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;

            var lblSubtitle = new LabelControl();
            lblSubtitle.Text = "Paranızı vadeli hesapta değerlendirin";
            lblSubtitle.Location = new Point(30, 70);
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 11F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(150, 150, 160);

            // Account selection
            var lblAccountTitle = new LabelControl();
            lblAccountTitle.Text = "💰 Kaynak Hesap";
            lblAccountTitle.Location = new Point(30, 120);
            lblAccountTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F);
            lblAccountTitle.Appearance.ForeColor = Color.White;

            this.cmbAccount = new LookUpEdit();
            this.cmbAccount.Location = new Point(30, 150);
            this.cmbAccount.Size = new Size(350, 40);
            this.cmbAccount.Properties.Appearance.Font = new Font("Segoe UI", 12F);
            this.cmbAccount.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.cmbAccount.Properties.Appearance.ForeColor = Color.White;
            this.cmbAccount.Properties.NullText = "Hesap Seçin...";

            // Principal amount
            var lblPrincipalTitle = new LabelControl();
            lblPrincipalTitle.Text = "💵 Yatırılacak Tutar";
            lblPrincipalTitle.Location = new Point(30, 210);
            lblPrincipalTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F);
            lblPrincipalTitle.Appearance.ForeColor = Color.White;

            this.txtPrincipal = new CalcEdit();
            this.txtPrincipal.Location = new Point(30, 240);
            this.txtPrincipal.Size = new Size(350, 60);
            this.txtPrincipal.Value = 10000;
            this.txtPrincipal.Properties.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            this.txtPrincipal.Properties.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            this.txtPrincipal.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.txtPrincipal.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtPrincipal.Properties.DisplayFormat.FormatString = "₺ #,##0";
            this.txtPrincipal.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.txtPrincipal.Properties.Buttons.Clear();
            this.txtPrincipal.EditValueChanged += (s, e) => UpdateCalculation();

            // Term days slider
            var lblTermTitle = new LabelControl();
            lblTermTitle.Text = "📅 Vade Süresi";
            lblTermTitle.Location = new Point(30, 320);
            lblTermTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F);
            lblTermTitle.Appearance.ForeColor = Color.White;

            this.trackDays = new TrackBarControl();
            this.trackDays.Location = new Point(30, 350);
            this.trackDays.Size = new Size(350, 45);
            this.trackDays.Properties.Minimum = 32;
            this.trackDays.Properties.Maximum = 365;
            this.trackDays.Value = 92;
            this.trackDays.Properties.SmallChange = 1;
            this.trackDays.Properties.LargeChange = 30;
            this.trackDays.EditValueChanged += (s, e) => UpdateCalculation();

            this.lblDays = new LabelControl();
            this.lblDays.Location = new Point(30, 400);
            this.lblDays.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblDays.Appearance.ForeColor = Color.FromArgb(33, 150, 243);

            // Interest rate display
            this.lblInterestRate = new LabelControl();
            this.lblInterestRate.Location = new Point(200, 400);
            this.lblInterestRate.Appearance.Font = new Font("Segoe UI", 12F);
            this.lblInterestRate.Appearance.ForeColor = Color.FromArgb(180, 180, 190);

            // Result card
            var pnlResult = new Panel();
            pnlResult.Location = new Point(30, 450);
            pnlResult.Size = new Size(350, 150);
            pnlResult.BackColor = Color.FromArgb(35, 38, 48);
            pnlResult.Paint += (s, e) => {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(76, 175, 80), 2))
                {
                    var rect = new Rectangle(1, 1, pnlResult.Width - 3, pnlResult.Height - 3);
                    var path = CreateRoundedRect(rect, 15);
                    g.DrawPath(pen, path);
                }
            };

            var lblReturnTitle = new LabelControl();
            lblReturnTitle.Text = "📈 Tahmini Getiri";
            lblReturnTitle.Location = new Point(20, 15);
            lblReturnTitle.Appearance.Font = new Font("Segoe UI", 11F);
            lblReturnTitle.Appearance.ForeColor = Color.FromArgb(180, 180, 190);
            pnlResult.Controls.Add(lblReturnTitle);

            this.lblEstimatedReturn = new LabelControl();
            this.lblEstimatedReturn.Location = new Point(20, 45);
            this.lblEstimatedReturn.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            this.lblEstimatedReturn.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            pnlResult.Controls.Add(lblEstimatedReturn);

            var lblTotalTitle = new LabelControl();
            lblTotalTitle.Text = "Vade Sonunda Toplam:";
            lblTotalTitle.Location = new Point(20, 95);
            lblTotalTitle.Appearance.Font = new Font("Segoe UI", 10F);
            lblTotalTitle.Appearance.ForeColor = Color.FromArgb(150, 150, 160);
            pnlResult.Controls.Add(lblTotalTitle);

            this.lblTotal = new LabelControl();
            this.lblTotal.Location = new Point(20, 115);
            this.lblTotal.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblTotal.Appearance.ForeColor = Color.White;
            pnlResult.Controls.Add(lblTotal);

            // Open button
            this.btnOpen = new SimpleButton();
            this.btnOpen.Text = "✅ VADELİ HESAP AÇ";
            this.btnOpen.Location = new Point(30, 620);
            this.btnOpen.Size = new Size(350, 55);
            this.btnOpen.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.btnOpen.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnOpen.Appearance.ForeColor = Color.White;
            this.btnOpen.Appearance.Options.UseBackColor = true;
            this.btnOpen.Appearance.Options.UseForeColor = true;
            this.btnOpen.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnOpen.Click += BtnOpen_Click;

            // Form
            this.Controls.AddRange(new Control[] { 
                lblTitle, lblSubtitle, lblAccountTitle, cmbAccount,
                lblPrincipalTitle, txtPrincipal, lblTermTitle, trackDays,
                lblDays, lblInterestRate, pnlResult, btnOpen
            });
            
            this.ClientSize = new Size(420, 700);
            this.Text = "🏦 Vadeli Hesap";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
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

        private void UpdateCalculation()
        {
            int days = trackDays.Value;
            decimal principal = txtPrincipal.Value;
            
            // Find interest rate for term
            decimal rate = 40.0m;
            foreach (var r in _interestRates)
            {
                if (days >= r.MinDays && days <= r.MaxDays)
                {
                    rate = r.Rate;
                    break;
                }
            }

            // Calculate return (simple interest)
            decimal estimatedReturn = principal * (rate / 100) * days / 365;
            decimal total = principal + estimatedReturn;

            // Update labels
            lblDays.Text = $"{days} Gün";
            lblInterestRate.Text = $"Faiz: %{rate:N1}";
            lblEstimatedReturn.Text = $"+ {estimatedReturn:N2} ₺";
            lblTotal.Text = $"{total:N2} ₺";
        }

        private async void BtnOpen_Click(object? sender, EventArgs e)
        {
            if (cmbAccount.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen kaynak hesap seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal principal = txtPrincipal.Value;
            if (principal < 1000)
            {
                XtraMessageBox.Show("Minimum 1.000 ₺ yatırım yapmalısınız!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int accountId = (int)cmbAccount.EditValue;
            int days = trackDays.Value;

            // Check balance
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var account = await accountRepo.GetByIdAsync(accountId);

            if (account == null || account.Balance < principal)
            {
                XtraMessageBox.Show("Yetersiz bakiye!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Deduct from account
            account.Balance -= principal;
            await accountRepo.UpdateAsync(account);

            // Find rate
            decimal rate = 40.0m;
            foreach (var r in _interestRates)
            {
                if (days >= r.MinDays && days <= r.MaxDays)
                {
                    rate = r.Rate;
                    break;
                }
            }

            // Create time deposit (in real app, would save to DB)
            var deposit = TimeDepositAccount.Create(_customerId, accountId, principal, days, rate);

            XtraMessageBox.Show(
                $"Vadeli hesap açıldı!\n\n" +
                $"Ana Para: {principal:N2} ₺\n" +
                $"Vade: {days} gün\n" +
                $"Faiz: %{rate:N1}\n" +
                $"Tahmini Getiri: {deposit.EstimatedReturn:N2} ₺\n" +
                $"Vade Sonu: {deposit.MaturityDate:dd.MM.yyyy}",
                "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }
    }
}
