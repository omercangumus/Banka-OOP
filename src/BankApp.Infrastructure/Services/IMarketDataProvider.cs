using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Market data provider interface for stocks and crypto
    /// </summary>
    public interface IMarketDataProvider
    {
        /// <summary>
        /// Get asset type (Stock or Crypto)
        /// </summary>
        string AssetType { get; }

        /// <summary>
        /// Get list of available symbols with basic info
        /// </summary>
        Task<List<MarketAsset>> GetAssetsAsync();

        /// <summary>
        /// Get real-time quote for a symbol
        /// </summary>
        Task<MarketQuote> GetQuoteAsync(string symbol);

        /// <summary>
        /// Get candlestick data for charting
        /// </summary>
        Task<List<MarketCandle>> GetCandlesAsync(string symbol, string timeframe, int count);

        /// <summary>
        /// Get recent news for a symbol
        /// </summary>
        Task<List<MarketNews>> GetNewsAsync(string symbol, int count = 10);

        /// <summary>
        /// Search/filter assets by query
        /// </summary>
        Task<List<MarketAsset>> SearchAssetsAsync(string query);
    }

    /// <summary>
    /// Market asset info (stock or crypto)
    /// </summary>
    public class MarketAsset
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Exchange { get; set; }
        public string AssetType { get; set; } // "Stock" or "Crypto"
        public double LastPrice { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
        public string Currency { get; set; }
        public bool IsFavorite { get; set; }
        public string LogoUrl { get; set; }
    }

    /// <summary>
    /// Real-time quote data
    /// </summary>
    public class MarketQuote
    {
        public string Symbol { get; set; }
        public double Current { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double PreviousClose { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
        public long Volume { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Candlestick data for charting
    /// </summary>
    public class MarketCandle
    {
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }
    }

    /// <summary>
    /// News/article data
    /// </summary>
    public class MarketNews
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Source { get; set; }
        public string ImageUrl { get; set; }
    }
}
