using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Service for portfolio calculations and enrichment
    /// Uses cached Finnhub prices to avoid rate limits
    /// </summary>
    public class PortfolioService
    {
        private readonly FinnhubService _finnhubService;
        private Dictionary<string, decimal> _priceCache;
        private DateTime _lastCacheUpdate;
        private const int CACHE_DURATION_MINUTES = 5;

        public PortfolioService()
        {
            _finnhubService = new FinnhubService();
            _priceCache = new Dictionary<string, decimal>();
            _lastCacheUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Get enriched portfolio holdings with current prices and P/L
        /// </summary>
        public async Task<List<PortfolioHolding>> GetEnrichedHoldingsAsync()
        {
            var holdings = MockPortfolioData.GetUserHoldings();

            // Update price cache if needed
            if ((DateTime.Now - _lastCacheUpdate).TotalMinutes > CACHE_DURATION_MINUTES)
            {
                await UpdatePriceCacheAsync(holdings);
            }

            // Enrich each holding with current price and calculations
            foreach (var holding in holdings)
            {
                holding.CurrentPrice = GetCurrentPrice(holding);
                holding.CurrentValue = holding.Quantity * holding.CurrentPrice;
                holding.ProfitLoss = holding.CurrentValue - (holding.Quantity * holding.AverageCost);
                holding.ProfitLossPercent = holding.AverageCost != 0 
                    ? ((holding.CurrentPrice - holding.AverageCost) / holding.AverageCost) * 100 
                    : 0;
            }

            return holdings;
        }

        /// <summary>
        /// Calculate total net worth across all assets
        /// </summary>
        public async Task<decimal> GetNetWorthAsync()
        {
            var holdings = await GetEnrichedHoldingsAsync();
            return holdings.Sum(h => h.CurrentValue);
        }

        /// <summary>
        /// Calculate total profit/loss across portfolio
        /// </summary>
        public async Task<(decimal amount, decimal percent)> GetTotalProfitLossAsync()
        {
            var holdings = await GetEnrichedHoldingsAsync();
            decimal totalCost = holdings.Sum(h => h.Quantity * h.AverageCost);
            decimal totalValue = holdings.Sum(h => h.CurrentValue);
            decimal profitLoss = totalValue - totalCost;
            decimal profitLossPercent = totalCost != 0 ? (profitLoss / totalCost) * 100 : 0;

            return (profitLoss, profitLossPercent);
        }

        /// <summary>
        /// Calculate asset allocation percentages
        /// </summary>
        public async Task<Dictionary<BankApp.Infrastructure.Services.AssetType, decimal>> GetAssetAllocationAsync()
        {
            var holdings = await GetEnrichedHoldingsAsync();
            decimal totalValue = holdings.Sum(h => h.CurrentValue);

            var allocation = holdings
                .GroupBy(h => h.AssetType)
                .ToDictionary(
                    g => g.Key,
                    g => totalValue != 0 ? (g.Sum(h => h.CurrentValue) / totalValue) * 100 : 0
                );

            return allocation;
        }

        /// <summary>
        /// Update price cache from Finnhub API (rate-limited)
        /// </summary>
        private async Task UpdatePriceCacheAsync(List<PortfolioHolding> holdings)
        {
            try
            {
                _priceCache.Clear();

                // Only fetch prices for tradeable assets (not Cash or Pension)
                var tradeableHoldings = holdings
                    .Where(h => h.AssetType != AssetType.Cash && h.AssetType != AssetType.Pension)
                    .ToList();

                foreach (var holding in tradeableHoldings)
                {
                    try
                    {
                        var symbol = ConvertSymbolForFinnhub(holding.Symbol);
                        var quote = await _finnhubService.GetQuoteAsync(symbol);
                        
                        if (quote != null && quote.C > 0)
                        {
                            // Convert double to decimal (learned from DEV_LOG)
                            _priceCache[holding.Symbol] = (decimal)quote.C;
                        }
                    }
                    catch
                    {
                        // If API fails, use fallback (average cost as current price)
                        _priceCache[holding.Symbol] = holding.AverageCost;
                    }
                }

                _lastCacheUpdate = DateTime.Now;
            }
            catch
            {
                // Cache update failed, use existing cache or fallbacks
            }
        }

        /// <summary>
        /// Get current price from cache or fallback
        /// </summary>
        private decimal GetCurrentPrice(PortfolioHolding holding)
        {
            // Cash and Pension use average cost as current price
            if (holding.AssetType == AssetType.Cash || holding.AssetType == AssetType.Pension)
            {
                return holding.AverageCost;
            }

            // Try to get from cache
            if (_priceCache.ContainsKey(holding.Symbol))
            {
                return _priceCache[holding.Symbol];
            }

            // Fallback: simulate price movement based on purchase date
            return SimulatePriceMovement(holding);
        }

        /// <summary>
        /// Simulate realistic price movement for demo purposes
        /// </summary>
        private decimal SimulatePriceMovement(PortfolioHolding holding)
        {
            var random = new Random(holding.Symbol.GetHashCode());
            var daysSincePurchase = (DateTime.Now - holding.PurchaseDate).Days;
            
            // Simulate different volatility by asset type
            decimal volatility = holding.AssetType switch
            {
                AssetType.Crypto => 0.002m,  // 0.2% per day (high volatility)
                AssetType.Stock => 0.0005m,  // 0.05% per day
                AssetType.Gold => 0.0003m,   // 0.03% per day
                AssetType.Forex => 0.0002m,  // 0.02% per day
                _ => 0m
            };

            decimal cumulativeChange = 0;
            for (int i = 0; i < daysSincePurchase; i++)
            {
                cumulativeChange += (decimal)(random.NextDouble() - 0.5) * 2 * volatility;
            }

            return holding.AverageCost * (1 + cumulativeChange);
        }

        /// <summary>
        /// Convert local symbols to Finnhub format
        /// </summary>
        private string ConvertSymbolForFinnhub(string symbol)
        {
            // Turkish stocks already have .IS suffix
            if (symbol.EndsWith(".IS")) return symbol;
            
            // Crypto symbols
            if (symbol == "BTC") return "BINANCE:BTCUSDT";
            if (symbol == "ETH") return "BINANCE:ETHUSDT";
            
            // Forex
            if (symbol == "USD-TRY") return "OANDA:USD_TRY";
            
            // Gold
            if (symbol == "GOLD") return "OANDA:XAU_USD";
            
            return symbol;
        }
    }
}
