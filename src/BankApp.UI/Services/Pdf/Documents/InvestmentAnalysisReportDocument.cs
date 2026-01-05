using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankApp.UI.Services.Pdf.Documents
{
    public class InvestmentAnalysisReportDocument : IDocument
    {
        private readonly InvestmentReportData _data;
        
        public InvestmentAnalysisReportDocument(InvestmentReportData data)
        {
            _data = data;
        }
        
        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Investment Analysis - {_data.Symbol}",
            Author = "NovaBank",
            Subject = "Investment Analysis Report",
            Creator = "NovaBank Investment Platform"
        };
        
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(PdfTheme.PageMargin);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI"));
                
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }
        
        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("NOVABANK")
                            .FontSize(20)
                            .Bold()
                            .FontColor(PdfTheme.PrimaryNavy);
                        col.Item().Text("Investment Platform")
                            .FontSize(10)
                            .FontColor(PdfTheme.TextGray);
                    });
                    
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("Investment Analysis Report")
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(PdfTheme.PrimaryNavy);
                        col.Item().Text(_data.GeneratedAt.ToString("dd MMM yyyy HH:mm"))
                            .FontSize(9)
                            .FontColor(PdfTheme.TextGray);
                    });
                });
                
                column.Item().PaddingTop(8).LineHorizontal(1).LineColor(PdfTheme.PrimaryNavy);
            });
        }
        
        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(15).Column(column =>
            {
                // Title Block
                column.Item().Element(ComposeTitleBlock);
                
                // Market Snapshot
                column.Item().Element(ComposeMarketSnapshot);
                
                // Indicators Summary
                column.Item().Element(ComposeIndicatorsSummary);
                
                // Chart Preview (if available)
                if (_data.ChartImage != null && _data.ChartImage.Length > 0)
                {
                    column.Item().Element(ComposeChartPreview);
                }
                
                // User Notes
                if (!string.IsNullOrWhiteSpace(_data.UserNotes))
                {
                    column.Item().Element(ComposeUserNotes);
                }
                
                // Risk Disclaimer
                column.Item().Element(ComposeDisclaimer);
            });
        }
        
        private void ComposeTitleBlock(IContainer container)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(_data.Symbol)
                            .FontSize(28)
                            .Bold()
                            .FontColor(PdfTheme.PrimaryNavy);
                        
                        col.Item().Text(_data.Name)
                            .FontSize(14)
                            .FontColor(PdfTheme.TextGray);
                        
                        col.Item().PaddingTop(3).Text($"{_data.AssetType} • {_data.Timeframe}")
                            .FontSize(10)
                            .FontColor(PdfTheme.SecondaryBlue);
                    });
                    
                    row.RelativeItem().AlignRight().AlignMiddle().Column(col =>
                    {
                        col.Item().Text($"${_data.LastPrice:N2}")
                            .FontSize(24)
                            .Bold()
                            .FontColor(PdfTheme.TextDark);
                        
                        var changeColor = _data.ChangePercent >= 0 ? PdfTheme.PositiveGreen : PdfTheme.NegativeRed;
                        var changeSign = _data.ChangePercent >= 0 ? "+" : "";
                        col.Item().AlignRight().Text($"{changeSign}{_data.ChangePercent:N2}% ({changeSign}{_data.ChangeAbsolute:N2})")
                            .FontSize(14)
                            .Bold()
                            .FontColor(changeColor);
                    });
                });
            });
        }
        
        private void ComposeMarketSnapshot(IContainer container)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Text("Market Snapshot")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor(PdfTheme.PrimaryNavy);
                
                column.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    
                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Last Price").SemiBold();
                        header.Cell().Element(CellStyle).Text("Change %").SemiBold();
                        header.Cell().Element(CellStyle).Text("Change $").SemiBold();
                        header.Cell().Element(CellStyle).Text("Volume").SemiBold();
                        
                        static IContainer CellStyle(IContainer c) => c
                            .Background(PdfTheme.PrimaryNavy)
                            .Padding(8)
                            .DefaultTextStyle(x => x.FontColor("#FFFFFF").FontSize(10));
                    });
                    
                    // Data row
                    var changeColor = _data.ChangePercent >= 0 ? PdfTheme.PositiveGreen : PdfTheme.NegativeRed;
                    
                    table.Cell().Element(DataCellStyle).Text($"${_data.LastPrice:N2}");
                    table.Cell().Element(DataCellStyle).Text($"{_data.ChangePercent:+0.00;-0.00}%").FontColor(changeColor);
                    table.Cell().Element(DataCellStyle).Text($"${_data.ChangeAbsolute:+0.00;-0.00}").FontColor(changeColor);
                    table.Cell().Element(DataCellStyle).Text(_data.Volume.HasValue ? $"{_data.Volume:N0}" : "N/A");
                    
                    static IContainer DataCellStyle(IContainer c) => c
                        .BorderBottom(1)
                        .BorderColor(PdfTheme.BorderGray)
                        .Padding(8)
                        .DefaultTextStyle(x => x.FontSize(10));
                });
            });
        }
        
        private void ComposeIndicatorsSummary(IContainer container)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Text("Technical Indicators")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor(PdfTheme.PrimaryNavy);
                
                column.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    
                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Indicator").SemiBold();
                        header.Cell().Element(CellStyle).Text("Value").SemiBold();
                        
                        static IContainer CellStyle(IContainer c) => c
                            .Background(PdfTheme.PrimaryNavy)
                            .Padding(8)
                            .DefaultTextStyle(x => x.FontColor("#FFFFFF").FontSize(10));
                    });
                    
                    AddIndicatorRow(table, "MA (20)", _data.MA20);
                    AddIndicatorRow(table, "MA (50)", _data.MA50);
                    AddIndicatorRow(table, "EMA (12)", _data.EMA12);
                    AddIndicatorRow(table, "EMA (26)", _data.EMA26);
                    AddIndicatorRow(table, "Bollinger Bands", _data.BollingerEnabled ? "Enabled" : "Disabled");
                    AddIndicatorRow(table, "RSI (14)", _data.RSI);
                    AddIndicatorRow(table, "MACD", _data.MACD);
                });
            });
        }
        
        private void AddIndicatorRow(TableDescriptor table, string name, double? value)
        {
            table.Cell().Element(DataCellStyle).Text(name);
            table.Cell().Element(DataCellStyle).Text(value.HasValue ? $"{value:N2}" : "N/A");
            
            static IContainer DataCellStyle(IContainer c) => c
                .BorderBottom(1)
                .BorderColor(PdfTheme.BorderGray)
                .Padding(6)
                .DefaultTextStyle(x => x.FontSize(10));
        }
        
        private void AddIndicatorRow(TableDescriptor table, string name, string value)
        {
            table.Cell().Element(DataCellStyle).Text(name);
            table.Cell().Element(DataCellStyle).Text(value);
            
            static IContainer DataCellStyle(IContainer c) => c
                .BorderBottom(1)
                .BorderColor(PdfTheme.BorderGray)
                .Padding(6)
                .DefaultTextStyle(x => x.FontSize(10));
        }
        
        private void ComposeChartPreview(IContainer container)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Text("Chart Preview")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor(PdfTheme.PrimaryNavy);
                
                column.Item().PaddingTop(8)
                    .Border(1)
                    .BorderColor(PdfTheme.BorderGray)
                    .Image(_data.ChartImage)
                    .FitWidth();
            });
        }
        
        private void ComposeUserNotes(IContainer container)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Text("User Notes")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor(PdfTheme.PrimaryNavy);
                
                column.Item().PaddingTop(8)
                    .Background(PdfTheme.BackgroundLight)
                    .Border(1)
                    .BorderColor(PdfTheme.BorderGray)
                    .Padding(10)
                    .Text(_data.UserNotes)
                    .FontSize(10)
                    .FontColor(PdfTheme.TextDark);
            });
        }
        
        private void ComposeDisclaimer(IContainer container)
        {
            container.PaddingTop(20).Column(column =>
            {
                column.Item()
                    .Background("#FFF3E0")
                    .Border(1)
                    .BorderColor("#FFB74D")
                    .Padding(12)
                    .Column(inner =>
                    {
                        inner.Item().Text("⚠️ Risk Disclaimer")
                            .FontSize(11)
                            .SemiBold()
                            .FontColor("#E65100");
                        
                        inner.Item().PaddingTop(5).Text(
                            "This report is for informational purposes only and does not constitute investment advice. " +
                            "Past performance is not indicative of future results. Always conduct your own research " +
                            "and consult with a qualified financial advisor before making investment decisions.")
                            .FontSize(9)
                            .FontColor("#5D4037");
                    });
            });
        }
        
        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(PdfTheme.BorderGray);
                
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("NovaBank © 2026")
                        .FontSize(8)
                        .FontColor(PdfTheme.TextGray);
                    
                    row.RelativeItem().AlignCenter().Text("Confidential • For informational purposes only")
                        .FontSize(8)
                        .FontColor(PdfTheme.TextGray);
                    
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(PdfTheme.TextGray);
                        text.CurrentPageNumber().FontSize(8).FontColor(PdfTheme.TextGray);
                        text.Span(" / ").FontSize(8).FontColor(PdfTheme.TextGray);
                        text.TotalPages().FontSize(8).FontColor(PdfTheme.TextGray);
                    });
                });
            });
        }
    }
}
