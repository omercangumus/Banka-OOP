using System;
using System.IO;
using BankApp.UI.Services.Pdf;

namespace BankApp.UI.Reports
{
    public static class PdfReportExporter
    {
        /// <summary>
        /// Generates Investment Analysis PDF using DevExpress XtraReport
        /// NO QuestPDF, NO Chart references
        /// </summary>
        public static void GenerateInvestmentReport(InvestmentAnalysisData data, string filePath)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            // Create and export report
            using var report = new InvestmentAnalysisReport(data);
            report.ExportToPdf(filePath);
        }
    }
}
