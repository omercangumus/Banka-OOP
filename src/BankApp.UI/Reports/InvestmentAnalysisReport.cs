using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using BankApp.UI.Services.Pdf;

namespace BankApp.UI.Reports
{
    public class InvestmentAnalysisReport : XtraReport
    {
        private readonly InvestmentAnalysisData _data;

        public InvestmentAnalysisReport(InvestmentAnalysisData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            InitializeReport();
        }

        private void InitializeReport()
        {
            // Page settings
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            this.Margins.Left = 50;
            this.Margins.Right = 50;
            this.Margins.Top = 50;
            this.Margins.Bottom = 50;

            // Create bands
            var reportHeader = new ReportHeaderBand { HeightF = 80 };
            var detailBand = new DetailBand { HeightF = 400 };
            var pageFooter = new PageFooterBand { HeightF = 30 };

            // === HEADER ===
            var lblTitle = new XRLabel
            {
                Text = "NovaBank",
                LocationF = new PointF(0, 10),
                SizeF = new SizeF(300, 30),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 118, 210)
            };
            reportHeader.Controls.Add(lblTitle);

            var lblSubtitle = new XRLabel
            {
                Text = "Investment Analysis Report",
                LocationF = new PointF(0, 40),
                SizeF = new SizeF(300, 20),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(66, 66, 66)
            };
            reportHeader.Controls.Add(lblSubtitle);

            var lblDate = new XRLabel
            {
                Text = $"Generated: {_data.GeneratedAt:dd MMM yyyy HH:mm}",
                LocationF = new PointF(450, 10),
                SizeF = new SizeF(200, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(117, 117, 117),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            reportHeader.Controls.Add(lblDate);

            var headerLine = new XRLine
            {
                LocationF = new PointF(0, 70),
                SizeF = new SizeF(650, 2),
                ForeColor = Color.FromArgb(25, 118, 210)
            };
            reportHeader.Controls.Add(headerLine);

            // === DETAIL - Symbol Info ===
            float y = 10;

            // Symbol box
            var symbolBox = new XRPanel
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(650, 70),
                BackColor = Color.FromArgb(245, 245, 245),
                BorderColor = Color.FromArgb(224, 224, 224),
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            var lblSymbol = new XRLabel
            {
                Text = _data.Symbol ?? "N/A",
                LocationF = new PointF(15, 10),
                SizeF = new SizeF(200, 30),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33)
            };
            symbolBox.Controls.Add(lblSymbol);

            var lblName = new XRLabel
            {
                Text = _data.Name ?? "N/A",
                LocationF = new PointF(15, 40),
                SizeF = new SizeF(200, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(117, 117, 117)
            };
            symbolBox.Controls.Add(lblName);

            var lblPrice = new XRLabel
            {
                Text = $"${_data.LastPrice:N2}",
                LocationF = new PointF(400, 10),
                SizeF = new SizeF(230, 30),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            symbolBox.Controls.Add(lblPrice);

            var changeColor = _data.ChangePercent >= 0 ? Color.FromArgb(38, 166, 91) : Color.FromArgb(232, 76, 61);
            var changeSign = _data.ChangePercent >= 0 ? "+" : "";
            var lblChange = new XRLabel
            {
                Text = $"{changeSign}{_data.ChangePercent:N2}% ({changeSign}${_data.ChangeAbsolute:N2})",
                LocationF = new PointF(400, 40),
                SizeF = new SizeF(230, 20),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = changeColor,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
            };
            symbolBox.Controls.Add(lblChange);

            detailBand.Controls.Add(symbolBox);
            y += 90;

            // Timeframe
            AddLabelPair(detailBand, "Timeframe:", _data.Timeframe ?? "N/A", ref y);
            y += 20;

            // === INDICATORS SECTION ===
            var lblIndicators = new XRLabel
            {
                Text = "Technical Indicators",
                LocationF = new PointF(0, y),
                SizeF = new SizeF(300, 25),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 118, 210)
            };
            detailBand.Controls.Add(lblIndicators);
            y += 30;

            var indicatorLine = new XRLine
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(650, 1),
                ForeColor = Color.FromArgb(224, 224, 224)
            };
            detailBand.Controls.Add(indicatorLine);
            y += 15;

            AddLabelPair(detailBand, "RSI (14):", _data.RSI ?? "N/A", ref y);
            AddLabelPair(detailBand, "MACD:", _data.MACD ?? "N/A", ref y);
            AddLabelPair(detailBand, "Signal:", _data.Signal ?? "N/A", ref y);
            AddLabelPair(detailBand, "Volume:", _data.Volume ?? "N/A", ref y);
            y += 30;

            // === DISCLAIMER ===
            var disclaimerBox = new XRPanel
            {
                LocationF = new PointF(0, y),
                SizeF = new SizeF(650, 70),
                BackColor = Color.FromArgb(255, 243, 224),
                BorderColor = Color.FromArgb(255, 183, 77),
                Borders = DevExpress.XtraPrinting.BorderSide.All
            };

            var lblDisclaimer = new XRLabel
            {
                Text = "Disclaimer",
                LocationF = new PointF(15, 8),
                SizeF = new SizeF(200, 18),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 124, 0)
            };
            disclaimerBox.Controls.Add(lblDisclaimer);

            var lblDisclaimerText = new XRLabel
            {
                Text = "This report is for informational purposes only and does not constitute investment advice. Past performance is not indicative of future results.",
                LocationF = new PointF(15, 28),
                SizeF = new SizeF(620, 35),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(66, 66, 66),
                Multiline = true,
                WordWrap = true
            };
            disclaimerBox.Controls.Add(lblDisclaimerText);

            detailBand.Controls.Add(disclaimerBox);

            // === FOOTER ===
            var lblFooterLeft = new XRLabel
            {
                Text = "NovaBank © 2026",
                LocationF = new PointF(0, 5),
                SizeF = new SizeF(200, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(158, 158, 158)
            };
            pageFooter.Controls.Add(lblFooterLeft);

            var lblFooterCenter = new XRLabel
            {
                Text = "Confidential • For informational purposes only",
                LocationF = new PointF(200, 5),
                SizeF = new SizeF(250, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(158, 158, 158),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
            };
            pageFooter.Controls.Add(lblFooterCenter);

            var pageInfo = new XRPageInfo
            {
                LocationF = new PointF(500, 5),
                SizeF = new SizeF(150, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(158, 158, 158),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight,
                Format = "Page {0} / {1}"
            };
            pageFooter.Controls.Add(pageInfo);

            // Add bands to report
            this.Bands.Add(reportHeader);
            this.Bands.Add(detailBand);
            this.Bands.Add(pageFooter);
        }

        private void AddLabelPair(DetailBand band, string label, string value, ref float y)
        {
            var lblLabel = new XRLabel
            {
                Text = label,
                LocationF = new PointF(0, y),
                SizeF = new SizeF(120, 22),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66)
            };
            band.Controls.Add(lblLabel);

            var lblValue = new XRLabel
            {
                Text = value,
                LocationF = new PointF(130, y),
                SizeF = new SizeF(300, 22),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(33, 33, 33)
            };
            band.Controls.Add(lblValue);

            y += 28;
        }
    }
}
