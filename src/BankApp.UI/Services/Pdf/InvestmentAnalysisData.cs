using System;

namespace BankApp.UI.Services.Pdf
{
    public class InvestmentAnalysisData
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Timeframe { get; set; }
        public double LastPrice { get; set; }
        public double ChangePercent { get; set; }
        public double ChangeAbsolute { get; set; }
        public string RSI { get; set; } = "N/A";
        public string MACD { get; set; } = "N/A";
        public string Signal { get; set; } = "N/A";
        public string Volume { get; set; } = "N/A";
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
