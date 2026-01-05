using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace BankApp.UI.Services.Pdf
{
    public static class PdfGenerator
    {
        static PdfGenerator()
        {
            // Configure QuestPDF license (Community license for free use)
            QuestPDF.Settings.License = LicenseType.Community;
        }
        
        public static void GenerateInvestmentAnalysis(InvestmentAnalysisData data, string filePath)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("PDF path is empty.", nameof(filePath));
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            // Generate PDF - IDocument implementation
            var document = new InvestmentAnalysisDocument(data);
            document.GeneratePdf(filePath);
        }
    }
}
