using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankApp.UI.Services.Pdf
{
    public static class PdfTheme
    {
        // NovaBank Corporate Colors
        public static readonly string PrimaryNavy = "#1a237e";
        public static readonly string SecondaryBlue = "#2196F3";
        public static readonly string PositiveGreen = "#26A65B";
        public static readonly string NegativeRed = "#E84142";
        public static readonly string TextDark = "#212121";
        public static readonly string TextGray = "#757575";
        public static readonly string BorderGray = "#E0E0E0";
        public static readonly string BackgroundLight = "#FAFAFA";
        
        // Page Settings
        public const float PageMargin = 25;
        public const float HeaderHeight = 60;
        public const float FooterHeight = 30;
        
        // Font Sizes
        public const float TitleSize = 24;
        public const float SubtitleSize = 16;
        public const float HeadingSize = 14;
        public const float BodySize = 10;
        public const float SmallSize = 8;
        
        public static string ColorFromHex(string hex)
        {
            return hex;
        }
    }
}
