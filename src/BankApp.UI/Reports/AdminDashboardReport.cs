using System;
using System.Drawing;
using DevExpress.XtraReports.UI;

namespace BankApp.UI.Reports
{
    public class AdminDashboardReport : XtraReport
    {
        private readonly AdminReportData _data;
        
        // Professional Color Palette
        private static readonly Color Primary = Color.FromArgb(79, 70, 229);      // Indigo
        private static readonly Color Success = Color.FromArgb(34, 197, 94);      // Green
        private static readonly Color Danger = Color.FromArgb(239, 68, 68);       // Red
        private static readonly Color Warning = Color.FromArgb(245, 158, 11);     // Amber
        private static readonly Color Info = Color.FromArgb(59, 130, 246);        // Blue
        private static readonly Color Dark = Color.FromArgb(17, 24, 39);
        private static readonly Color Gray600 = Color.FromArgb(75, 85, 99);
        private static readonly Color Gray400 = Color.FromArgb(156, 163, 175);
        private static readonly Color Gray200 = Color.FromArgb(229, 231, 235);
        private static readonly Color Gray100 = Color.FromArgb(243, 244, 246);
        private static readonly Color White = Color.White;

        public AdminDashboardReport(AdminReportData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            BuildReport();
        }

        private void BuildReport()
        {
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            this.Margins.Left = 40;
            this.Margins.Right = 40;
            this.Margins.Top = 40;
            this.Margins.Bottom = 40;

            var header = new ReportHeaderBand { HeightF = 100 };
            var detail = new DetailBand { HeightF = 720 };
            var footer = new PageFooterBand { HeightF = 25 };

            // ══════════════════════════════════════════════════════════════
            // HEADER
            // ══════════════════════════════════════════════════════════════
            
            // Purple gradient header bar
            var headerBar = new XRPanel
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(715, 80),
                BackColor = Primary,
                Borders = DevExpress.XtraPrinting.BorderSide.None
            };

            var lblLogo = new XRLabel
            {
                Text = "NOVABANK",
                LocationF = new PointF(20, 15),
                SizeF = new SizeF(200, 30),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = White
            };
            headerBar.Controls.Add(lblLogo);

            var lblSubtitle = new XRLabel
            {
                Text = "Admin Dashboard Report",
                LocationF = new PointF(20, 45),
                SizeF = new SizeF(250, 20),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(200, 200, 255)
            };
            headerBar.Controls.Add(lblSubtitle);

            var lblDate = new XRLabel
            {
                Text = _data.GeneratedAt.ToString("dd MMMM yyyy • HH:mm"),
                LocationF = new PointF(480, 25),
                SizeF = new SizeF(215, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = White,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            headerBar.Controls.Add(lblDate);

            header.Controls.Add(headerBar);

            // ══════════════════════════════════════════════════════════════
            // CONTENT
            // ══════════════════════════════════════════════════════════════
            float y = 15;

            // ─────────────────────────────────────────────────────────────
            // OVERVIEW STATS
            // ─────────────────────────────────────────────────────────────
            var lblOverview = new XRLabel
            {
                Text = "System Overview",
                LocationF = new PointF(0, y),
                SizeF = new SizeF(200, 22),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Dark
            };
            detail.Controls.Add(lblOverview);
            y += 30;

            // Stats cards row 1
            AddStatCard(detail, "Total Users", _data.TotalUsers.ToString("N0"), Info, 0, y);
            AddStatCard(detail, "Active Users", _data.ActiveUsers.ToString("N0"), Success, 180, y);
            AddStatCard(detail, "Admin Users", _data.AdminUsers.ToString("N0"), Primary, 360, y);
            AddStatCard(detail, "Banned Users", _data.BannedUsers.ToString("N0"), Danger, 540, y);
            y += 75;

            // ─────────────────────────────────────────────────────────────
            // LOAN STATISTICS
            // ─────────────────────────────────────────────────────────────
            var lblLoans = new XRLabel
            {
                Text = "Loan Statistics",
                LocationF = new PointF(0, y),
                SizeF = new SizeF(200, 22),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Dark
            };
            detail.Controls.Add(lblLoans);
            y += 30;

            AddStatCard(detail, "Total Loans", _data.TotalLoans.ToString("N0"), Info, 0, y);
            AddStatCard(detail, "Pending", _data.PendingLoans.ToString("N0"), Warning, 180, y);
            AddStatCard(detail, "Approved", _data.ApprovedLoans.ToString("N0"), Success, 360, y);
            AddStatCard(detail, "Rejected", _data.RejectedLoans.ToString("N0"), Danger, 540, y);
            y += 75;

            // Total Loan Amount
            var loanAmountCard = new XRPanel
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(350, 60),
                BackColor = Gray100,
                BorderColor = Gray200,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            var lblAmountTitle = new XRLabel
            {
                Text = "Total Loan Amount",
                LocationF = new PointF(15, 10),
                SizeF = new SizeF(200, 18),
                Font = new Font("Segoe UI", 9),
                ForeColor = Gray600
            };
            loanAmountCard.Controls.Add(lblAmountTitle);

            var lblAmount = new XRLabel
            {
                Text = $"₺{_data.TotalLoanAmount:N2}",
                LocationF = new PointF(15, 30),
                SizeF = new SizeF(320, 25),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Primary
            };
            loanAmountCard.Controls.Add(lblAmount);

            detail.Controls.Add(loanAmountCard);

            // Pending Amount
            var pendingAmountCard = new XRPanel
            {
                LocationF = new PointF(365, y),
                SizeF = new SizeF(350, 60),
                BackColor = Color.FromArgb(254, 252, 232),
                BorderColor = Warning,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            var lblPendingTitle = new XRLabel
            {
                Text = "Pending Loan Amount",
                LocationF = new PointF(15, 10),
                SizeF = new SizeF(200, 18),
                Font = new Font("Segoe UI", 9),
                ForeColor = Gray600
            };
            pendingAmountCard.Controls.Add(lblPendingTitle);

            var lblPending = new XRLabel
            {
                Text = $"₺{_data.PendingLoanAmount:N2}",
                LocationF = new PointF(15, 30),
                SizeF = new SizeF(320, 25),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Warning
            };
            pendingAmountCard.Controls.Add(lblPending);

            detail.Controls.Add(pendingAmountCard);
            y += 80;

            // ─────────────────────────────────────────────────────────────
            // TRANSACTION SUMMARY
            // ─────────────────────────────────────────────────────────────
            var lblTrans = new XRLabel
            {
                Text = "Transaction Summary",
                LocationF = new PointF(0, y),
                SizeF = new SizeF(200, 22),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Dark
            };
            detail.Controls.Add(lblTrans);
            y += 30;

            AddStatCard(detail, "Total Transactions", _data.TotalTransactions.ToString("N0"), Info, 0, y);
            AddStatCard(detail, "Today", _data.TodayTransactions.ToString("N0"), Success, 180, y);
            AddStatCard(detail, "This Week", _data.WeekTransactions.ToString("N0"), Primary, 360, y);
            AddStatCard(detail, "This Month", _data.MonthTransactions.ToString("N0"), Gray600, 540, y);
            y += 85;

            // ─────────────────────────────────────────────────────────────
            // SYSTEM INFO
            // ─────────────────────────────────────────────────────────────
            var sysPanel = new XRPanel
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(715, 80),
                BackColor = Gray100,
                BorderColor = Gray200,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            var lblSysTitle = new XRLabel
            {
                Text = "System Information",
                LocationF = new PointF(15, 10),
                SizeF = new SizeF(200, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Dark
            };
            sysPanel.Controls.Add(lblSysTitle);

            var lblSysInfo = new XRLabel
            {
                Text = $"Report Generated: {_data.GeneratedAt:dd.MM.yyyy HH:mm:ss}\nGenerated By: {_data.GeneratedBy}\nDatabase: {_data.DatabaseStatus}",
                LocationF = new PointF(15, 32),
                SizeF = new SizeF(680, 45),
                Font = new Font("Segoe UI", 9),
                ForeColor = Gray600,
                Multiline = true
            };
            sysPanel.Controls.Add(lblSysInfo);

            detail.Controls.Add(sysPanel);
            y += 95;

            // ─────────────────────────────────────────────────────────────
            // NOTES
            // ─────────────────────────────────────────────────────────────
            if (!string.IsNullOrEmpty(_data.Notes))
            {
                var notesPanel = new XRPanel
                {
                    LocationF = new PointF(0, y),
                    SizeF = new SizeF(715, 60),
                    BackColor = Color.FromArgb(239, 246, 255),
                    BorderColor = Info,
                    Borders = DevExpress.XtraPrinting.BorderSide.All
                };

                var lblNotesTitle = new XRLabel
                {
                    Text = "Notes",
                    LocationF = new PointF(15, 8),
                    SizeF = new SizeF(100, 18),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Info
                };
                notesPanel.Controls.Add(lblNotesTitle);

                var lblNotes = new XRLabel
                {
                    Text = _data.Notes,
                    LocationF = new PointF(15, 28),
                    SizeF = new SizeF(680, 28),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Gray600,
                    Multiline = true,
                    WordWrap = true
                };
                notesPanel.Controls.Add(lblNotes);

                detail.Controls.Add(notesPanel);
            }

            // ══════════════════════════════════════════════════════════════
            // FOOTER
            // ══════════════════════════════════════════════════════════════
            var lblFooter = new XRLabel
            {
                Text = "© 2026 NovaBank • Confidential - For Internal Use Only",
                LocationF = new PointF(0, 5),
                SizeF = new SizeF(400, 18),
                Font = new Font("Segoe UI", 8),
                ForeColor = Gray400
            };
            footer.Controls.Add(lblFooter);

            var pageNum = new XRPageInfo
            {
                LocationF = new PointF(550, 5),
                SizeF = new SizeF(165, 18),
                Font = new Font("Segoe UI", 8),
                ForeColor = Gray400,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight,
                Format = "Page {0} of {1}"
            };
            footer.Controls.Add(pageNum);

            // Add bands
            this.Bands.Add(header);
            this.Bands.Add(detail);
            this.Bands.Add(footer);
        }

        private void AddStatCard(DetailBand band, string label, string value, Color accent, float x, float y)
        {
            var card = new XRPanel
            {
                LocationF = new PointF(x, y),
                SizeF = new SizeF(170, 60),
                BackColor = White,
                BorderColor = Gray200,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            // Color accent bar on left
            var accentBar = new XRPanel
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(4, 60),
                BackColor = accent,
                Borders = DevExpress.XtraPrinting.BorderSide.None
            };
            card.Controls.Add(accentBar);

            var lblLabel = new XRLabel
            {
                Text = label,
                LocationF = new PointF(12, 10),
                SizeF = new SizeF(150, 16),
                Font = new Font("Segoe UI", 9),
                ForeColor = Gray600
            };
            card.Controls.Add(lblLabel);

            var lblValue = new XRLabel
            {
                Text = value,
                LocationF = new PointF(12, 30),
                SizeF = new SizeF(150, 24),
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Dark
            };
            card.Controls.Add(lblValue);

            band.Controls.Add(card);
        }
    }

    // Data model for Admin Report
    public class AdminReportData
    {
        // User Statistics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int BannedUsers { get; set; }
        
        // Loan Statistics
        public int TotalLoans { get; set; }
        public int PendingLoans { get; set; }
        public int ApprovedLoans { get; set; }
        public int RejectedLoans { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal PendingLoanAmount { get; set; }
        
        // Transaction Statistics
        public int TotalTransactions { get; set; }
        public int TodayTransactions { get; set; }
        public int WeekTransactions { get; set; }
        public int MonthTransactions { get; set; }
        
        // System Info
        public string GeneratedBy { get; set; } = "Admin";
        public string DatabaseStatus { get; set; } = "Connected";
        public string Notes { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
