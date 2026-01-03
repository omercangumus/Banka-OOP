using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Enhanced Finnhub Service V2 for Investment Dashboard with expanded asset support
    /// Supports 8-12 major assets including stocks, crypto, forex, and commodities
    /// </summary>
    public class FinnhubServiceV2 : FinnhubService
    {
        // Enhanced asset list for Investment Dashboard V2
        private readonly AssetConfiguration[] _supportedAssets = new AssetConfiguration[]
        {
            // Stocks
            new AssetConfiguration { Symbol = "AAPL", DisplayName = "Apple Inc.", Type = AssetType.Stock, FinnhubSymbol = "AAPL", IsEnabled = true },
            new AssetConfiguration { Symbol = "TSLA", DisplayName = "Tesla Inc.", Type = AssetType.Stock, FinnhubSymbol = "TSLA", IsEnabled = true },
            new AssetConfiguration { Symbol = "AMZN", DisplayName = "Amazon.com Inc.", Type = AssetType.Stock, FinnhubSymbol = "AMZN", IsEnabled = true },
            new AssetConfiguration { Symbol = "GOOGL", DisplayName = "Alphabet Inc.", Type = AssetType.Stock, FinnhubSymbol = "GOOGL", IsEnabled = true },
            
            // Crypto
            new AssetConfiguration { Symbol = "BTC-USD", DisplayName = "Bitcoin", Type = AssetType.Crypto, FinnhubSymbol = "BINANCE:BTCUSDT", IsEnabled = true },
            new AssetConfiguration { Symbol = "ETH-USD", DisplayName = "Ethereum", Type = AssetType.Crypto, FinnhubSymbol = "BINANCE:ETHUSDT", IsEnabled = true },
            
            // Commodities
            new AssetConfiguration { Symbol = "XAU", DisplayName = "Gold", Type = AssetType.Commodity, FinnhubSymbol = "OANDA:XAU_USD", IsEnabled = true },
            
            // Forex
            new AssetConfiguration { Symbol = "EUR/USD", DisplayName = "Euro/US Dollar", Type = AssetType.Forex, FinnhubSymbol = "OANDA:EUR_USD", IsEnabled = true },
            new AssetConfiguration { Symbol = "GBP/USD", DisplayName = "British Pound/US Dollar", Type = AssetType.Forex, FinnhubSymbol = "OANDA:GBP_USD", IsEnabled = true }
        };

        private readonly Dictionary<string, MarketData> _marketDataCache;
        private readonly TimeSpan _marketDataCacheExpiry = TimeSpan.FromSeconds(30);

        public FinnhubServiceV2() : base()
        {
            _marketDataCache = new Dictionary<string, MarketData>();
        }

        /// <summary>
        /// Get all supported assets for the dashboard
        /// </summary>
        public AssetConfiguration[] GetSupportedAssets()
        {
            return _supportedAssets.Where(a => a.IsEnabled).ToArray();
        }

        /// <summary>
        /// Get enhanced market data for a specific asset
        /// </summary>
        public async Task<MarketData> GetMarketDataAsync(string symbol)
        {
            string cacheKey = $"market_{symbol}";
            
            // Check cache first
            if (_marketDataCache.ContainsKey(cacheKey))
            {
                var cached = _marketDataCache[cacheKey];
                if (DateTime.Now - cached.LastUpdate < _marketDataCacheExpiry)
                {
                    return cached;
                }
            }

            try
            {
                var asset = _supportedAssets.FirstOrDefault(a => a.Symbol == symbol);
                if (asset == null)
                {
                    throw new ArgumentException($"Unsupported asset: {symbol}");
                }

                // Get real-time quote
                var quote = await GetQuoteAsync(asset.FinnhubSymbol);
                
                // Get historical candlestick data for sparkline
                var candles = await GetCandlesAsync(asset.FinnhubSymbol, "D", 30);
                var candlestickData = CandlestickData.FromFinnhubCandles(candles);

                var marketData = new MarketData
                {
                    Symbol = symbol,
                    DisplayName = asset.DisplayName,
                    AssetType = asset.Type,
                    CurrentPrice = (decimal)quote.C,
                    Change = (decimal)quote.D,
                    ChangePercent = (decimal)quote.Dp,
                    High24h = (decimal)quote.H,
                    Low24h = (decimal)quote.L,
                    Volume = 0, // Volume not available in quote endpoint
                    LastUpdate = DateTime.Now,
                    HistoricalData = candlestickData,
                    IsValid = true
                };

                // Cache the result
                _marketDataCache[cacheKey] = marketData;
                
                return marketData;
            }
            catch (Exception ex)
            {
                LogError($"GetMarketData error for {symbol}: {ex.Message}");
                return GetFallbackMarketData(symbol);
            }
        }

        /// <summary>
        /// Get market data for multiple assets
        /// </summary>
        public async Task<List<MarketData>> GetMultipleMarketDataAsync(string[] symbols = null)
        {
            var results = new List<MarketData>();
            var assetsToFetch = symbols ?? _supportedAssets.Where(a => a.IsEnabled).Select(a => a.Symbol).ToArray();
            
            foreach (var symbol in assetsToFetch)
            {
                var marketData = await GetMarketDataAsync(symbol);
                results.Add(marketData);
                
                // Small delay to respect rate limits
                await Task.Delay(100);
            }
            
            return results;
        }

        /// <summary>
        /// Get historical candlestick data with enhanced error handling
        /// </summary>
        public async Task<CandlestickData[]> GetHistoricalDataAsync(string symbol, int days = 30)
        {
            try
            {
                var asset = _supportedAssets.FirstOrDefault(a => a.Symbol == symbol);
                if (asset == null)
                {
                    throw new ArgumentException($"Unsupported asset: {symbol}");
                }

                var candles = await GetCandlesAsync(asset.FinnhubSymbol, "D", days);
                return CandlestickData.FromFinnhubCandles(candles);
            }
            catch (Exception ex)
            {
                LogError($"GetHistoricalData error for {symbol}: {ex.Message}");
                return GenerateFallbackCandlestickData(symbol, days);
            }
        }

        /// <summary>
        /// Validate data sufficiency for technical indicator calculations
        /// </summary>
        public ValidationResult ValidateDataSufficiency(double[] data, int requiredPeriods)
        {
            if (data == null || data.Length == 0)
            {
                return ValidationResult.Failure("No data available");
            }

            if (data.Length < requiredPeriods)
            {
                return ValidationResult.Failure($"Insufficient data: {data.Length} points available, {requiredPeriods} required");
            }

            if (data.Any(d => double.IsNaN(d) || double.IsInfinity(d)))
            {
                return ValidationResult.Failure("Data contains invalid values");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Clear all caches
        /// </summary>
        public void ClearAllCaches()
        {
            _marketDataCache.Clear();
        }

        private MarketData GetFallbackMarketData(string symbol)
        {
            var asset = _supportedAssets.FirstOrDefault(a => a.Symbol == symbol);
            var random = new Random(symbol.GetHashCode());
            
            double basePrice = asset?.Type switch
            {
                AssetType.Stock => random.Next(50, 500),
                AssetType.Crypto => random.Next(1000, 50000),
                AssetType.Forex => random.NextDouble() + 1.0,
                AssetType.Commodity => random.Next(1500, 2500),
                _ => random.Next(100, 1000)
            };

            double change = (random.NextDouble() * 10 - 5);
            
            return new MarketData
            {
                Symbol = symbol,
                DisplayName = asset?.DisplayName ?? symbol,
                AssetType = asset?.Type ?? AssetType.Stock,
                CurrentPrice = (decimal)basePrice,
                Change = (decimal)change,
                ChangePercent = (decimal)((change / basePrice) * 100),
                High24h = (decimal)(basePrice + Math.Abs(change) * 0.5),
                Low24h = (decimal)(basePrice - Math.Abs(change) * 0.5),
                Volume = random.Next(100000, 10000000),
                LastUpdate = DateTime.Now,
                HistoricalData = GenerateFallbackCandlestickData(symbol, 30),
                IsValid = false,
                ErrorMessage = "Using fallback data due to API error"
            };
        }

        private CandlestickData[] GenerateFallbackCandlestickData(string symbol, int days)
        {
            var random = new Random(symbol.GetHashCode());
            var data = new List<CandlestickData>();
            
            double price = random.Next(50, 500);
            
            for (int i = 0; i < days; i++)
            {
                double change = (random.NextDouble() * 10 - 5);
                double open = price;
                price += change;
                double close = price;
                double high = Math.Max(open, close) + Math.Abs(change) * 0.3;
                double low = Math.Min(open, close) - Math.Abs(change) * 0.3;
                
                data.Add(new CandlestickData
                {
                    Time = DateTime.Now.AddDays(-days + i),
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = random.Next(100000, 1000000)
                });
            }

            return data.ToArray();
        }

        private void LogError(string message)
        {
            // Log to DEV_LOG.md or console
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] - [FinnhubServiceV2] - {message}");
        }
    }
}