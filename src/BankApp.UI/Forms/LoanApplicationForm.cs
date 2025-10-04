#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.LookAndFeel;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Kredi Başvuru Formu - User için
    /// </summary>
    public class LoanApplicationForm : XtraForm
    {
        private LoanService _loanService;
        
        private CalcEdit txtAmount;
        private SpinEdit spinTerm;
        private MemoEdit txtNotes;
        private LabelControl lblMonthlyPayment;
        private LabelControl lblTotalPayment;
        private SimpleButton btnApply;
        private SimpleButton btnCancel;

        public LoanApplicationForm()
        {
            InitializeComponent();
            InitializeServices();
            UpdateCalculation();
        }

        private void InitializeServices()
        {
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var auditRepo = new AuditRepository(context);
            _loanService = new LoanService(accountRepo, auditRepo);
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Title
            var lblTitle = new LabelControl();
            lblTitle.Text = "💰 Kredi Başvurusu";
            lblTitle.Location = new Point(30, 25);
            lblTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;

            var lblSubtitle = new LabelControl();
            lblSubtitle.Text = "Hızlı ve kolay kredi başvurusu";
            lblSubtitle.Location = new Point(30, 65);
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 10F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(150, 150, 160);

            // Amount
            var lblAmountTitle = new LabelControl();
            lblAmountTitle.Text = "💵 Talep Edilen Tutar";
            lblAmountTitle.Location = new Point(30, 110);
            lblAmountTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblAmountTitle.Appearance.ForeColor = Color.White;

            this.txtAmount = new CalcEdit();
            this.txtAmount.Location = new Point(30, 140);
            this.txtAmount.Size = new Size(320, 55);
            this.txtAmount.Value = 10000;
            this.txtAmount.Properties.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            this.txtAmount.Properties.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            this.txtAmount.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.txtAmount.Properties.DisplayFormat.FormatString = "₺ #,##0";
            this.txtAmount.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.txtAmount.EditValueChanged += (s, e) => UpdateCalculation();

            // Term
            var lblTermTitle = new LabelControl();
            lblTermTitle.Text = "📅 Vade (Ay)";
            lblTermTitle.Location = new Point(30, 210);
            lblTermTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblTermTitle.Appearance.ForeColor = Color.White;

            this.spinTerm = new SpinEdit();
            this.spinTerm.Location = new Point(30, 240);
            this.spinTerm.Size = new Size(150, 45);
            this.spinTerm.Value = 12;
            this.spinTerm.Properties.MinValue = 3;
            this.spinTerm.Properties.MaxValue = 60;
            this.spinTerm.Properties.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.spinTerm.Properties.Appearance.ForeColor = Color.White;
            this.spinTerm.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.spinTerm.EditValueChanged += (s, e) => UpdateCalculation();

            // Notes
            var lblNotesTitle = new LabelControl();
            lblNotesTitle.Text = "📝 Başvuru Notu (Opsiyonel)";
            lblNotesTitle.Location = new Point(30, 300);
            lblNotesTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblNotesTitle.Appearance.ForeColor = Color.White;

            this.txtNotes = new MemoEdit();
            this.txtNotes.Location = new Point(30, 330);
            this.txtNotes.Size = new Size(320, 80);
            this.txtNotes.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            this.txtNotes.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.txtNotes.Properties.Appearance.ForeColor = Color.White;

            // Payment info panel
            var pnlInfo = new Panel();
            pnlInfo.Location = new Point(30, 430);
            pnlInfo.Size = new Size(320, 100);
            pnlInfo.BackColor = Color.FromArgb(35, 38, 48);
            pnlInfo.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(76, 175, 80), 2))
                {
                    var rect = new Rectangle(1, 1, pnlInfo.Width - 3, pnlInfo.Height - 3);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            this.lblMonthlyPayment = new LabelControl();
            this.lblMonthlyPayment.Location = new Point(15, 15);
            this.lblMonthlyPayment.Appearance.Font = new Font("Segoe UI", 11F);
            this.lblMonthlyPayment.Appearance.ForeColor = Color.FromArgb(180, 180, 190);
            pnlInfo.Controls.Add(lblMonthlyPayment);

            this.lblTotalPayment = new LabelControl();
            this.lblTotalPayment.Location = new Point(15, 55);
            this.lblTotalPayment.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblTotalPayment.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            pnlInfo.Controls.Add(lblTotalPayment);

            // Buttons
            this.btnApply = new SimpleButton();
            this.btnApply.Text = "✅ BAŞVUR";
            this.btnApply.Location = new Point(30, 550);
            this.btnApply.Size = new Size(155, 50);
            this.btnApply.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnApply.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnApply.Appearance.ForeColor = Color.White;
            this.btnApply.Appearance.Options.UseBackColor = true;
            this.btnApply.Appearance.Options.UseForeColor = true;
            this.btnApply.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnApply.Click += BtnApply_Click;

            this.btnCancel = new SimpleButton();
            this.btnCancel.Text = "İptal";
            this.btnCancel.Location = new Point(195, 550);
            this.btnCancel.Size = new Size(155, 50);
            this.btnCancel.Appearance.Font = new Font("Segoe UI", 12F);
            this.btnCancel.Appearance.BackColor = Color.FromArgb(60, 60, 70);
            this.btnCancel.Appearance.ForeColor = Color.White;
            this.btnCancel.Appearance.Options.UseBackColor = true;
            this.btnCancel.Appearance.Options.UseForeColor = true;
            this.btnCancel.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnCancel.Click += (s, e) => this.Close();

            // Form
            this.Controls.AddRange(new Control[] { 
                lblTitle, lblSubtitle, lblAmountTitle, txtAmount,
                lblTermTitle, spinTerm, lblNotesTitle, txtNotes,
                pnlInfo, btnApply, btnCancel
            });
            
            this.ClientSize = new Size(380, 620);
            this.Text = "Kredi Başvurusu";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void UpdateCalculation()
        {
            decimal amount = txtAmount.Value;
            int term = (int)spinTerm.Value;
            
            // Faiz oranı
            decimal rate = term switch
            {
                <= 12 => 3.0m,
                <= 24 => 3.5m,
                <= 36 => 4.0m,
                _ => 4.5m
            };

            decimal totalInterest = amount * (rate / 100) * (term / 12m);
            decimal total = amount + totalInterest;
            decimal monthly = total / term;

            lblMonthlyPayment.Text = $"Aylık Taksit: {monthly:N2} ₺ (Faiz: %{rate:N1})";
            lblTotalPayment.Text = $"Toplam Geri Ödeme: {total:N2} ₺";
        }

        private async void BtnApply_Click(object? sender, EventArgs e)
        {
            decimal amount = txtAmount.Value;
            int term = (int)spinTerm.Value;
            string notes = txtNotes.Text;

            var result = await _loanService.ApplyForLoanAsync(
                AppEvents.CurrentSession.UserId,
                1, // Demo customer ID
                amount,
                term,
                notes
            );

            if (result.Success)
            {
                XtraMessageBox.Show(result.Message, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                XtraMessageBox.Show(result.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
