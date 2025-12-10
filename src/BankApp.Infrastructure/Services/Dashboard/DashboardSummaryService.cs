using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Infrastructure.Data;
using Dapper;

namespace BankApp.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// Dashboard için gerçek veri hesaplamaları
    /// </summary>
    public class DashboardSummaryService
    {
        private readonly DapperContext _context;

        public DashboardSummaryService(DapperContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Dashboard için tüm verileri tek seferde çek (performans için)
        /// </summary>
        public async Task<DashboardFullData> GetFullDashboardDataAsync(int userId)
        {
            var data = new DashboardFullData();
            
            System.Diagnostics.Debug.WriteLine($"[DASHBOARD] GetFullDashboardDataAsync started - UserId: {userId}");
            
            try
            {
                using var conn = _context.CreateConnection();
                
                // Get customer ID
                var customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });
                
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] CustomerId lookup: UserId={userId} -> CustomerId={customerId}");
                
                if (!customerId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[DASHBOARD] ❌ No Customer found for UserId={userId}! Returning empty data.");
                    return data;
                }
                
                // 1. Total Balance (tüm hesaplar)
                data.TotalBalance = await conn.QueryFirstOrDefaultAsync<decimal?>(
                    "SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId",
                    new { CustomerId = customerId.Value }) ?? 0;
                
                // 2. Active Accounts count
                data.ActiveAccounts = await conn.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId",
                    new { CustomerId = customerId.Value });
                
                // 3. IBAN (ilk hesap)
                data.PrimaryIban = await conn.QueryFirstOrDefaultAsync<string>(
                    "SELECT \"IBAN\" FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId ORDER BY \"Balance\" DESC LIMIT 1",
                    new { CustomerId = customerId.Value }) ?? "";
                
                // 4. Total Debt (Approved loans)
                var loans = await conn.QueryAsync<dynamic>(@"
                    SELECT ""Amount"", ""TermMonths"", ""InterestRate""
                    FROM ""Loans""
                    WHERE ""CustomerId"" = @CustomerId AND ""Status"" = 'Approved'",
                    new { CustomerId = customerId.Value });
                
                foreach (var loan in loans)
                {
                    decimal interestAmount = (decimal)loan.Amount * ((decimal)loan.InterestRate / 100) * ((decimal)loan.TermMonths / 12);
                    data.TotalDebt += (decimal)loan.Amount + interestAmount;
                }
                
                // 5. Net Worth
                data.NetWorth = data.TotalBalance - data.TotalDebt;
                
                // 6. 30-day change (son 30 gün gelir - gider)
                var monthlyStats = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT 
                        COALESCE(SUM(CASE WHEN ""TransactionType"" IN ('Deposit', 'TransferIn') THEN ""Amount"" ELSE 0 END), 0) as Income,
                        COALESCE(SUM(CASE WHEN ""TransactionType"" IN ('Withdraw', 'TransferOut') THEN ""Amount"" ELSE 0 END), 0) as Expense
                    FROM ""Transactions"" t
                    INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                    WHERE a.""CustomerId"" = @CustomerId
                    AND t.""TransactionDate"" >= NOW() - INTERVAL '30 days'",
                    new { CustomerId = customerId.Value });
                
                if (monthlyStats != null)
                {
                    data.MonthlyChange = (decimal)monthlyStats.Income - (decimal)monthlyStats.Expense;
                }
                
                // 7. Portfolio Value (yatırımlar)
                try
                {
                    data.PortfolioValue = await conn.QueryFirstOrDefaultAsync<decimal?>(
                        @"SELECT COALESCE(SUM(""Quantity"" * ""AverageCost""), 0) 
                          FROM ""CustomerPortfolios"" 
                          WHERE ""CustomerId"" = @CustomerId",
                        new { CustomerId = customerId.Value }) ?? 0;
                }
                catch { }
                
                // DEBUG: Log all values
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] ✅ Data loaded for CustomerId={customerId}:");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD]   TotalBalance: {data.TotalBalance:N2}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD]   TotalDebt: {data.TotalDebt:N2}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD]   NetWorth: {data.NetWorth:N2}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD]   MonthlyChange: {data.MonthlyChange:N2}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD]   ActiveAccounts: {data.ActiveAccounts}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD]   PortfolioValue: {data.PortfolioValue:N2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] ❌ GetFullDashboardDataAsync Error: {ex.Message}");
            }
            
            return data;
        }

        /// <summary>
        /// Toplam aktif kredi borcu
        /// </summary>
        public async Task<decimal> GetTotalDebtAsync(int userId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                
                // Get customer ID first
                var customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });
                
                if (!customerId.HasValue) return 0;
                
                // Calculate total debt from active loans
                var loans = await conn.QueryAsync<dynamic>(@"
                    SELECT 
                        Amount,
                        TermMonths,
                        InterestRate,
                        Status,
                        CreatedDate
                    FROM ""Loans""
                    WHERE ""CustomerId"" = @CustomerId AND ""Status"" = 'Approved'");
                
                decimal totalDebt = 0;
                foreach (var loan in loans)
                {
                    // Simple calculation: principal + interest
                    decimal interestAmount = loan.Amount * (loan.InterestRate / 100) * (loan.TermMonths / 12);
                    totalDebt += loan.Amount + interestAmount;
                }
                
                return totalDebt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTotalDebtAsync Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Net varlık hesapla
        /// </summary>
        public async Task<decimal> GetNetWorthAsync(int userId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                
                // Get customer ID first
                var customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });
                
                if (!customerId.HasValue) return 0;
                
                // Get total account balance
                var accounts = await conn.QueryAsync<decimal>(
                    "SELECT \"Balance\" FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId",
                    new { CustomerId = customerId.Value });
                
                decimal totalBalance = accounts.Sum();
                
                // Get total debt
                decimal totalDebt = await GetTotalDebtAsync(userId);
                
                // Net Worth = Total Balance - Total Debt
                return totalBalance - totalDebt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetNetWorthAsync Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Harcama dağılımını kategoriye göre getir
        /// </summary>
        public async Task<List<ExpenseSlice>> GetExpenseDistributionAsync(int userId, DateTime from, DateTime to)
        {
            var slices = new List<ExpenseSlice>();
            
            try
            {
                using var conn = _context.CreateConnection();
                
                // Get customer ID first
                var customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });
                
                if (!customerId.HasValue) return slices;
                
                // Get expense transactions
                var transactions = await conn.QueryAsync<dynamic>(@"
                    SELECT 
                        Description,
                        Amount,
                        TransactionType
                    FROM ""Transactions""
                    WHERE ""CustomerId"" = @CustomerId 
                    AND ""TransactionType"" IN ('Withdraw', 'TransferOut')
                    AND ""TransactionDate"" >= @From 
                    AND ""TransactionDate"" <= @To
                    ORDER BY Amount DESC",
                    new { 
                        CustomerId = customerId.Value,
                        From = from,
                        To = to
                    });
                
                // Group by description/category
                var grouped = transactions
                    .GroupBy(t => t.Description?.ToString() ?? "Diğer")
                    .Select(g => new ExpenseSlice
                    {
                        Category = g.Key,
                        Amount = g.Sum(t => (decimal)t.Amount)
                    })
                    .OrderByDescending(s => s.Amount)
                    .Take(8) // Top 8 categories
                    .ToList();
                
                slices = grouped;
                
                // If no categories, add default
                if (!slices.Any())
                {
                    slices.Add(new ExpenseSlice { Category = "Henüz harcama yok", Amount = 0 });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetExpenseDistributionAsync Error: {ex.Message}");
                slices.Add(new ExpenseSlice { Category = "Veri yüklenemedi", Amount = 0 });
            }
            
            return slices;
        }
        
        /// <summary>
        /// Asset Allocation (Nakit / Hisse / Kripto) - Pie Chart için
        /// CustomerPortfolios tablosunu kullanır
        /// </summary>
        public async Task<List<AssetAllocationSlice>> GetAssetAllocationAsync(int userId)
        {
            var slices = new List<AssetAllocationSlice>();
            decimal cashTotal = 0;
            decimal stockTotal = 0;
            decimal cryptoTotal = 0;
            
            try
            {
                using var conn = _context.CreateConnection();
                
                // Get customer ID
                var customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });
                
                if (!customerId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[ASSET] No customer for userId={userId}");
                    return slices;
                }
                
                // 1. Nakit (Accounts toplamı)
                cashTotal = await conn.QueryFirstOrDefaultAsync<decimal?>(
                    "SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId",
                    new { CustomerId = customerId.Value }) ?? 0;
                
                // 2. Yatırım pozisyonları (CustomerPortfolios)
                try
                {
                    var positions = await conn.QueryAsync<dynamic>(
                        @"SELECT ""StockSymbol"", ""Quantity"", ""AverageCost"" 
                          FROM ""CustomerPortfolios"" 
                          WHERE ""CustomerId"" = @CustomerId AND ""Quantity"" > 0",
                        new { CustomerId = customerId.Value });
                    
                    foreach (var pos in positions)
                    {
                        string symbol = pos.StockSymbol?.ToString()?.ToUpper() ?? "";
                        decimal qty = (decimal)pos.Quantity;
                        decimal avgCost = (decimal)pos.AverageCost;
                        decimal value = qty * avgCost; // TODO: Use current price from market service
                        
                        // Classify by symbol
                        if (IsCrypto(symbol))
                        {
                            cryptoTotal += value;
                        }
                        else
                        {
                            stockTotal += value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ASSET] Portfolio query error: {ex.Message}");
                }
                
                // Build slices
                if (cashTotal > 0)
                {
                    slices.Add(new AssetAllocationSlice 
                    { 
                        Category = "Nakit", 
                        Amount = cashTotal,
                        Color = "#4CAF50" // Green
                    });
                }
                
                if (stockTotal > 0)
                {
                    slices.Add(new AssetAllocationSlice 
                    { 
                        Category = "Hisse", 
                        Amount = stockTotal,
                        Color = "#2196F3" // Blue
                    });
                }
                
                if (cryptoTotal > 0)
                {
                    slices.Add(new AssetAllocationSlice 
                    { 
                        Category = "Kripto", 
                        Amount = cryptoTotal,
                        Color = "#FF9800" // Orange
                    });
                }
                
                // Empty state
                if (!slices.Any())
                {
                    slices.Add(new AssetAllocationSlice 
                    { 
                        Category = "Veri yok", 
                        Amount = 1,
                        Color = "#757575"
                    });
                }
                
                System.Diagnostics.Debug.WriteLine($"[ASSET] AssetAllocation slices={slices.Count} cash={cashTotal:N0} stock={stockTotal:N0} crypto={cryptoTotal:N0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ASSET] GetAssetAllocationAsync Error: {ex.Message}");
                slices.Add(new AssetAllocationSlice { Category = "Yüklenemedi", Amount = 1, Color = "#757575" });
            }
            
            return slices;
        }
        
        private static bool IsCrypto(string symbol)
        {
            var cryptoSymbols = new[] { "BTC", "ETH", "XRP", "SOL", "ADA", "DOGE", "DOT", "AVAX", "MATIC", "LINK", "UNI", "ATOM" };
            return cryptoSymbols.Contains(symbol) || symbol.EndsWith("USD") || symbol.EndsWith("USDT");
        }
        
        /// <summary>
        /// Toplam varlık (Nakit + Yatırım)
        /// </summary>
        public async Task<decimal> GetTotalAssetsAsync(int userId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                
                var customerId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });
                
                if (!customerId.HasValue) return 0;
                
                // Cash
                var cash = await conn.QueryFirstOrDefaultAsync<decimal?>(
                    "SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId",
                    new { CustomerId = customerId.Value }) ?? 0;
                
                // Investments
                decimal investments = 0;
                try
                {
                    investments = await conn.QueryFirstOrDefaultAsync<decimal?>(
                        @"SELECT COALESCE(SUM(""Quantity"" * ""CurrentPrice""), 0) 
                          FROM ""UserStocks"" 
                          WHERE ""UserId"" = @UserId AND ""Quantity"" > 0",
                        new { UserId = userId }) ?? 0;
                }
                catch { }
                
                return cash + investments;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTotalAssetsAsync Error: {ex.Message}");
                return 0;
            }
        }
    }

    /// <summary>
    /// Harcama dilimi DTO
    /// </summary>
    public sealed class ExpenseSlice
    {
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
    }
    
    /// <summary>
    /// Asset Allocation dilimi DTO
    /// </summary>
    public sealed class AssetAllocationSlice
    {
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
        public string Color { get; set; } = "#757575";
    }
    
    /// <summary>
    /// Dashboard için tüm veriler DTO
    /// </summary>
    public sealed class DashboardFullData
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal NetWorth { get; set; }
        public decimal MonthlyChange { get; set; }
        public decimal PortfolioValue { get; set; }
        public int ActiveAccounts { get; set; }
        public string PrimaryIban { get; set; } = "";
    }
}
