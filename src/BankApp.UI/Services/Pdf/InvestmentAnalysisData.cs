using System;
using System.Collections.Generic;

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
        
        // OHLC Data
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        
        // Technical Indicators
        public string RSI { get; set; } = "N/A";
        public string MACD { get; set; } = "N/A";
        public string Signal { get; set; } = "N/A";
        public string Volume { get; set; } = "N/A";
        
        // 52 Week Range
        public double Week52High { get; set; }
        public double Week52Low { get; set; }
        
        // AI Analysis
        public string AIAnalysis { get; set; }
        public string AIRecommendation { get; set; } // BUY, SELL, HOLD
        public string AIConfidence { get; set; } // High, Medium, Low
        
        // Price History for Chart (last N candles)
        public List<CandleData> PriceHistory { get; set; } = new List<CandleData>();
        
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
    
    public class CandleData
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }
}
