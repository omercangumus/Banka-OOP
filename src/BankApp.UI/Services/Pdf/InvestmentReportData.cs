using System;

namespace BankApp.UI.Services.Pdf
{
    public class InvestmentReportData
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public string AssetType { get; set; } = "Stock";
        public double LastPrice { get; set; }
        public double ChangePercent { get; set; }
        public double ChangeAbsolute { get; set; }
        public double? Volume { get; set; }
        public string Timeframe { get; set; } = "1D";
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        
        // Indicators
        public double? MA20 { get; set; }
        public double? MA50 { get; set; }
        public double? EMA12 { get; set; }
        public double? EMA26 { get; set; }
        public bool BollingerEnabled { get; set; }
        public double? RSI { get; set; }
        public double? MACD { get; set; }
        
        // User notes
        public string UserNotes { get; set; } = "";
        
        // Chart image (optional)
        public byte[] ChartImage { get; set; }
    }
}
