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

            // SIMPLE TEST - just header band with one label
            var reportHeader = new ReportHeaderBand { HeightF = 200 };
            
            var lblTitle = new XRLabel
            {
                Text = $"Investment Report\n\nSymbol: {_data.Symbol}\nPrice: {_data.LastPrice:N2}\nChange: {_data.ChangePercent:N2}%\nTime: {_data.GeneratedAt}",
                LocationF = new PointF(50, 50),
                SizeF = new SizeF(500, 100),
                Font = new Font("Arial", 14),
                ForeColor = Color.Black,
                Multiline = true
            };
            reportHeader.Controls.Add(lblTitle);
            
            this.Bands.Add(reportHeader);
        }
    }
}
