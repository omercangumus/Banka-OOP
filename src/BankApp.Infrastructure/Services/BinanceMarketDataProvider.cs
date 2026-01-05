using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Binance implementation of IMarketDataProvider for crypto
    /// </summary>
    public class BinanceMarketDataProvider : IMarketDataProvider
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _baseUrl = "https://api.binance.com";
        
        // Popular crypto pairs - expandable list
        private static readonly List<(string Symbol, string Name)> _cryptoList = new List<(string, string)>
        {
            // Major Pairs
            ("BTCUSDT", "Bitcoin"),
            ("ETHUSDT", "Ethereum"),
            ("BNBUSDT", "Binance Coin"),
            ("XRPUSDT", "Ripple"),
            ("ADAUSDT", "Cardano"),
            ("DOGEUSDT", "Dogecoin"),
            ("SOLUSDT", "Solana"),
            ("DOTUSDT", "Polkadot"),
            ("MATICUSDT", "Polygon"),
            ("TRXUSDT", "TRON"),
            
            // DeFi Tokens
            ("UNIUSDT", "Uniswap"),
            ("LINKUSDT", "Chainlink"),
            ("AVAXUSDT", "Avalanche"),
            ("ATOMUSDT", "Cosmos"),
            ("LTCUSDT", "Litecoin"),
            ("ETCUSDT", "Ethereum Classic"),
            ("NEARUSDT", "NEAR Protocol"),
            ("ALGOUSDT", "Algorand"),
            ("AAVEUSDT", "Aave"),
            ("FTMUSDT", "Fantom"),
            
            // Stablecoins & Others
            ("SHIBUSDT", "Shiba Inu"),
            ("APTUSDT", "Aptos"),
            ("ARBUSDT", "Arbitrum"),
            ("OPUSDT", "Optimism"),
            ("INJUSDT", "Injective"),
            ("LDOUSDT", "Lido DAO"),
            ("SUIUSDT", "Sui"),
            ("PEPEUSDT", "Pepe"),
            ("BCHUSDT", "Bitcoin Cash"),
            ("XLMUSDT", "Stellar")
        };

        public string AssetType => "Crypto";

        public BinanceMarketDataProvider()
        {
            _apiKey = GetApiKey();
            _secretKey = GetSecretKey();
        }

        private static string GetApiKey()
        {
            // Read from config
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var configPath = System.IO.Path.Combine(appDir, "appsettings.local.json");
                
                if (!System.IO.File.Exists(configPath))
                {
                    var devPath = System.IO.Path.Combine(appDir, "..", "..", "..", "appsettings.local.json");
                    if (System.IO.File.Exists(devPath))
                        configPath = devPath;
                }
                
                if (System.IO.File.Exists(configPath))
                {
                    var json = System.IO.File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("Binance", out var section) &&
                        section.TryGetProperty("ApiKey", out var keyElement))
                    {
                        return keyElement.GetString() ?? "";
                    }
                }
            }
            catch { }
            
            return "";
        }

        private static string GetSecretKey()
        {
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var configPath = System.IO.Path.Combine(appDir, "appsettings.local.json");
                
                if (!System.IO.File.Exists(configPath))
                {
                    var devPath = System.IO.Path.Combine(appDir, "..", "..", "..", "appsettings.local.json");
                    if (System.IO.File.Exists(devPath))
                        configPath = devPath;
                }
                
                if (System.IO.File.Exists(configPath))
                {
                    var json = System.IO.File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("Binance", out var section) &&
                        section.TryGetProperty("SecretKey", out var keyElement))
                    {
                        return keyElement.GetString() ?? "";
                    }
                }
            }
            catch { }
            
            return "";
        }

        public async Task<List<MarketAsset>> GetAssetsAsync()
        {
            var assets = new List<MarketAsset>();
            
            // Batch fetch prices for all crypto pairs
            try
            {
                // Get all prices in one request
                var url = $"{_baseUrl}/api/v3/ticker/24hr";
                var response = await _http.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return GetFallbackAssets();
                }
                
                var json = await response.Content.ReadAsStringAsync();
                var tickers = JsonSerializer.Deserialize<List<BinanceTicker>>(json);
                
                foreach (var crypto in _cryptoList)
                {
                    var ticker = tickers?.FirstOrDefault(t => t.symbol == crypto.Symbol);
                    
                    var baseSymbol = crypto.Symbol.Replace("USDT", "").ToLower();
                    var logoUrl = $"https://cryptologos.cc/logos/{GetCryptoLogoName(crypto.Name)}-{baseSymbol}-logo.png";
                    
                    if (ticker != null)
                    {
                        assets.Add(new MarketAsset
                        {
                            Symbol = crypto.Symbol,
                            Name = crypto.Name,
                            Exchange = "Binance",
                            AssetType = "Crypto",
                            LastPrice = double.Parse(ticker.lastPrice),
                            Change = double.Parse(ticker.priceChange),
                            ChangePercent = double.Parse(ticker.priceChangePercent),
                            Currency = "USDT",
                            IsFavorite = false,
                            LogoUrl = logoUrl
                        });
                    }
                    else
                    {
                        // Fallback
                        assets.Add(new MarketAsset
                        {
                            Symbol = crypto.Symbol,
                            Name = crypto.Name,
                            Exchange = "Binance",
                            AssetType = "Crypto",
                            LastPrice = 0,
                            Change = 0,
                            ChangePercent = 0,
                            Currency = "USDT",
                            IsFavorite = false,
                            LogoUrl = logoUrl
                        });
                    }
                }
            }
            catch
            {
                return GetFallbackAssets();
            }
            
            return assets;
        }

        public async Task<MarketQuote> GetQuoteAsync(string symbol)
        {
            try
            {
                var url = $"{_baseUrl}/api/v3/ticker/24hr?symbol={symbol}";
                var response = await _http.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return GetFallbackQuote(symbol);
                }
                
                var json = await response.Content.ReadAsStringAsync();
                var ticker = JsonSerializer.Deserialize<BinanceTicker>(json);
                
                return new MarketQuote
                {
                    Symbol = symbol,
                    Current = double.Parse(ticker.lastPrice),
                    Open = double.Parse(ticker.openPrice),
                    High = double.Parse(ticker.highPrice),
                    Low = double.Parse(ticker.lowPrice),
                    PreviousClose = double.Parse(ticker.prevClosePrice),
                    Change = double.Parse(ticker.priceChange),
                    ChangePercent = double.Parse(ticker.priceChangePercent),
                    Volume = long.Parse(ticker.volume.Split('.')[0]),
                    Timestamp = DateTime.Now
                };
            }
            catch
            {
                return GetFallbackQuote(symbol);
            }
        }

        public async Task<List<MarketCandle>> GetCandlesAsync(string symbol, string timeframe, int count)
        {
            try
            {
                // Convert timeframe to Binance format (D -> 1d, H -> 1h, etc.)
                var interval = ConvertTimeframe(timeframe);
                
                var url = $"{_baseUrl}/api/v3/klines?symbol={symbol}&interval={interval}&limit={count}";
                var response = await _http.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<MarketCandle>();
                }
                
                var json = await response.Content.ReadAsStringAsync();
                var klines = JsonSerializer.Deserialize<List<List<JsonElement>>>(json);
                
                var candles = new List<MarketCandle>();
                foreach (var kline in klines)
                {
                    candles.Add(new MarketCandle
                    {
                        Time = DateTimeOffset.FromUnixTimeMilliseconds(kline[0].GetInt64()).DateTime,
                        Open = double.Parse(kline[1].GetString()),
                        High = double.Parse(kline[2].GetString()),
                        Low = double.Parse(kline[3].GetString()),
                        Close = double.Parse(kline[4].GetString()),
                        Volume = long.Parse(kline[5].GetString().Split('.')[0])
                    });
                }
                
                return candles;
            }
            catch
            {
                return new List<MarketCandle>();
            }
        }

        public Task<List<MarketNews>> GetNewsAsync(string symbol, int count = 10)
        {
            // Binance doesn't have news API, return empty
            return Task.FromResult(new List<MarketNews>());
        }

        public async Task<List<MarketAsset>> SearchAssetsAsync(string query)
        {
            var allAssets = await GetAssetsAsync();
            
            if (string.IsNullOrWhiteSpace(query))
                return allAssets;
            
            query = query.ToUpperInvariant();
            
            return allAssets.Where(a => 
                a.Symbol.Contains(query) || 
                a.Name.ToUpperInvariant().Contains(query)
            ).ToList();
        }

        private string ConvertTimeframe(string timeframe)
        {
            // Convert TradingView format to Binance format
            return timeframe switch
            {
                "1" => "1m",
                "5" => "5m",
                "15" => "15m",
                "60" => "1h",
                "240" => "4h",
                "D" => "1d",
                "W" => "1w",
                "M" => "1M",
                _ => "1d"
            };
        }

        private static string GetCryptoLogoName(string name)
        {
            return name.ToLower().Replace(" ", "-");
        }
        
        private List<MarketAsset> GetFallbackAssets()
        {
            return _cryptoList.Select(c => {
                var baseSymbol = c.Symbol.Replace("USDT", "").ToLower();
                return new MarketAsset
                {
                    Symbol = c.Symbol,
                    Name = c.Name,
                    Exchange = "Binance",
                    AssetType = "Crypto",
                    LastPrice = 0,
                    Change = 0,
                    ChangePercent = 0,
                    Currency = "USDT",
                    IsFavorite = false,
                    LogoUrl = $"https://cryptologos.cc/logos/{GetCryptoLogoName(c.Name)}-{baseSymbol}-logo.png"
                };
            }).ToList();
        }

        private MarketQuote GetFallbackQuote(string symbol)
        {
            return new MarketQuote
            {
                Symbol = symbol,
                Current = 0,
                Open = 0,
                High = 0,
                Low = 0,
                PreviousClose = 0,
                Change = 0,
                ChangePercent = 0,
                Volume = 0,
                Timestamp = DateTime.Now
            };
        }

        #region Binance DTOs
        private class BinanceTicker
        {
            public string symbol { get; set; }
            public string priceChange { get; set; }
            public string priceChangePercent { get; set; }
            public string lastPrice { get; set; }
            public string openPrice { get; set; }
            public string highPrice { get; set; }
            public string lowPrice { get; set; }
            public string prevClosePrice { get; set; }
            public string volume { get; set; }
        }
        #endregion
    }
}
