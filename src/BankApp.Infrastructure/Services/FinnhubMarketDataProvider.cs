using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Finnhub implementation of IMarketDataProvider for stocks
    /// </summary>
    public class FinnhubMarketDataProvider : IMarketDataProvider
    {
        private readonly FinnhubService _finnhubService;
        
        // Popular US stocks - expandable list
        private static readonly List<(string Symbol, string Name, string Exchange)> _stockList = new List<(string, string, string)>
        {
            // Tech Giants
            ("AAPL", "Apple Inc.", "NASDAQ"),
            ("MSFT", "Microsoft Corp.", "NASDAQ"),
            ("GOOGL", "Alphabet Inc.", "NASDAQ"),
            ("AMZN", "Amazon.com Inc.", "NASDAQ"),
            ("META", "Meta Platforms Inc.", "NASDAQ"),
            ("TSLA", "Tesla Inc.", "NASDAQ"),
            ("NVDA", "NVIDIA Corp.", "NASDAQ"),
            ("AMD", "Advanced Micro Devices", "NASDAQ"),
            ("INTC", "Intel Corp.", "NASDAQ"),
            ("NFLX", "Netflix Inc.", "NASDAQ"),
            
            // Financial
            ("JPM", "JPMorgan Chase", "NYSE"),
            ("BAC", "Bank of America", "NYSE"),
            ("WFC", "Wells Fargo", "NYSE"),
            ("GS", "Goldman Sachs", "NYSE"),
            ("MS", "Morgan Stanley", "NYSE"),
            ("V", "Visa Inc.", "NYSE"),
            ("MA", "Mastercard Inc.", "NYSE"),
            ("AXP", "American Express", "NYSE"),
            
            // Healthcare
            ("JNJ", "Johnson & Johnson", "NYSE"),
            ("UNH", "UnitedHealth Group", "NYSE"),
            ("PFE", "Pfizer Inc.", "NYSE"),
            ("ABBV", "AbbVie Inc.", "NYSE"),
            ("TMO", "Thermo Fisher Scientific", "NYSE"),
            ("MRK", "Merck & Co.", "NYSE"),
            ("ABT", "Abbott Laboratories", "NYSE"),
            
            // Consumer
            ("PG", "Procter & Gamble", "NYSE"),
            ("KO", "Coca-Cola Co.", "NYSE"),
            ("PEP", "PepsiCo Inc.", "NASDAQ"),
            ("WMT", "Walmart Inc.", "NYSE"),
            ("HD", "Home Depot", "NYSE"),
            ("MCD", "McDonald's Corp.", "NYSE"),
            ("NKE", "Nike Inc.", "NYSE"),
            ("SBUX", "Starbucks Corp.", "NASDAQ"),
            ("DIS", "Walt Disney Co.", "NYSE"),
            
            // Energy
            ("XOM", "Exxon Mobil", "NYSE"),
            ("CVX", "Chevron Corp.", "NYSE"),
            ("COP", "ConocoPhillips", "NYSE"),
            
            // Industrial
            ("BA", "Boeing Co.", "NYSE"),
            ("CAT", "Caterpillar Inc.", "NYSE"),
            ("GE", "General Electric", "NYSE"),
            ("MMM", "3M Company", "NYSE"),
            
            // Telecom
            ("T", "AT&T Inc.", "NYSE"),
            ("VZ", "Verizon Communications", "NYSE"),
            ("TMUS", "T-Mobile US", "NASDAQ"),
            
            // Other Tech
            ("ORCL", "Oracle Corp.", "NYSE"),
            ("IBM", "IBM Corp.", "NYSE"),
            ("CSCO", "Cisco Systems", "NASDAQ"),
            ("ADBE", "Adobe Inc.", "NASDAQ"),
            ("CRM", "Salesforce Inc.", "NYSE"),
            ("PYPL", "PayPal Holdings", "NASDAQ"),
            ("UBER", "Uber Technologies", "NYSE"),
            ("ABNB", "Airbnb Inc.", "NASDAQ")
        };

        public string AssetType => "Stock";

        public FinnhubMarketDataProvider()
        {
            _finnhubService = new FinnhubService();
        }

        public async Task<List<MarketAsset>> GetAssetsAsync()
        {
            var assets = new List<MarketAsset>();
            
            // Batch fetch quotes for all stocks
            foreach (var stock in _stockList)
            {
                try
                {
                    var quote = await _finnhubService.GetQuoteAsync(stock.Symbol);
                    
                    assets.Add(new MarketAsset
                    {
                        Symbol = stock.Symbol,
                        Name = stock.Name,
                        Exchange = stock.Exchange,
                        AssetType = "Stock",
                        LastPrice = quote.C,
                        Change = quote.D,
                        ChangePercent = quote.Dp,
                        Currency = "USD",
                        IsFavorite = false,
                        LogoUrl = $"https://financialmodelingprep.com/image-stock/{stock.Symbol}.png"
                    });
                    
                    // Small delay to respect rate limits
                    await Task.Delay(50);
                }
                catch
                {
                    // If API fails, add with placeholder data
                    assets.Add(new MarketAsset
                    {
                        Symbol = stock.Symbol,
                        Name = stock.Name,
                        Exchange = stock.Exchange,
                        AssetType = "Stock",
                        LastPrice = 0,
                        Change = 0,
                        ChangePercent = 0,
                        Currency = "USD",
                        IsFavorite = false,
                        LogoUrl = $"https://financialmodelingprep.com/image-stock/{stock.Symbol}.png"
                    });
                }
            }
            
            return assets;
        }

        public async Task<MarketQuote> GetQuoteAsync(string symbol)
        {
            var quote = await _finnhubService.GetQuoteAsync(symbol);
            
            return new MarketQuote
            {
                Symbol = symbol,
                Current = quote.C,
                Open = quote.O,
                High = quote.H,
                Low = quote.L,
                PreviousClose = quote.Pc,
                Change = quote.D,
                ChangePercent = quote.Dp,
                Volume = 0, // Finnhub quote doesn't include volume
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(quote.T).DateTime
            };
        }

        public async Task<List<MarketCandle>> GetCandlesAsync(string symbol, string timeframe, int count)
        {
            var candles = await _finnhubService.GetCandlesAsync(symbol, timeframe, count);
            
            if (candles?.C == null || candles.C.Count == 0)
                return new List<MarketCandle>();
            
            var result = new List<MarketCandle>();
            for (int i = 0; i < candles.C.Count; i++)
            {
                result.Add(new MarketCandle
                {
                    Time = DateTimeOffset.FromUnixTimeSeconds(candles.T[i]).DateTime,
                    Open = candles.O[i],
                    High = candles.H[i],
                    Low = candles.L[i],
                    Close = candles.C[i],
                    Volume = candles.V != null && candles.V.Count > i ? candles.V[i] : 0
                });
            }
            
            return result;
        }

        public async Task<List<MarketNews>> GetNewsAsync(string symbol, int count = 10)
        {
            var news = await _finnhubService.GetMarketNewsAsync("general");
            
            return news.Select(n => new MarketNews
            {
                Title = n.Headline,
                Summary = n.Summary,
                Url = n.Url,
                PublishedAt = DateTimeOffset.FromUnixTimeSeconds(n.Datetime).DateTime,
                Source = n.Source,
                ImageUrl = n.Image
            }).Take(count).ToList();
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
    }
}
