using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Infrastructure.Data;
using Dapper;

namespace BankApp.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// Dashboard veri servisi - DB'den aggregate veriler çeker
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly DapperContext _context;

        public DashboardService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(int userId)
        {
            try
            {
                using var connection = _context.CreateConnection();
                
                // Get customer ID for this user
                var customerId = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });

                if (!customerId.HasValue)
                {
                    return GetEmptySummary();
                }

                // Total Balance from Accounts
                var totalBalance = await connection.QueryFirstOrDefaultAsync<decimal>(
                    "SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId",
                    new { CustomerId = customerId.Value });

                // Total Assets (placeholder - would need Assets table)
                var totalAssets = 0m;

                // Total Debt from Active Loans
                var totalDebt = await connection.QueryFirstOrDefaultAsync<decimal>(
                    @"SELECT COALESCE(SUM(""Amount"" * (1 + ""InterestRate"" / 100 * ""TermMonths"" / 12)), 0) 
                      FROM ""Loans"" 
                      WHERE ""CustomerId"" = @CustomerId AND ""Status"" = 'Approved'",
                    new { CustomerId = customerId.Value });

                // Active Loan Count
                var activeLoanCount = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM \"Loans\" WHERE \"CustomerId\" = @CustomerId AND \"Status\" = 'Approved'",
                    new { CustomerId = customerId.Value });

                // Monthly Spending (last 30 days - Withdraw + TransferOut)
                var monthlySpending = await connection.QueryFirstOrDefaultAsync<decimal>(
                    @"SELECT COALESCE(SUM(""Amount""), 0) 
                      FROM ""Transactions"" t
                      INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                      WHERE a.""CustomerId"" = @CustomerId 
                        AND t.""TransactionDate"" >= @FromDate
                        AND t.""TransactionType"" IN ('Withdraw', 'TransferOut')",
                    new { CustomerId = customerId.Value, FromDate = DateTime.UtcNow.AddDays(-30) });

                // Monthly Income (last 30 days - Deposit + TransferIn)
                var monthlyIncome = await connection.QueryFirstOrDefaultAsync<decimal>(
                    @"SELECT COALESCE(SUM(""Amount""), 0) 
                      FROM ""Transactions"" t
                      INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                      WHERE a.""CustomerId"" = @CustomerId 
                        AND t.""TransactionDate"" >= @FromDate
                        AND t.""TransactionType"" IN ('Deposit', 'TransferIn')",
                    new { CustomerId = customerId.Value, FromDate = DateTime.UtcNow.AddDays(-30) });

                // Total Transaction Count
                var totalTransactionCount = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) 
                      FROM ""Transactions"" t
                      INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                      WHERE a.""CustomerId"" = @CustomerId",
                    new { CustomerId = customerId.Value });

                return new DashboardSummaryDto
                {
                    TotalBalance = totalBalance,
                    TotalAssets = totalAssets,
                    TotalDebt = totalDebt,
                    NetWorth = totalBalance + totalAssets - totalDebt,
                    MonthlySpending = monthlySpending,
                    MonthlyIncome = monthlyIncome,
                    ActiveLoanCount = activeLoanCount,
                    TotalTransactionCount = totalTransactionCount
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardService] GetSummaryAsync error: {ex.Message}");
                return GetEmptySummary();
            }
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int userId, int take = 10)
        {
            try
            {
                using var connection = _context.CreateConnection();
                
                var customerId = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });

                if (!customerId.HasValue)
                {
                    return new List<RecentTransactionDto>();
                }

                var transactions = await connection.QueryAsync<RecentTransactionDto>(
                    @"SELECT t.""Id"", t.""TransactionType"", t.""Amount"", t.""Description"", 
                             t.""TransactionDate"", a.""AccountNumber""
                      FROM ""Transactions"" t
                      INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                      WHERE a.""CustomerId"" = @CustomerId
                      ORDER BY t.""TransactionDate"" DESC
                      LIMIT @Take",
                    new { CustomerId = customerId.Value, Take = take });

                return transactions.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardService] GetRecentTransactionsAsync error: {ex.Message}");
                return new List<RecentTransactionDto>();
            }
        }

        public async Task<List<PieSliceDto>> GetSpendingDistributionAsync(int userId, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var connection = _context.CreateConnection();
                
                var customerId = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });

                if (!customerId.HasValue)
                {
                    return new List<PieSliceDto>();
                }

                var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
                var toDate = to ?? DateTime.UtcNow;

                var distribution = await connection.QueryAsync<dynamic>(
                    @"SELECT t.""TransactionType"" as Category, SUM(t.""Amount"") as Amount
                      FROM ""Transactions"" t
                      INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                      WHERE a.""CustomerId"" = @CustomerId
                        AND t.""TransactionDate"" >= @FromDate
                        AND t.""TransactionDate"" <= @ToDate
                        AND t.""TransactionType"" IN ('Withdraw', 'TransferOut')
                      GROUP BY t.""TransactionType""",
                    new { CustomerId = customerId.Value, FromDate = fromDate, ToDate = toDate });

                var result = distribution.Select(d => new PieSliceDto
                {
                    Category = (string)d.Category,
                    Amount = (decimal)d.Amount,
                    Percentage = 0
                }).ToList();

                // Calculate percentages
                var total = result.Sum(r => r.Amount);
                if (total > 0)
                {
                    foreach (var item in result)
                    {
                        item.Percentage = (double)(item.Amount / total * 100);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardService] GetSpendingDistributionAsync error: {ex.Message}");
                return new List<PieSliceDto>();
            }
        }

        public async Task<List<PieSliceDto>> GetAssetDistributionAsync(int userId)
        {
            try
            {
                using var connection = _context.CreateConnection();
                
                var customerId = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = userId });

                if (!customerId.HasValue)
                {
                    return new List<PieSliceDto>();
                }

                var distribution = await connection.QueryAsync<dynamic>(
                    @"SELECT ""CurrencyCode"" as Category, SUM(""Balance"") as Amount
                      FROM ""Accounts""
                      WHERE ""CustomerId"" = @CustomerId
                      GROUP BY ""CurrencyCode""",
                    new { CustomerId = customerId.Value });

                var result = distribution.Select(d => new PieSliceDto
                {
                    Category = (string)d.Category,
                    Amount = (decimal)d.Amount,
                    Percentage = 0
                }).ToList();

                // Calculate percentages
                var total = result.Sum(r => r.Amount);
                if (total > 0)
                {
                    foreach (var item in result)
                    {
                        item.Percentage = (double)(item.Amount / total * 100);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardService] GetAssetDistributionAsync error: {ex.Message}");
                return new List<PieSliceDto>();
            }
        }

        private DashboardSummaryDto GetEmptySummary()
        {
            return new DashboardSummaryDto
            {
                NetWorth = 0,
                TotalDebt = 0,
                TotalBalance = 0,
                TotalAssets = 0,
                MonthlySpending = 0,
                MonthlyIncome = 0,
                ActiveLoanCount = 0,
                TotalTransactionCount = 0
            };
        }
    }
}
