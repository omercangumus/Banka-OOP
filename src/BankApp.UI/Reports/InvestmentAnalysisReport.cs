using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using BankApp.UI.Services.Pdf;

namespace BankApp.UI.Reports
{
    public class InvestmentAnalysisReport : XtraReport
    {
        private readonly InvestmentAnalysisData _data;
        
        // Clean Professional White Theme
        private static readonly Color Primary = Color.FromArgb(37, 99, 235);      // Blue
        private static readonly Color Secondary = Color.FromArgb(99, 102, 241);   // Indigo
        private static readonly Color Success = Color.FromArgb(34, 197, 94);      // Green
        private static readonly Color Danger = Color.FromArgb(239, 68, 68);       // Red
        private static readonly Color Warning = Color.FromArgb(234, 179, 8);      // Yellow
        private static readonly Color Dark = Color.FromArgb(17, 24, 39);          // Almost black
        private static readonly Color Gray600 = Color.FromArgb(75, 85, 99);
        private static readonly Color Gray400 = Color.FromArgb(156, 163, 175);
        private static readonly Color Gray200 = Color.FromArgb(229, 231, 235);
        private static readonly Color Gray100 = Color.FromArgb(243, 244, 246);
        private static readonly Color White = Color.White;

        public InvestmentAnalysisReport(InvestmentAnalysisData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            BuildReport();
        }

        private void BuildReport()
        {
            // Page Setup
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            this.Margins.Left = 50;
            this.Margins.Right = 50;
            this.Margins.Top = 40;
            this.Margins.Bottom = 40;

            var header = new ReportHeaderBand { HeightF = 85 };
            var detail = new DetailBand { HeightF = 700 };
            var footer = new PageFooterBand { HeightF = 25 };

            // ══════════════════════════════════════════════════════════════
            // HEADER
            // ══════════════════════════════════════════════════════════════
            
            // Blue accent bar at top
            var topBar = new XRPanel
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(695, 5),
                BackColor = Primary,
                Borders = DevExpress.XtraPrinting.BorderSide.None
            };
            header.Controls.Add(topBar);

            // Logo
            var lblLogo = new XRLabel
            {
                Text = "NOVABANK",
                LocationF = new PointF(0, 15),
                SizeF = new SizeF(200, 30),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Primary
            };
            header.Controls.Add(lblLogo);

            var lblTagline = new XRLabel
            {
                Text = "Investment Analysis Report",
                LocationF = new PointF(0, 45),
                SizeF = new SizeF(250, 18),
                Font = new Font("Segoe UI", 10),
                ForeColor = Gray600
            };
            header.Controls.Add(lblTagline);

            // Date & Report ID (right)
            var lblDate = new XRLabel
            {
                Text = _data.GeneratedAt.ToString("dd MMMM yyyy"),
                LocationF = new PointF(500, 20),
                SizeF = new SizeF(195, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Gray600,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            header.Controls.Add(lblDate);

            var lblTime = new XRLabel
            {
                Text = _data.GeneratedAt.ToString("HH:mm:ss"),
                LocationF = new PointF(500, 40),
                SizeF = new SizeF(195, 18),
                Font = new Font("Segoe UI", 9),
                ForeColor = Gray400,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            header.Controls.Add(lblTime);

            // Divider line
            var divider = new XRLine
            {
                LocationF = new PointF(0, 75),
                SizeF = new SizeF(695, 2),
                ForeColor = Gray200
            };
            header.Controls.Add(divider);

            // ══════════════════════════════════════════════════════════════
            // DETAIL CONTENT
            // ══════════════════════════════════════════════════════════════
            float y = 10;

            // ─────────────────────────────────────────────────────────────
            // SYMBOL HERO SECTION
            // ─────────────────────────────────────────────────────────────
            var heroPanel = new XRPanel
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(695, 90),
                BackColor = Gray100,
                BorderColor = Gray200,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            // Symbol
            var lblSymbol = new XRLabel
            {
                Text = _data.Symbol ?? "N/A",
                LocationF = new PointF(20, 15),
                SizeF = new SizeF(200, 40),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Dark
            };
            heroPanel.Controls.Add(lblSymbol);

            var lblCompany = new XRLabel
            {
                Text = $"{_data.Name} • {_data.Timeframe}",
                LocationF = new PointF(20, 55),
                SizeF = new SizeF(250, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Gray600
            };
            heroPanel.Controls.Add(lblCompany);

            // Price
            var lblPrice = new XRLabel
            {
                Text = $"${_data.LastPrice:N2}",
                LocationF = new PointF(380, 15),
                SizeF = new SizeF(295, 40),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Dark,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            heroPanel.Controls.Add(lblPrice);

            // Change
            var isPositive = _data.ChangePercent >= 0;
            var changeColor = isPositive ? Success : Danger;
            var arrow = isPositive ? "↑" : "↓";
            var lblChange = new XRLabel
            {
                Text = $"{arrow} {Math.Abs(_data.ChangePercent):N2}%  (${Math.Abs(_data.ChangeAbsolute):N2})",
                LocationF = new PointF(380, 55),
                SizeF = new SizeF(295, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = changeColor,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            heroPanel.Controls.Add(lblChange);

            detail.Controls.Add(heroPanel);
            y += 105;

            // ─────────────────────────────────────────────────────────────
            // MARKET DATA SECTION
            // ─────────────────────────────────────────────────────────────
            var lblMarketTitle = new XRLabel
            {
                Text = "Market Data",
                LocationF = new PointF(0, y),
                SizeF = new SizeF(200, 22),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Dark
            };
            detail.Controls.Add(lblMarketTitle);
            y += 28;

            // OHLC Row
            AddDataCard(detail, "Open", $"${_data.Open:N2}", 0, y);
            AddDataCard(detail, "High", $"${_data.High:N2}", 175, y);
            AddDataCard(detail, "Low", $"${_data.Low:N2}", 350, y);
            AddDataCard(detail, "Close", $"${_data.Close:N2}", 525, y);
            y += 65;

            // ─────────────────────────────────────────────────────────────
            // TECHNICAL INDICATORS
            // ─────────────────────────────────────────────────────────────
            var lblTechTitle = new XRLabel
            {
                Text = "Technical Indicators",
                LocationF = new PointF(0, y),
                SizeF = new SizeF(200, 22),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Dark
            };
            detail.Controls.Add(lblTechTitle);
            y += 28;

            AddDataCard(detail, "RSI (14)", _data.RSI ?? "-", 0, y);
            AddDataCard(detail, "MACD", _data.MACD ?? "-", 175, y);
            AddDataCard(detail, "Signal", _data.Signal ?? "-", 350, y);
            AddDataCard(detail, "Volume", _data.Volume ?? "-", 525, y);
            y += 75;

            // ─────────────────────────────────────────────────────────────
            // AI ANALYSIS SECTION
            // ─────────────────────────────────────────────────────────────
            var aiPanel = new XRPanel
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(695, 130),
                BackColor = Color.FromArgb(239, 246, 255), // Light blue bg
                BorderColor = Primary,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            // AI Icon & Title
            var lblAiTitle = new XRLabel
            {
                Text = "AI Analysis",
                LocationF = new PointF(15, 12),
                SizeF = new SizeF(200, 22),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Primary
            };
            aiPanel.Controls.Add(lblAiTitle);

            // Recommendation Badge
            var rec = _data.AIRecommendation ?? "HOLD";
            var recBgColor = rec == "BUY" ? Success : (rec == "SELL" ? Danger : Warning);
            var lblRec = new XRLabel
            {
                Text = rec,
                LocationF = new PointF(570, 10),
                SizeF = new SizeF(110, 28),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = White,
                BackColor = recBgColor,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
            };
            aiPanel.Controls.Add(lblRec);

            // Analysis Text
            var lblAnalysis = new XRLabel
            {
                Text = _data.AIAnalysis ?? "AI analizi mevcut değil.",
                LocationF = new PointF(15, 42),
                SizeF = new SizeF(665, 50),
                Font = new Font("Segoe UI", 10),
                ForeColor = Gray600,
                Multiline = true,
                WordWrap = true
            };
            aiPanel.Controls.Add(lblAnalysis);

            // Confidence
            var conf = _data.AIConfidence ?? "Medium";
            var confColor = conf == "High" ? Success : (conf == "Low" ? Danger : Warning);
            var lblConf = new XRLabel
            {
                Text = $"Confidence: {conf}",
                LocationF = new PointF(15, 100),
                SizeF = new SizeF(150, 18),
                Font = new Font("Segoe UI", 9),
                ForeColor = confColor
            };
            aiPanel.Controls.Add(lblConf);

            var lblPowered = new XRLabel
            {
                Text = "Powered by Groq AI",
                LocationF = new PointF(520, 100),
                SizeF = new SizeF(160, 18),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Gray400,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            aiPanel.Controls.Add(lblPowered);

            detail.Controls.Add(aiPanel);
            y += 145;

            // ─────────────────────────────────────────────────────────────
            // DISCLAIMER
            // ─────────────────────────────────────────────────────────────
            var discPanel = new XRPanel
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(695, 50),
                BackColor = Color.FromArgb(254, 252, 232), // Light yellow
                BorderColor = Warning,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            var lblDiscTitle = new XRLabel
            {
                Text = "⚠ Disclaimer",
                LocationF = new PointF(12, 8),
                SizeF = new SizeF(150, 16),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(161, 98, 7) // Amber
            };
            discPanel.Controls.Add(lblDiscTitle);

            var lblDiscText = new XRLabel
            {
                Text = "This report is for informational purposes only and does not constitute investment advice. Past performance does not guarantee future results.",
                LocationF = new PointF(12, 26),
                SizeF = new SizeF(670, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Gray600
            };
            discPanel.Controls.Add(lblDiscText);

            detail.Controls.Add(discPanel);

            // ══════════════════════════════════════════════════════════════
            // FOOTER
            // ══════════════════════════════════════════════════════════════
            var lblFooterLeft = new XRLabel
            {
                Text = "© 2026 NovaBank • Confidential",
                LocationF = new PointF(0, 5),
                SizeF = new SizeF(250, 18),
                Font = new Font("Segoe UI", 8),
                ForeColor = Gray400
            };
            footer.Controls.Add(lblFooterLeft);

            var pageNum = new XRPageInfo
            {
                LocationF = new PointF(550, 5),
                SizeF = new SizeF(145, 18),
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

        private void AddDataCard(DetailBand band, string label, string value, float x, float y)
        {
            var card = new XRPanel
            {
                LocationF = new PointF(x, y),
                SizeF = new SizeF(165, 55),
                BackColor = White,
                BorderColor = Gray200,
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            var lblLabel = new XRLabel
            {
                Text = label,
                LocationF = new PointF(12, 8),
                SizeF = new SizeF(140, 16),
                Font = new Font("Segoe UI", 9),
                ForeColor = Gray400
            };
            card.Controls.Add(lblLabel);

            var lblValue = new XRLabel
            {
                Text = value,
                LocationF = new PointF(12, 28),
                SizeF = new SizeF(140, 22),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Dark
            };
            card.Controls.Add(lblValue);

            band.Controls.Add(card);
        }
    }
}
