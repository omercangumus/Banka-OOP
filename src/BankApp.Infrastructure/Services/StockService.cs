using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;

namespace BankApp.Infrastructure.Services
{
    public class StockService
    {
        private readonly HttpClient _httpClient;
        
        public StockService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }
        
        /// <summary>
        /// Yahoo Finance'den canlı hisse fiyatı ve değişim verilerini çeker
        /// </summary>
        public async Task<StockLiveData> GetLiveStockDataAsync(string symbol)
        {
            try
            {
                // Yahoo Finance URL'i
                string url = $"https://finance.yahoo.com/quote/{symbol}";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return GetFallbackData(symbol);
                }
                
                string html = await response.Content.ReadAsStringAsync();
                
                // Fiyat ve değişim bilgisini HTML'den çıkar
                var price = ExtractPrice(html);
                var change = ExtractChange(html);
                var changePercent = ExtractChangePercent(html);
                
                return new StockLiveData
                {
                    Symbol = symbol,
                    Price = price ?? GetRandomPrice(symbol),
                    Change = change ?? 0m,
                    ChangePercent = changePercent ?? 0m,
                    LastUpdated = DateTime.Now,
                    IsLive = price.HasValue
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Stock scraping error: {ex.Message}");
                return GetFallbackData(symbol);
            }
        }
        
        private decimal? ExtractPrice(string html)
        {
            try
            {
                // Yahoo Finance'de fiyat için birden fazla pattern dene
                var patterns = new[]
                {
                    @"data-symbol=""[^""]*""\s+data-test=""qsp-price""\s+data-field=""regularMarketPrice""\s+data-trend=""[^""]*""\s+data-pricehint=""[^""]*""\s+value=""([0-9,.]+)""",
                    @"data-reactid=""[^""]*""\s*\u003e([0-9,.]+)\u003c/span\u003e\s*\u003c/fin-streamer\u003e",
                    @"""regularMarketPrice"":{""raw"":([0-9.]+)",
                };
                
                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(html, pattern);
                    if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                    {
                        return price;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        private decimal? ExtractChange(string html)
        {
            try
            {
                var patterns = new[]
                {
                    @"""regularMarketChange"":{""raw"":([+-]?[0-9.]+)",
                    @"data-field=""regularMarketChange""\s+[^>]*value=""([+-]?[0-9,.]+)""",
                };
                
                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(html, pattern);
                    if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var change))
                    {
                        return change;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        private decimal? ExtractChangePercent(string html)
        {
            try
            {
                var patterns = new[]
                {
                    @"""regularMarketChangePercent"":{""raw"":([+-]?[0-9.]+)",
                    @"data-field=""regularMarketChangePercent""\s+[^>]*value=""([+-]?[0-9,.]+)""",
                };
                
                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(html, pattern);
                    if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", "").Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var changePercent))
                    {
                        return changePercent;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Scraping başarısız olursa simüle edilmiş veri döner
        /// </summary>
        private StockLiveData GetFallbackData(string symbol)
        {
            return new StockLiveData
            {
                Symbol = symbol,
                Price = GetRandomPrice(symbol),
                Change = GetRandomChange(),
                ChangePercent = GetRandomChangePercent(),
                LastUpdated = DateTime.Now,
                IsLive = false // Simülasyon olduğunu belirt
            };
        }
        
        private decimal GetRandomPrice(string symbol)
        {
            var random = new Random(symbol.GetHashCode());
            return random.Next(50, 500) + (decimal)random.NextDouble();
        }
        
        private decimal GetRandomChange()
        {
            var random = new Random();
            return (decimal)((random.NextDouble() * 10) - 5); // -5 ile +5 arası
        }
        
        private decimal GetRandomChangePercent()
        {
            var random = new Random();
            return (decimal)((random.NextDouble() * 4) - 2); // -2% ile +2% arası
        }
    }
    
    /// <summary>
    /// Canlı hisse verisi modeli
    /// </summary>
    public class StockLiveData
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsLive { get; set; } // true = gerçek veri, false = simülasyon
        
        public string ChangeDisplay => Change >= 0 ? $"+{Change:N2}" : $"{Change:N2}";
        public string ChangePercentDisplay => ChangePercent >= 0 ? $"+{ChangePercent:N2}%" : $"{ChangePercent:N2}%";
    }
}
