using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace BankApp.UI.Services.Pdf
{
    public static class PdfStyles
    {
        public static void Title(this IContainer container)
        {
            container
                .PaddingBottom(5)
                .Text(text => text.DefaultTextStyle(x => x
                    .FontSize(PdfTheme.TitleSize)
                    .FontColor(PdfTheme.PrimaryNavy)
                    .Bold()));
        }
        
        public static void Subtitle(this IContainer container)
        {
            container
                .PaddingBottom(3)
                .Text(text => text.DefaultTextStyle(x => x
                    .FontSize(PdfTheme.SubtitleSize)
                    .FontColor(PdfTheme.TextGray)));
        }
        
        public static void Heading(this IContainer container)
        {
            container
                .PaddingTop(10)
                .PaddingBottom(5)
                .Text(text => text.DefaultTextStyle(x => x
                    .FontSize(PdfTheme.HeadingSize)
                    .FontColor(PdfTheme.PrimaryNavy)
                    .SemiBold()));
        }
        
        public static void Body(this IContainer container)
        {
            container
                .Text(text => text.DefaultTextStyle(x => x
                    .FontSize(PdfTheme.BodySize)
                    .FontColor(PdfTheme.TextDark)));
        }
        
        public static void Small(this IContainer container)
        {
            container
                .Text(text => text.DefaultTextStyle(x => x
                    .FontSize(PdfTheme.SmallSize)
                    .FontColor(PdfTheme.TextGray)));
        }
        
        public static void PositiveValue(this IContainer container)
        {
            container
                .Text(text => text.DefaultTextStyle(x => x
                    .FontSize(PdfTheme.BodySize)
                    .FontColor(PdfTheme.PositiveGreen)
                    .Bold()));
        }
        
        public static void NegativeValue(this IContainer container)
        {
            container
                .Text(text => text.DefaultTextStyle(x => x
                    .FontSize(PdfTheme.BodySize)
                    .FontColor(PdfTheme.NegativeRed)
                    .Bold()));
        }
        
        public static IContainer TableHeader(this IContainer container)
        {
            return container
                .Background(PdfTheme.PrimaryNavy)
                .Padding(8)
                .DefaultTextStyle(x => x
                    .FontColor("#FFFFFF")
                    .FontSize(PdfTheme.BodySize)
                    .SemiBold());
        }
        
        public static IContainer TableCell(this IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(PdfTheme.BorderGray)
                .Padding(6)
                .DefaultTextStyle(x => x
                    .FontSize(PdfTheme.BodySize)
                    .FontColor(PdfTheme.TextDark));
        }
        
        public static IContainer TableCellAlternate(this IContainer container)
        {
            return container
                .Background(PdfTheme.BackgroundLight)
                .BorderBottom(1)
                .BorderColor(PdfTheme.BorderGray)
                .Padding(6)
                .DefaultTextStyle(x => x
                    .FontSize(PdfTheme.BodySize)
                    .FontColor(PdfTheme.TextDark));
        }
        
        public static IContainer Card(this IContainer container)
        {
            return container
                .Background("#FFFFFF")
                .Border(1)
                .BorderColor(PdfTheme.BorderGray)
                .Padding(12);
        }
        
        public static IContainer DisclaimerBox(this IContainer container)
        {
            return container
                .Background("#FFF3E0")
                .Border(1)
                .BorderColor("#FFB74D")
                .Padding(10);
        }
    }
}
