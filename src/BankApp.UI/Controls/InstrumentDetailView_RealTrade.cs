using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Events;
using BankApp.Core.Entities;
using Dapper;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// InstrumentDetailView için GERÇEK trade metodu
    /// </summary>
    public partial class InstrumentDetailView
    {
        private DapperContext _context = new DapperContext();
        private AccountRepository _accountRepository;
        private TransactionService _transactionService;
        private CustomerPortfolioRepository _portfolioRepository;
        private PendingOrderRepository _pendingOrderRepository;
        
        private void InitializeTradeServices()
        {
            var accountRepo = new AccountRepository(_context);
            var transactionRepo = new TransactionRepository(_context);
            var auditRepo = new AuditRepository(_context);
            
            _accountRepository = accountRepo;
            _transactionService = new TransactionService(accountRepo, transactionRepo, auditRepo);
            _portfolioRepository = new CustomerPortfolioRepository(_context);
            _pendingOrderRepository = new PendingOrderRepository(_context);
        }
        private bool _isTrading = false;
        
        private async Task ExecuteRealTradeAsync(bool isBuy)
        {
            // [CRITICAL] TradeStart - ZORUNLU
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] TradeStart viewType={GetType().FullName} viewHash={GetHashCode()} isBuy={isBuy} symbol={_currentSymbol} userId={AppEvents.CurrentSession.UserId}");
            
            // Initialize services if not already done
            if (_transactionService == null)
            {
                InitializeTradeServices();
            }
            
            if (_isTrading) return;
            _isTrading = true;
            
            try
            {
                // Parse quantity
                if (!decimal.TryParse(txtAmount.Text, out decimal quantity) || quantity <= 0)
                {
                    MessageBox.Show("Geçerli bir miktar giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Get price and total from UI (txtPrice, txtTotal)
                decimal price = 100m; // Fallback
                decimal totalAmount = quantity * price; // Will be recalculated from txtTotal
                
                // Try to get actual total from txtTotal (USDT)
                if (!string.IsNullOrEmpty(txtTotal.Text) && decimal.TryParse(txtTotal.Text.Replace(",", ""), out decimal parsedTotal))
                {
                    totalAmount = parsedTotal; // Use UI calculated total (includes commission)
                    if (quantity > 0)
                    {
                        price = totalAmount / quantity; // Recalculate price from total
                    }
                }
                else if (!string.IsNullOrEmpty(txtPrice.Text))
                {
                    // Try txtPrice
                    var priceText = txtPrice.Text.Replace("Market Fiyab", "").Replace(",", "").Trim();
                    if (decimal.TryParse(priceText, out decimal parsedPrice))
                    {
                        price = parsedPrice;
                        totalAmount = quantity * price;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[TRADE] Calculated: price={price:N2}, qty={quantity}, total={totalAmount:N2}");
                
                // Get user's primary account
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] InstrumentDetailView Trade START - UserId={AppEvents.CurrentSession.UserId}");
                
                // 1. Get Customer from UserId
                int? customerId = null;
                using (var conn = _context.CreateConnection())
                {
                    customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                        "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                        new { UserId = AppEvents.CurrentSession.UserId });
                }
                
                if (!customerId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[CRITICAL] ERROR: No Customer found for UserId={AppEvents.CurrentSession.UserId}");
                    MessageBox.Show($"Müşteri bulunamadı (UserId={AppEvents.CurrentSession.UserId}).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Customer found: UserId={AppEvents.CurrentSession.UserId} -> CustomerId={customerId.Value}");
                
                // 2. Get Account from CustomerId
                var accounts = await _accountRepository.GetByCustomerIdAsync(customerId.Value);
                var primaryAccount = accounts?.FirstOrDefault();
                
                if (primaryAccount == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CRITICAL] ERROR: No account found for CustomerId={customerId.Value}");
                    MessageBox.Show($"Hesap bulunamadı (CustomerId={customerId.Value}).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Trade - AccountId={primaryAccount.Id}, CustomerId={primaryAccount.CustomerId}, UserId={AppEvents.CurrentSession.UserId}");
                
                // TÜM HESAPLARIN TOPLAM BAKİYESİNİ AL
                decimal totalBalance = accounts.Sum(a => a.Balance);
                int accountCount = accounts.Count();
                
                // Emir tipini kontrol et
                var orderType = cmbOrderType?.EditValue?.ToString() ?? "Market";
                System.Diagnostics.Debug.WriteLine($"[TRADE] OrderType={orderType} isBuy={isBuy} symbol={_currentSymbol} qty={quantity} price={price}");
                
                // LİMİT/STOP EMİR: Para çekilmez, bekleyen emirlere eklenir
                if (orderType != "Market")
                {
                    // Limit fiyatı al
                    decimal limitPrice = price;
                    if (!string.IsNullOrWhiteSpace(txtPrice.Text) && decimal.TryParse(txtPrice.Text.Replace(",", ""), out decimal parsedLimit))
                    {
                        limitPrice = parsedLimit;
                    }
                    
                    // Bekleyen emir oluştur
                    var pendingOrder = new PendingOrder
                    {
                        CustomerId = primaryAccount.CustomerId,
                        AccountId = primaryAccount.Id,
                        Symbol = _currentSymbol,
                        OrderType = orderType,
                        Side = isBuy ? "Buy" : "Sell",
                        Quantity = quantity,
                        LimitPrice = limitPrice,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    var orderId = await _pendingOrderRepository.CreateAsync(pendingOrder);
                    
                    System.Diagnostics.Debug.WriteLine($"[CRITICAL] PendingOrder created Id={orderId} Type={orderType} Side={pendingOrder.Side} Symbol={_currentSymbol} Qty={quantity} LimitPrice={limitPrice}");
                    
                    // Kullanıcıya bilgi ver
                    DevExpress.XtraEditors.XtraMessageBox.Show(
                        $"✅ {orderType} Emir Oluşturuldu!\n\nEmir No: #{orderId}\nSembol: {_currentSymbol}\nYön: {(isBuy ? "AL" : "SAT")}\nMiktar: {quantity}\nLimit Fiyat: ${limitPrice:N2}\n\n📌 Bu emir Portföy → Bekleyen Emirler kısmında görünecek.\nFiyat hedefe ulaşınca otomatik işlenecek.\n\n❌ İptal etmek için Portföy'den iptal edebilirsiniz.",
                        "Bekleyen Emir",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    // Event tetikle (portföy güncellensin)
                    PortfolioEvents.OnPortfolioChanged(AppEvents.CurrentSession.UserId, "PendingOrder");
                    return;
                }
                
                // MARKET EMİR: Direkt işlem yap
                string result;
                string actionType;
                
                if (isBuy)
                {
                    // BAKİYE KONTROLÜ
                    if (totalBalance < totalAmount)
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show(
                            $"Yetersiz bakiye!\n\nMevcut: ₺{totalBalance:N2}\nGerekli: ₺{totalAmount:N2}",
                            "Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                    
                    // AL: Withdraw from account
                    result = await _transactionService.WithdrawAsync(
                        primaryAccount.Id, 
                        totalAmount, 
                        $"Yatırım AL: {quantity} adet {_currentSymbol} @ ${price:N2}");
                    actionType = "StockBuy";
                    
                    if (result == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CRITICAL] BuyAsync - CustomerId={primaryAccount.CustomerId}, Symbol={_currentSymbol}, Qty={quantity}");
                        await _portfolioRepository.BuyAsync(primaryAccount.CustomerId, _currentSymbol, quantity, price);
                    }
                }
                else
                {
                    // SAT: Check position first
                    System.Diagnostics.Debug.WriteLine($"[CRITICAL] SellAsync - CustomerId={primaryAccount.CustomerId}, Symbol={_currentSymbol}, Qty={quantity}");
                    var canSell = await _portfolioRepository.SellAsync(primaryAccount.CustomerId, _currentSymbol, quantity);
                    if (!canSell)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CRITICAL] SELL FAILED: Insufficient position for {_currentSymbol}");
                        MessageBox.Show($"Yetersiz {_currentSymbol} pozisyonu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    // SAT: Deposit to account
                    result = await _transactionService.DepositAsync(
                        primaryAccount.Id, 
                        totalAmount, 
                        $"Yatırım SAT: {quantity} adet {_currentSymbol} @ ${price:N2}");
                    actionType = "StockSell";
                }
                
                if (result == null) // Success
                {
                    decimal currentBalance = 0;
                    
                    // DB Verification
                    try
                    {
                        using var conn = new DapperContext().CreateConnection();
                        currentBalance = await conn.ExecuteScalarAsync<decimal>(
                            "SELECT \"Balance\" FROM \"Accounts\" WHERE \"Id\" = @AccId", 
                            new { AccId = primaryAccount.Id });
                    }
                    catch { }
                    
                    // B3: TEK RefreshPipeline - NotifyTradeCompleted
                    AppEvents.NotifyTradeCompleted(
                        primaryAccount.Id,
                        primaryAccount.CustomerId,
                        _currentSymbol,
                        totalAmount,
                        isBuy);
                    
                    // Legacy events (opsiyonel - yedek olarak)
                    PortfolioEvents.OnPortfolioChanged(AppEvents.CurrentSession.UserId, "Trade");
                    
                    // Trade başarılı - sadece log
                    string tradeType = isBuy ? "AL" : "SAT";
                    System.Diagnostics.Debug.WriteLine($"[TRADE] {tradeType} SUCCESS - Symbol={_currentSymbol}, Qty={quantity}, Balance={currentBalance:N2}");
                }
                else
                {
                    MessageBox.Show(result, "İşlem Başarısız", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TRADE ERROR] {ex.Message}");
                MessageBox.Show("İşlem sırasında bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isTrading = false;
            }
        }
    }
}
