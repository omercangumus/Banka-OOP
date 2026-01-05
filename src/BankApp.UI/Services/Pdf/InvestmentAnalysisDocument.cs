using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankApp.UI.Services.Pdf
{
    public class InvestmentAnalysisDocument : IDocument
    {
        private readonly InvestmentAnalysisData _data;

        public InvestmentAnalysisDocument(InvestmentAnalysisData data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("NovaBank")
                        .FontSize(24)
                        .SemiBold()
                        .FontColor("#1976D2");
                    
                    column.Item().Text("Investment Analysis Report")
                        .FontSize(14)
                        .FontColor("#424242");
                });

                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().Text($"Generated: {_data.GeneratedAt:dd MMM yyyy}")
                        .FontSize(10)
                        .FontColor("#757575");
                    
                    column.Item().Text($"Time: {_data.GeneratedAt:HH:mm}")
                        .FontSize(10)
                        .FontColor("#757575");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(15);

                // Symbol Header
                column.Item().Background("#F5F5F5").Padding(15).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(_data.Symbol)
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#212121");
                        
                        col.Item().Text(_data.Name)
                            .FontSize(12)
                            .FontColor("#757575");
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"${_data.LastPrice:N2}")
                            .FontSize(18)
                            .SemiBold()
                            .FontColor("#212121");
                        
                        var changeColor = _data.ChangePercent >= 0 ? "#26A65B" : "#E84C3D";
                        var changeSign = _data.ChangePercent >= 0 ? "+" : "";
                        
                        col.Item().Text($"{changeSign}{_data.ChangePercent:N2}% ({changeSign}${_data.ChangeAbsolute:N2})")
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(changeColor);
                    });
                });

                // Timeframe
                column.Item().Row(row =>
                {
                    row.ConstantItem(120).Text("Timeframe:")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor("#424242");
                    
                    row.RelativeItem().Text(_data.Timeframe)
                        .FontSize(11)
                        .FontColor("#212121");
                });

                // Technical Indicators Section
                column.Item().PaddingTop(10).Text("Technical Indicators")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1976D2");

                column.Item().LineHorizontal(1).LineColor("#E0E0E0");

                // RSI
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.ConstantItem(120).Text("RSI(14):")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor("#424242");
                    
                    row.RelativeItem().Text(_data.RSI)
                        .FontSize(11)
                        .FontColor("#212121");
                });

                // MACD
                column.Item().Row(row =>
                {
                    row.ConstantItem(120).Text("MACD:")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor("#424242");
                    
                    row.RelativeItem().Text(_data.MACD)
                        .FontSize(11)
                        .FontColor("#212121");
                });

                // Signal
                column.Item().Row(row =>
                {
                    row.ConstantItem(120).Text("Signal:")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor("#424242");
                    
                    row.RelativeItem().Text(_data.Signal)
                        .FontSize(11)
                        .FontColor("#212121");
                });

                // Volume
                column.Item().Row(row =>
                {
                    row.ConstantItem(120).Text("Volume:")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor("#424242");
                    
                    row.RelativeItem().Text(_data.Volume)
                        .FontSize(11)
                        .FontColor("#212121");
                });

                // Disclaimer
                column.Item().PaddingTop(30).Background("#FFF3E0").Padding(15).Column(col =>
                {
                    col.Item().Text("Disclaimer")
                        .FontSize(10)
                        .SemiBold()
                        .FontColor("#F57C00");
                    
                    col.Item().PaddingTop(5).Text("This report is for informational purposes only and does not constitute investment advice. Past performance is not indicative of future results. Please consult with a qualified financial advisor before making investment decisions.")
                        .FontSize(9)
                        .FontColor("#424242")
                        .Italic();
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text("NovaBank © 2026")
                    .FontSize(9)
                    .FontColor("#9E9E9E");

                row.RelativeItem().AlignCenter().Text("Confidential • For informational purposes only")
                    .FontSize(9)
                    .FontColor("#9E9E9E");

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(9).FontColor("#9E9E9E");
                    text.CurrentPageNumber().FontSize(9).FontColor("#9E9E9E");
                    text.Span(" / ").FontSize(9).FontColor("#9E9E9E");
                    text.TotalPages().FontSize(9).FontColor("#9E9E9E");
                });
            });
        }
    }
}
