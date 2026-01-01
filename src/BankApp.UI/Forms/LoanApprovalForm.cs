#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.LookAndFeel;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Kredi Onay Formu - Admin için
    /// </summary>
    public class LoanApprovalForm : XtraForm
    {
        private LoanService _loanService;
        private GridControl gridLoans;
        private GridView gridViewLoans;
        private SimpleButton btnApprove;
        private SimpleButton btnReject;
        private SimpleButton btnRefresh;

        public LoanApprovalForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadPendingLoans();
        }

        private void InitializeServices()
        {
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var auditRepo = new AuditRepository(context);
            _loanService = new LoanService(accountRepo, auditRepo);
            _loanService.CreateDemoLoans(); // Demo verisi
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Title
            var lblTitle = new LabelControl();
            lblTitle.Text = "🏦 Kredi Onay Paneli";
            lblTitle.Location = new Point(25, 20);
            lblTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;

            var lblSubtitle = new LabelControl();
            lblSubtitle.Text = "Bekleyen kredi başvurularını onaylayın veya reddedin";
            lblSubtitle.Location = new Point(25, 60);
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 10F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(150, 150, 160);

            // Grid
            this.gridLoans = new GridControl();
            this.gridViewLoans = new GridView();
            this.gridLoans.Location = new Point(25, 100);
            this.gridLoans.Size = new Size(700, 350);
            this.gridLoans.MainView = this.gridViewLoans;

            this.gridViewLoans.GridControl = this.gridLoans;
            this.gridViewLoans.OptionsView.ShowGroupPanel = false;
            this.gridViewLoans.OptionsBehavior.Editable = false;
            this.gridViewLoans.Appearance.Row.Font = new Font("Segoe UI", 10F);
            this.gridViewLoans.Appearance.Row.ForeColor = Color.White;
            this.gridViewLoans.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 10F);
            this.gridViewLoans.Appearance.HeaderPanel.ForeColor = Color.FromArgb(180, 180, 190);

            // Columns
            this.gridViewLoans.Columns.AddVisible("Id", "ID");
            this.gridViewLoans.Columns.AddVisible("Amount", "Tutar");
            this.gridViewLoans.Columns.AddVisible("TermMonths", "Vade (Ay)");
            this.gridViewLoans.Columns.AddVisible("InterestRate", "Faiz %");
            this.gridViewLoans.Columns.AddVisible("MonthlyPayment", "Aylık Taksit");
            this.gridViewLoans.Columns.AddVisible("ApplicationDate", "Başvuru Tarihi");
            this.gridViewLoans.Columns.AddVisible("Notes", "Not");

            this.gridViewLoans.Columns["Amount"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridViewLoans.Columns["Amount"].DisplayFormat.FormatString = "₺ #,##0";
            this.gridViewLoans.Columns["MonthlyPayment"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridViewLoans.Columns["MonthlyPayment"].DisplayFormat.FormatString = "₺ #,##0.00";
            this.gridViewLoans.Columns["ApplicationDate"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.gridViewLoans.Columns["ApplicationDate"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";

            // Buttons
            this.btnApprove = new SimpleButton();
            this.btnApprove.Text = "✅ ONAYLA";
            this.btnApprove.Location = new Point(25, 470);
            this.btnApprove.Size = new Size(150, 50);
            this.btnApprove.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnApprove.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.btnApprove.Appearance.ForeColor = Color.White;
            this.btnApprove.Appearance.Options.UseBackColor = true;
            this.btnApprove.Appearance.Options.UseForeColor = true;
            this.btnApprove.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnApprove.Click += BtnApprove_Click;

            this.btnReject = new SimpleButton();
            this.btnReject.Text = "❌ REDDET";
            this.btnReject.Location = new Point(185, 470);
            this.btnReject.Size = new Size(150, 50);
            this.btnReject.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnReject.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            this.btnReject.Appearance.ForeColor = Color.White;
            this.btnReject.Appearance.Options.UseBackColor = true;
            this.btnReject.Appearance.Options.UseForeColor = true;
            this.btnReject.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnReject.Click += BtnReject_Click;

            this.btnRefresh = new SimpleButton();
            this.btnRefresh.Text = "🔄 Yenile";
            this.btnRefresh.Location = new Point(575, 470);
            this.btnRefresh.Size = new Size(150, 50);
            this.btnRefresh.Appearance.Font = new Font("Segoe UI", 11F);
            this.btnRefresh.Appearance.BackColor = Color.FromArgb(50, 50, 60);
            this.btnRefresh.Appearance.ForeColor = Color.White;
            this.btnRefresh.Appearance.Options.UseBackColor = true;
            this.btnRefresh.Appearance.Options.UseForeColor = true;
            this.btnRefresh.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnRefresh.Click += (s, e) => LoadPendingLoans();

            // Stats
            var pnlStats = new Panel();
            pnlStats.Location = new Point(25, 540);
            pnlStats.Size = new Size(700, 40);
            pnlStats.BackColor = Color.FromArgb(30, 32, 40);

            var lblStats = new LabelControl();
            lblStats.Name = "lblStats";
            lblStats.Location = new Point(15, 10);
            lblStats.Appearance.Font = new Font("Segoe UI", 11F);
            lblStats.Appearance.ForeColor = Color.FromArgb(180, 180, 190);
            pnlStats.Controls.Add(lblStats);

            // Form
            this.Controls.AddRange(new Control[] { 
                lblTitle, lblSubtitle, gridLoans,
                btnApprove, btnReject, btnRefresh, pnlStats
            });
            
            this.ClientSize = new Size(750, 600);
            this.Text = "Kredi Onay Paneli";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void LoadPendingLoans()
        {
            var loans = _loanService.GetPendingLoans();
            gridLoans.DataSource = loans;
            gridViewLoans.RefreshData();

            // Stats
            var lblStats = this.Controls.Find("lblStats", true);
            if (lblStats.Length > 0 && lblStats[0] is LabelControl lbl)
            {
                decimal totalPending = 0;
                foreach (var loan in loans) totalPending += loan.Amount;
                lbl.Text = $"Bekleyen: {loans.Count} başvuru | Toplam: {totalPending:N2} ₺";
            }
        }

        private async void BtnApprove_Click(object? sender, EventArgs e)
        {
            var row = gridViewLoans.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show("Lütfen bir başvuru seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var loanId = (int)gridViewLoans.GetRowCellValue(row, "Id");
            
            var confirm = XtraMessageBox.Show("Bu krediyi onaylamak istediğinize emin misiniz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                // LoanService artık string döndürüyor: null = başarılı, string = hata mesajı
                string result = await _loanService.ApproveLoanAsync(loanId, AppEvents.CurrentSession.UserId);
                
                if (result == null)
                {
                    XtraMessageBox.Show("Kredi onaylandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPendingLoans();
                }
                else
                {
                    XtraMessageBox.Show(result, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnReject_Click(object? sender, EventArgs e)
        {
            var row = gridViewLoans.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show("Lütfen bir başvuru seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var loanId = (int)gridViewLoans.GetRowCellValue(row, "Id");
            
            string reason = XtraInputBox.Show("Red nedeni girin:", "Kredi Reddi", "");
            
            if (!string.IsNullOrEmpty(reason))
            {
                // LoanService artık string döndürüyor: null = başarılı, string = hata mesajı
                string result = await _loanService.RejectLoanAsync(loanId, AppEvents.CurrentSession.UserId, reason);
                
                if (result == null)
                {
                    XtraMessageBox.Show("Kredi başvurusu reddedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPendingLoans();
                }
                else
                {
                    XtraMessageBox.Show(result, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
