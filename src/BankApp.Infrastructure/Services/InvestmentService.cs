using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Yatırım işlemleri servisi - Hisse alım/satım
    /// </summary>
    public class InvestmentService
    {
        private readonly MarketSimulatorService _marketSimulator;
        private readonly AccountRepository _accountRepo;
        private readonly AuditRepository _auditRepo;
        
        // In-memory portfolio (gerçek uygulamada DB'de olur)
        private static readonly List<CustomerPortfolio> _portfolios = new List<CustomerPortfolio>();

        public InvestmentService(MarketSimulatorService marketSimulator, AccountRepository accountRepo, AuditRepository auditRepo)
        {
            _marketSimulator = marketSimulator;
            _accountRepo = accountRepo;
            _auditRepo = auditRepo;
        }

        /// <summary>
        /// Hisse satın al
        /// </summary>
        public async Task<(bool Success, string Message)> BuyStockAsync(int customerId, int accountId, string symbol, decimal quantity)
        {
            try
            {
                // Hisseyi bul
                var stock = _marketSimulator.GetStock(symbol);
                if (stock == null)
                    return (false, $"Hisse bulunamadı: {symbol}");

                decimal totalCost = stock.CurrentPrice * quantity;

                // Hesap bakiyesini kontrol et
                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null)
                    return (false, "Hesap bulunamadı");

                if (account.Balance < totalCost)
                    return (false, $"Yetersiz bakiye. Gereken: {totalCost:N2} TL, Mevcut: {account.Balance:N2} TL");

                // Bakiyeden düş
                account.Balance -= totalCost;
                await _accountRepo.UpdateAsync(account);

                // Portföye ekle veya güncelle
                var existingPortfolio = _portfolios.FirstOrDefault(p => p.CustomerId == customerId && p.StockSymbol == symbol);
                
                if (existingPortfolio != null)
                {
                    // Ortalama maliyeti güncelle
                    var totalQuantity = existingPortfolio.Quantity + quantity;
                    existingPortfolio.AverageCost = 
                        ((existingPortfolio.Quantity * existingPortfolio.AverageCost) + (quantity * stock.CurrentPrice)) / totalQuantity;
                    existingPortfolio.Quantity = totalQuantity;
                }
                else
                {
                    _portfolios.Add(new CustomerPortfolio
                    {
                        Id = _portfolios.Count + 1,
                        CustomerId = customerId,
                        StockSymbol = symbol,
                        Quantity = quantity,
                        AverageCost = stock.CurrentPrice,
                        PurchaseDate = DateTime.Now
                    });
                }

                // Audit log
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = customerId,
                    Action = "BuyStock",
                    Details = $"{quantity} adet {symbol} hissesi {stock.CurrentPrice:N2} TL'den alındı. Toplam: {totalCost:N2} TL",
                    IpAddress = "127.0.0.1"
                });

                return (true, $"{quantity} adet {symbol} başarıyla alındı!");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Hisse sat
        /// </summary>
        public async Task<(bool Success, string Message)> SellStockAsync(int customerId, int accountId, string symbol, decimal quantity)
        {
            try
            {
                // Portföyü kontrol et
                var portfolio = _portfolios.FirstOrDefault(p => p.CustomerId == customerId && p.StockSymbol == symbol);
                if (portfolio == null || portfolio.Quantity < quantity)
                    return (false, "Yeterli hisse yok");

                var stock = _marketSimulator.GetStock(symbol);
                if (stock == null)
                    return (false, $"Hisse bulunamadı: {symbol}");

                decimal totalValue = stock.CurrentPrice * quantity;

                // Hesaba ekle
                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null)
                    return (false, "Hesap bulunamadı");

                account.Balance += totalValue;
                await _accountRepo.UpdateAsync(account);

                // Portföyden düş
                portfolio.Quantity -= quantity;
                if (portfolio.Quantity <= 0)
                    _portfolios.Remove(portfolio);

                // Audit log
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = customerId,
                    Action = "SellStock",
                    Details = $"{quantity} adet {symbol} hissesi {stock.CurrentPrice:N2} TL'den satıldı. Toplam: {totalValue:N2} TL",
                    IpAddress = "127.0.0.1"
                });

                return (true, $"{quantity} adet {symbol} satıldı! {totalValue:N2} TL hesaba eklendi.");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Müşterinin portföyünü getir
        /// </summary>
        public List<PortfolioItem> GetPortfolio(int customerId)
        {
            var result = new List<PortfolioItem>();
            var customerPortfolios = _portfolios.Where(p => p.CustomerId == customerId);

            foreach (var p in customerPortfolios)
            {
                var stock = _marketSimulator.GetStock(p.StockSymbol);
                if (stock != null)
                {
                    var currentValue = p.Quantity * stock.CurrentPrice;
                    var totalCost = p.Quantity * p.AverageCost;
                    var profitLoss = currentValue - totalCost;
                    var profitLossPercent = totalCost > 0 ? (profitLoss / totalCost) * 100 : 0;

                    result.Add(new PortfolioItem
                    {
                        StockSymbol = p.StockSymbol,
                        StockName = stock.Name,
                        Quantity = p.Quantity,
                        AverageCost = p.AverageCost,
                        CurrentPrice = stock.CurrentPrice,
                        TotalCost = totalCost,
                        CurrentValue = currentValue,
                        ProfitLoss = profitLoss,
                        ProfitLossPercent = profitLossPercent
                    });
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Portföy görüntüleme modeli
    /// </summary>
    public class PortfolioItem
    {
        public string StockSymbol { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercent { get; set; }
    }
}
