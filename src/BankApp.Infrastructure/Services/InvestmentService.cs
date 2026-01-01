using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Yatırım işlemleri servisi - Hisse alım/satım yönetimi
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class InvestmentService
    {
        private readonly MarketSimulatorService _marketSimulator;
        private readonly AccountRepository _accountRepo;
        private readonly AuditRepository _auditRepo;
        
        // In-memory portfolio simulasyonu
        private static readonly List<CustomerPortfolio> _portfolios = new List<CustomerPortfolio>();

        /// <summary>
        /// Servis yapıcı metodu
        /// </summary>
        /// <param name="marketSimulator">Piyasa simülatörü</param>
        /// <param name="accountRepo">Hesap repository</param>
        /// <param name="auditRepo">Denetim repository</param>
        public InvestmentService(MarketSimulatorService marketSimulator, AccountRepository accountRepo, AuditRepository auditRepo)
        {
            _marketSimulator = marketSimulator;
            _accountRepo = accountRepo;
            _auditRepo = auditRepo;
        }

        /// <summary>
        /// Hisse satın alma işlemi
        /// </summary>
        /// <param name="customerId">Müşteri ID</param>
        /// <param name="accountId">Kullanılacak hesap ID</param>
        /// <param name="symbol">Hisse sembolü</param>
        /// <param name="quantity">Adet</param>
        /// <returns>Başarılı ise null, hata varsa hata mesajı</returns>
        public async Task<string?> BuyStockAsync(int customerId, int accountId, string symbol, decimal quantity)
        {
            try
            {
                var stock = _marketSimulator.GetStock(symbol);
                if (stock == null)
                    return $"Hisse bulunamadı: {symbol}";

                decimal totalCost = stock.CurrentPrice * quantity;

                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null)
                    return "Hesap bulunamadı";

                if (account.Balance < totalCost)
                {
                    var sb = new StringBuilder();
                    sb.Append("Yetersiz bakiye. Gereken: ");
                    sb.Append(totalCost.ToString("N2"));
                    sb.Append(" TL, Mevcut: ");
                    sb.Append(account.Balance.ToString("N2"));
                    sb.Append(" TL");
                    return sb.ToString();
                }

                // Bakiyeden düş
                account.Balance -= totalCost;
                await _accountRepo.UpdateAsync(account);

                // Portföye ekle
                var existingPortfolio = _portfolios.FirstOrDefault(p => p.CustomerId == customerId && p.StockSymbol == symbol);
                
                if (existingPortfolio != null)
                {
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

                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = customerId,
                    Action = "BuyStock",
                    Details = $"{quantity} adet {symbol} hissesi {stock.CurrentPrice:N2} TL'den alındı. Toplam: {totalCost:N2} TL",
                    IpAddress = "127.0.0.1"
                });

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                return "İşlem sırasında bir hata oluştu: " + ex.Message;
            }
        }

        /// <summary>
        /// Hisse satış işlemi
        /// </summary>
        /// <param name="customerId">Müşteri ID</param>
        /// <param name="accountId">Yatırılacak hesap ID</param>
        /// <param name="symbol">Hisse sembolü</param>
        /// <param name="quantity">Adet</param>
        /// <returns>Başarılı ise null, hata varsa hata mesajı</returns>
        public async Task<string?> SellStockAsync(int customerId, int accountId, string symbol, decimal quantity)
        {
            try
            {
                var portfolio = _portfolios.FirstOrDefault(p => p.CustomerId == customerId && p.StockSymbol == symbol);
                if (portfolio == null || portfolio.Quantity < quantity)
                    return "Satılacak yeterli hisse adedi bulunamadı.";

                var stock = _marketSimulator.GetStock(symbol);
                if (stock == null)
                    return $"Hisse piyasada bulunamadı: {symbol}";

                decimal totalValue = stock.CurrentPrice * quantity;

                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null)
                    return "Hedef hesap bulunamadı.";

                account.Balance += totalValue;
                await _accountRepo.UpdateAsync(account);

                portfolio.Quantity -= quantity;
                if (portfolio.Quantity <= 0)
                    _portfolios.Remove(portfolio);

                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = customerId,
                    Action = "SellStock",
                    Details = $"{quantity} adet {symbol} hissesi {stock.CurrentPrice:N2} TL'den satıldı. Toplam: {totalValue:N2} TL",
                    IpAddress = "127.0.0.1"
                });

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                return "Satış işlemi başarısız: " + ex.Message;
            }
        }

        /// <summary>
        /// Müşteri portföyünü listeler
        /// </summary>
        /// <param name="customerId">Müşteri ID</param>
        /// <returns>Portföy listesi</returns>
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
                    var profitLossPercent = totalCost > 0 ? (profitLoss / totalCost) : 0; // Yüzdeyi 100 ile çarpmadım, format string halleder (örn: P2) ama UI manual format kullanıyor. UI'daki format +0.00% şeklinde, bu yüzden raw değer (0.15 gibi) mi yoksa 15.0 mi bekliyor?
                    // UI Format string: "+0.00%;-0.00%;0.00%"
                    // Eğer 0.15 gelirse ve format % ise, 15% yazar. Eğer format numerikse 0.15 yazar. 
                    // Eski kod: (profitLoss / totalCost) * 100 yapıyordu.
                    // Sanırım UI numerik değer bekliyor, yüzdelik değil. Eski koda sadık kalalım.
                    
                    // DÜZELTME: Eski kod * 100 yapıyordu. UI Column display format: "+0.00%;-0.00%;0.00%"
                    // Standard DevExpress display format "%" sembolünü otomatik eklemez eğer format string içinde literal yoksa.
                    // Format string "+0.00%" literal % içeriyor. Bu durumda değer 15 gelirse 15.00% yazar. 
                    // 0.15 gelirse 0.15% yazar.
                    // Yani değerin 100 ile çarpılmış olması lazım. Eski kod doğruydu.
                    
                    if (totalCost > 0)
                         profitLossPercent = (profitLoss / totalCost) * 100;

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
