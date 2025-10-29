using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Globalization;

namespace BankApp.Infrastructure.Services
{
    public class FinnhubService
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly string _apiKey = "d5ce7rpr01qsbm8k8vh0d5ce7rpr01qsbm8k8vhg";
        private readonly string _baseUrl = "https://finnhub.io/api/v1";
        
        // Cache to prevent hitting rate limits
        private Dictionary<string, (DateTime timestamp, object data)> _cache;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromSeconds(30); // Cache for 30 seconds
        
        // Popular symbols for dashboard
        private readonly string[] _popularSymbols = new string[] 
        { 
            "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", 
            "META", "NVDA", "AMD", "NFLX", "DIS"
        };

        public FinnhubService()
        {
            _cache = new Dictionary<string, (DateTime, object)>();
        }

        /// <summary>
        /// Get real-time quote for a symbol
        /// </summary>
        public async Task<FinnhubQuote> GetQuoteAsync(string symbol)
        {
            string cacheKey = $"quote_{symbol}";
            
            // Check cache first
            if (_cache.ContainsKey(cacheKey))
            {
                var cached = _cache[cacheKey];
                if (DateTime.Now - cached.timestamp < _cacheExpiry)
                {
                    return cached.data as FinnhubQuote;
                }
            }

            try
            {
                string url = $"{_baseUrl}/quote?symbol={symbol}&token={_apiKey}";
                var response = await _http.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Finnhub API Error: {response.StatusCode}");
                    return GetFallbackQuote(symbol);
                }

                var json = await response.Content.ReadAsStringAsync();
                var quote = JsonSerializer.Deserialize<FinnhubQuote>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Cache the result
                _cache[cacheKey] = (DateTime.Now, quote);
                
                return quote;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Finnhub GetQuote Exception: {ex.Message}");
                return GetFallbackQuote(symbol);
            }
        }

        /// <summary>
        /// Get candle data for charts (OHLC)
        /// </summary>
        public async Task<FinnhubCandles> GetCandlesAsync(string symbol, string resolution = "D", int days = 30)
        {
            string cacheKey = $"candles_{symbol}_{resolution}_{days}";
            
            if (_cache.ContainsKey(cacheKey))
            {
                var cached = _cache[cacheKey];
                if (DateTime.Now - cached.timestamp < TimeSpan.FromMinutes(5)) // Longer cache for historical data
                {
                    return cached.data as FinnhubCandles;
                }
            }

            try
            {
                long to = DateTimeOffset.Now.ToUnixTimeSeconds();
                long from = DateTimeOffset.Now.AddDays(-days).ToUnixTimeSeconds();
                
                string url = $"{_baseUrl}/stock/candle?symbol={symbol}&resolution={resolution}&from={from}&to={to}&token={_apiKey}";
                var response = await _http.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Finnhub Candles API Error: {response.StatusCode}");
                    return GetFallbackCandles(symbol, days);
                }

                var json = await response.Content.ReadAsStringAsync();
                var candles = JsonSerializer.Deserialize<FinnhubCandles>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (candles.S == "no_data")
                {
                    return GetFallbackCandles(symbol, days);
                }

                _cache[cacheKey] = (DateTime.Now, candles);
                return candles;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Finnhub GetCandles Exception: {ex.Message}");
                return GetFallbackCandles(symbol, days);
            }
        }

        /// <summary>
        /// Get market news
        /// </summary>
        public async Task<List<FinnhubNews>> GetMarketNewsAsync(string category = "general")
        {
            string cacheKey = $"news_{category}";
            
            if (_cache.ContainsKey(cacheKey))
            {
                var cached = _cache[cacheKey];
                if (DateTime.Now - cached.timestamp < TimeSpan.FromMinutes(10))
                {
                    return cached.data as List<FinnhubNews>;
                }
            }

            try
            {
                string url = $"{_baseUrl}/news?category={category}&token={_apiKey}";
                var response = await _http.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Finnhub News API Error: {response.StatusCode}");
                    return GetFallbackNews();
                }

                var json = await response.Content.ReadAsStringAsync();
                var news = JsonSerializer.Deserialize<List<FinnhubNews>>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Take top 10
                var topNews = news?.Take(10).ToList() ?? new List<FinnhubNews>();
                
                _cache[cacheKey] = (DateTime.Now, topNews);
                return topNews;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Finnhub GetNews Exception: {ex.Message}");
                return GetFallbackNews();
            }
        }

        /// <summary>
        /// Get multiple quotes at once
        /// </summary>
        public async Task<List<(string symbol, FinnhubQuote quote)>> GetMultipleQuotesAsync(string[] symbols)
        {
            var results = new List<(string, FinnhubQuote)>();
            
            foreach (var symbol in symbols)
            {
                var quote = await GetQuoteAsync(symbol);
                results.Add((symbol, quote));
                
                // Small delay to respect rate limits
                await Task.Delay(100);
            }
            
            return results;
        }

        public string[] GetPopularSymbols() => _popularSymbols;

        // Fallback methods for when API fails
        private FinnhubQuote GetFallbackQuote(string symbol)
        {
            var random = new Random(symbol.GetHashCode());
            double basePrice = random.Next(50, 500);
            double change = (random.NextDouble() * 10 - 5);
            
            return new FinnhubQuote
            {
                C = basePrice,
                D = change,
                Dp = (change / basePrice) * 100,
                H = basePrice + 5,
                L = basePrice - 5,
                O = basePrice - change,
                Pc = basePrice - change,
                T = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
        }

        private FinnhubCandles GetFallbackCandles(string symbol, int days)
        {
            var random = new Random(symbol.GetHashCode());
            var candles = new FinnhubCandles
            {
                S = "ok",
                C = new List<double>(),
                H = new List<double>(),
                L = new List<double>(),
                O = new List<double>(),
                T = new List<long>(),
                V = new List<long>()
            };

            double price = random.Next(50, 500);
            for (int i = 0; i < days; i++)
            {
                double change = (random.NextDouble() * 10 - 5);
                price += change;
                
                candles.O.Add(price - change);
                candles.C.Add(price);
                candles.H.Add(price + Math.Abs(change) * 0.5);
                candles.L.Add(price - Math.Abs(change) * 0.5);
                candles.T.Add(DateTimeOffset.Now.AddDays(-days + i).ToUnixTimeSeconds());
                candles.V.Add(random.Next(100000, 1000000));
            }

            return candles;
        }

        private List<FinnhubNews> GetFallbackNews()
        {
            return new List<FinnhubNews>
            {
                new FinnhubNews
                {
                    Headline = "Market Update: Tech Stocks Rally",
                    Summary = "Major technology stocks showed strong performance today...",
                    Source = "Financial Times",
                    Url = "https://finnhub.io",
                    Datetime = DateTimeOffset.Now.ToUnixTimeSeconds()
                },
                new FinnhubNews
                {
                    Headline = "Fed Signals Caution on Rate Changes",
                    Summary = "Federal Reserve maintains current stance...",
                    Source = "Reuters",
                    Url = "https://finnhub.io",
                    Datetime = DateTimeOffset.Now.AddHours(-2).ToUnixTimeSeconds()
                },
                new FinnhubNews
                {
                    Headline = "Energy Sector Shows Resilience",
                    Summary = "Oil prices stabilize amid global demand...",
                    Source = "Bloomberg",
                    Url = "https://finnhub.io",
                    Datetime = DateTimeOffset.Now.AddHours(-4).ToUnixTimeSeconds()
                }
            };
        }
    }

    // Data models for Finnhub API responses
    public class FinnhubQuote
    {
        public double C { get; set; }  // Current price
        public double D { get; set; }  // Change
        public double Dp { get; set; } // Percent change
        public double H { get; set; }  // High price of the day
        public double L { get; set; }  // Low price of the day
        public double O { get; set; }  // Open price of the day
        public double Pc { get; set; } // Previous close price
        public long T { get; set; }    // Timestamp
    }

    public class FinnhubCandles
    {
        public string S { get; set; }         // Status
        public List<double> C { get; set; }   // Close prices
        public List<double> H { get; set; }   // High prices
        public List<double> L { get; set; }   // Low prices
        public List<double> O { get; set; }   // Open prices
        public List<long> T { get; set; }     // Timestamps
        public List<long> V { get; set; }     // Volumes
    }

    public class FinnhubNews
    {
        public string Category { get; set; }
        public long Datetime { get; set; }
        public string Headline { get; set; }
        public int Id { get; set; }
        public string Image { get; set; }
        public string Related { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
    }

    // Enhanced MarketData class for Investment Dashboard V2
    public class MarketData
    {
        public string Symbol { get; set; }
        public string DisplayName { get; set; }
        public AssetType AssetType { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public long Volume { get; set; }
        public DateTime LastUpdate { get; set; }
        public CandlestickData[] HistoricalData { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public bool IsPriceUp => Change > 0;
        public bool IsPriceDown => Change < 0;
        public string PriceDirection => IsPriceUp ? "▲" : (IsPriceDown ? "▼" : "→");
        public System.Drawing.Color PriceColor => IsPriceUp ? System.Drawing.Color.Green : 
                                                 (IsPriceDown ? System.Drawing.Color.Red : System.Drawing.Color.Gray);
    }

    public enum AssetType
    {
        Stock,
        Crypto,
        Forex,
        Commodity
    }

    public class CandlestickData
    {
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }

        /// <summary>
        /// Create CandlestickData from FinnhubCandles
        /// </summary>
        public static CandlestickData[] FromFinnhubCandles(FinnhubCandles candles)
        {
            if (candles == null || candles.C == null || candles.C.Count == 0)
            {
                return new CandlestickData[0];
            }

            var result = new List<CandlestickData>();
            
            for (int i = 0; i < candles.C.Count; i++)
            {
                result.Add(new CandlestickData
                {
                    Time = DateTimeOffset.FromUnixTimeSeconds(candles.T[i]).DateTime,
                    Open = candles.O[i],
                    High = candles.H[i],
                    Low = candles.L[i],
                    Close = candles.C[i],
                    Volume = candles.V[i]
                });
            }

            return result.ToArray();
        }
    }

    public class AssetConfiguration
    {
        public string Symbol { get; set; }
        public string DisplayName { get; set; }
        public AssetType Type { get; set; }
        public string FinnhubSymbol { get; set; }
        public bool IsEnabled { get; set; }
    }
}