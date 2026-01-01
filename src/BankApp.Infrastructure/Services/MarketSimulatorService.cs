#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Core.Entities;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Borsa simülatörü - Arka planda hisse fiyatlarını günceller
    /// </summary>
    public class MarketSimulatorService : IDisposable
    {
        private readonly List<Stock> _stocks;
        private readonly Random _random = new Random();
        private CancellationTokenSource? _cts;
        private Task? _simulationTask;
        private bool _isRunning;

        /// <summary>
        /// Fiyat değiştiğinde tetiklenir
        /// </summary>
        public event EventHandler<StockPriceChangedEventArgs>? PriceChanged;

        /// <summary>
        /// Varsayılan BIST hisseleri ile başlat
        /// </summary>
        public MarketSimulatorService()
        {
            _stocks = new List<Stock>
            {
                new Stock { Id = 1, Symbol = "THYAO", Name = "Türk Hava Yolları", CurrentPrice = 285.50m, PreviousPrice = 283.00m, Volatility = 1.5m, Sector = "Havacılık" },
                new Stock { Id = 2, Symbol = "GARAN", Name = "Garanti BBVA", CurrentPrice = 125.80m, PreviousPrice = 124.50m, Volatility = 1.2m, Sector = "Bankacılık" },
                new Stock { Id = 3, Symbol = "AKBNK", Name = "Akbank", CurrentPrice = 58.40m, PreviousPrice = 57.80m, Volatility = 1.0m, Sector = "Bankacılık" },
                new Stock { Id = 4, Symbol = "SISE", Name = "Şişecam", CurrentPrice = 48.65m, PreviousPrice = 49.10m, Volatility = 0.8m, Sector = "Cam" },
                new Stock { Id = 5, Symbol = "TUPRS", Name = "Tüpraş", CurrentPrice = 185.20m, PreviousPrice = 182.00m, Volatility = 2.0m, Sector = "Enerji" },
                new Stock { Id = 6, Symbol = "EREGL", Name = "Ereğli Demir Çelik", CurrentPrice = 52.30m, PreviousPrice = 51.80m, Volatility = 1.3m, Sector = "Metal" },
                new Stock { Id = 7, Symbol = "KCHOL", Name = "Koç Holding", CurrentPrice = 198.75m, PreviousPrice = 195.00m, Volatility = 1.1m, Sector = "Holding" },
                new Stock { Id = 8, Symbol = "SAHOL", Name = "Sabancı Holding", CurrentPrice = 82.50m, PreviousPrice = 81.20m, Volatility = 1.0m, Sector = "Holding" },
                new Stock { Id = 9, Symbol = "BIMAS", Name = "BİM Mağazalar", CurrentPrice = 410.00m, PreviousPrice = 405.00m, Volatility = 0.6m, Sector = "Perakende" },
                new Stock { Id = 10, Symbol = "ASELS", Name = "Aselsan", CurrentPrice = 68.90m, PreviousPrice = 67.50m, Volatility = 1.8m, Sector = "Savunma" }
            };
        }

        /// <summary>
        /// Tüm hisseleri getir
        /// </summary>
        public IReadOnlyList<Stock> GetAllStocks() => _stocks.AsReadOnly();

        /// <summary>
        /// Belirli bir hisseyi getir
        /// </summary>
        public Stock? GetStock(string symbol) => _stocks.Find(s => s.Symbol == symbol);

        /// <summary>
        /// Simülasyonu başlat
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cts = new CancellationTokenSource();

            _simulationTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(3000, _cts.Token); // Her 3 saniyede bir

                        foreach (var stock in _stocks)
                        {
                            UpdateStockPrice(stock);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, _cts.Token);
        }

        /// <summary>
        /// Simülasyonu durdur
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _cts?.Cancel();
            _isRunning = false;
        }

        /// <summary>
        /// Hisse fiyatını rastgele güncelle (±0.5% - ±2% arası)
        /// </summary>
        private void UpdateStockPrice(Stock stock)
        {
            stock.PreviousPrice = stock.CurrentPrice;

            // Oynaklığa göre değişim oranı hesapla
            double volatilityFactor = (double)stock.Volatility / 100;
            double minChange = 0.005 * volatilityFactor; // %0.5
            double maxChange = 0.02 * volatilityFactor;  // %2

            double changePercent = minChange + _random.NextDouble() * (maxChange - minChange);
            
            // Rastgele artış veya azalış
            if (_random.Next(2) == 0)
                changePercent = -changePercent;

            decimal newPrice = stock.CurrentPrice * (1 + (decimal)changePercent);
            
            // Fiyatın negatif olmamasını sağla
            stock.CurrentPrice = Math.Max(0.01m, Math.Round(newPrice, 2));

            // Event fırlat
            PriceChanged?.Invoke(this, new StockPriceChangedEventArgs(stock));
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }

    /// <summary>
    /// Fiyat değişim event argümanları
    /// </summary>
    public class StockPriceChangedEventArgs : EventArgs
    {
        public Stock Stock { get; }
        public decimal OldPrice { get; }
        public decimal NewPrice { get; }
        public decimal ChangePercent { get; }

        public StockPriceChangedEventArgs(Stock stock)
        {
            Stock = stock;
            OldPrice = stock.PreviousPrice;
            NewPrice = stock.CurrentPrice;
            ChangePercent = stock.ChangePercent;
        }
    }
}
