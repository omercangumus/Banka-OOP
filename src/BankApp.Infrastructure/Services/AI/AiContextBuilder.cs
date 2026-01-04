using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Infrastructure.Data;
using Dapper;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// Builds context data for AI requests by collecting real data from services
    /// All operations are read-only
    /// </summary>
    public class AiContextBuilder
    {
        private readonly PortfolioService _portfolioService;
        private readonly FinnhubService _finnhubService;
        private readonly DapperContext _dbContext;
        
        public AiContextBuilder()
        {
            _portfolioService = new PortfolioService();
            _finnhubService = new FinnhubService();
            _dbContext = new DapperContext();
        }
        
        /// <summary>
        /// Build complete context for AI request
        /// </summary>
        public async Task<AiContext> BuildContextAsync(int userId, string username, string? stockSymbol = null)
        {
            var context = new AiContext
            {
                Username = username
            };
            
            // Load data in parallel where possible
            var portfolioTask = LoadPortfolioDataAsync(context);
            var transactionTask = LoadTransactionDataAsync(context, userId);
            var accountTask = LoadAccountDataAsync(context, userId);
            
            await Task.WhenAll(portfolioTask, transactionTask, accountTask);
            
            // Load stock data if symbol provided
            if (!string.IsNullOrEmpty(stockSymbol))
            {
                await LoadStockDataAsync(context, stockSymbol);
            }
            
            return context;
        }
        
        /// <summary>
        /// Build context for stock analysis
        /// </summary>
        public async Task<AiContext> BuildStockContextAsync(int userId, string username, string symbol)
        {
            var context = await BuildContextAsync(userId, username, symbol);
            return context;
        }
        
        private async Task LoadPortfolioDataAsync(AiContext context)
        {
            try
            {
                context.NetWorth = await _portfolioService.GetNetWorthAsync();
                var (profit, percent) = await _portfolioService.GetTotalProfitLossAsync();
                context.TotalProfit = profit;
                context.ProfitPercent = (decimal)percent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPortfolioData Error: {ex.Message}");
            }
        }
        
        private async Task LoadTransactionDataAsync(AiContext context, int userId)
        {
            try
            {
                using var conn = _dbContext.CreateConnection();
                
                // Get recent transaction count and total spending
                var transactionData = await conn.QueryFirstOrDefaultAsync<(int Count, decimal Total)>(@"
                    SELECT 
                        COUNT(*) as Count,
                        COALESCE(SUM(ABS(""Amount"")), 0) as Total
                    FROM ""Transactions"" t
                    INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                    WHERE a.""CustomerId"" = @UserId 
                    AND t.""TransactionType"" IN ('Withdraw', 'TransferOut')
                    AND t.""TransactionDate"" >= CURRENT_DATE - INTERVAL '30 days'",
                    new { UserId = userId });
                
                context.RecentTransactionCount = transactionData.Count;
                context.TotalSpending = transactionData.Total;
                
                // Get spending by category
                var spendingByCategory = await conn.QueryAsync<(string Category, decimal Amount)>(@"
                    SELECT 
                        CASE 
                            WHEN ""Description"" LIKE '%Yatırım%' THEN 'Yatırım'
                            WHEN ""Description"" LIKE '%market%' OR ""Description"" LIKE '%Market%' THEN 'Market'
                            WHEN ""Description"" LIKE '%fatura%' THEN 'Faturalar'
                            WHEN ""Description"" LIKE '%kira%' THEN 'Kira'
                            ELSE 'Diğer'
                        END as Category,
                        SUM(ABS(""Amount"")) as Amount
                    FROM ""Transactions"" t
                    INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                    WHERE a.""CustomerId"" = @UserId 
                    AND t.""TransactionType"" IN ('Withdraw', 'TransferOut')
                    AND t.""TransactionDate"" >= CURRENT_DATE - INTERVAL '30 days'
                    GROUP BY Category
                    ORDER BY Amount DESC
                    LIMIT 5",
                    new { UserId = userId });
                
                var sb = new StringBuilder();
                foreach (var item in spendingByCategory)
                {
                    sb.Append($"{item.Category}: ₺{item.Amount:N0}, ");
                }
                context.SpendingByCategory = sb.ToString().TrimEnd(',', ' ');
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTransactionData Error: {ex.Message}");
            }
        }
        
        private async Task LoadAccountDataAsync(AiContext context, int userId)
        {
            try
            {
                using var conn = _dbContext.CreateConnection();
                
                var accountData = await conn.QueryFirstOrDefaultAsync<(int Count, decimal Total)>(@"
                    SELECT 
                        COUNT(*) as Count,
                        COALESCE(SUM(""Balance""), 0) as Total
                    FROM ""Accounts""
                    WHERE ""CustomerId"" = @UserId",
                    new { UserId = userId });
                
                context.AccountCount = accountData.Count;
                context.TotalBalance = accountData.Total;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAccountData Error: {ex.Message}");
            }
        }
        
        private async Task LoadStockDataAsync(AiContext context, string symbol)
        {
            try
            {
                context.StockSymbol = symbol;
                
                // Get quote
                var quote = await _finnhubService.GetQuoteAsync(symbol);
                if (quote != null)
                {
                    context.StockPrice = quote.C;
                    context.StockChange = quote.D;
                    context.StockChangePercent = quote.Dp;
                }
                
                // Get market news (general news, not company-specific)
                try
                {
                    var news = await _finnhubService.GetMarketNewsAsync();
                    if (news?.Count > 0)
                    {
                        var newsText = string.Join("; ", news.Take(3).Select(n => n.Headline));
                        context.StockNews = newsText;
                    }
                }
                catch { /* News is optional */ }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStockData Error: {ex.Message}");
            }
        }
    }
}
