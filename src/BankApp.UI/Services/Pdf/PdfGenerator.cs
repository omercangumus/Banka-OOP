using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using BankApp.UI.Services.Pdf.Documents;

namespace BankApp.UI.Services.Pdf
{
    public static class PdfGenerator
    {
        static PdfGenerator()
        {
            // Configure QuestPDF license (Community license for free use)
            QuestPDF.Settings.License = LicenseType.Community;
        }
        
        public static void GenerateInvestmentAnalysisReport(InvestmentReportData data, string filePath)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            
            var document = new InvestmentAnalysisReportDocument(data);
            document.GeneratePdf(filePath);
        }
        
        public static byte[] GenerateInvestmentAnalysisReportBytes(InvestmentReportData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            var document = new InvestmentAnalysisReportDocument(data);
            return document.GeneratePdf();
        }
        
        public static void GenerateInvestmentAnalysis(InvestmentAnalysisData data, string filePath)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            
            var document = new InvestmentAnalysisDocument(data);
            document.GeneratePdf(filePath);
        }
    }
}
