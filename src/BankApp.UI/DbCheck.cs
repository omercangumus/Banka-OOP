using System;
using System.IO;
using System.Threading.Tasks;
using BankApp.Infrastructure.Data;
using Dapper;

namespace BankApp.UI
{
    public static class DbCheck
    {
        private static readonly string LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "novabank_dbcheck.txt");
        
        private static void Log(string msg)
        {
            Console.WriteLine(msg);
            System.Diagnostics.Debug.WriteLine(msg);
            try { File.AppendAllText(LogFile, msg + Environment.NewLine); } catch { }
        }
        
        public static async Task CheckAndLogAsync()
        {
            try
            {
                File.WriteAllText(LogFile, $"=== DB CHECK {DateTime.Now:HH:mm:ss} ===" + Environment.NewLine);
                
                var context = new DapperContext();
                using var conn = context.CreateConnection();
                
                Log("=== DB CHECK START ===");
                
                // Check if tables exist first
                var tableCheck = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Users'");
                Log($"Users table exists: {tableCheck > 0}");
                
                var userCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Users\"");
                var customerCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Customers\"");
                var accountCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Accounts\"");
                var txCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Transactions\"");
                var portfolioCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"CustomerPortfolios\"");
                var loanCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Loans\"");
                
                Log($"Users: {userCount}");
                Log($"Customers: {customerCount}");
                Log($"Accounts: {accountCount}");
                Log($"Transactions: {txCount}");
                Log($"Portfolios: {portfolioCount}");
                Log($"Loans: {loanCount}");
                
                // Check test user
                var testUser = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT \"Id\", \"Username\" FROM \"Users\" WHERE \"Username\" = '1'");
                if (testUser != null)
                {
                    Log($"Test User Found: Id={testUser.Id}, Username={testUser.Username}");
                    
                    // Check customer for this user
                    var customer = await conn.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT \"Id\", \"FirstName\", \"LastName\" FROM \"Customers\" WHERE \"UserId\" = @UserId",
                        new { UserId = (int)testUser.Id });
                    
                    if (customer != null)
                    {
                        Log($"Customer Found: Id={customer.Id}, Name={customer.FirstName} {customer.LastName}");
                        
                        // Check accounts
                        var accounts = await conn.QueryAsync<dynamic>(
                            "SELECT \"Id\", \"Balance\", \"IBAN\" FROM \"Accounts\" WHERE \"CustomerId\" = @CustId",
                            new { CustId = (int)customer.Id });
                        
                        foreach (var acc in accounts)
                        {
                            Log($"Account: Id={acc.Id}, Balance={acc.Balance}, IBAN={acc.IBAN}");
                        }
                    }
                    else
                    {
                        Log("!!! NO CUSTOMER FOUND FOR TEST USER !!!");
                    }
                }
                else
                {
                    Log("!!! TEST USER '1' NOT FOUND !!!");
                }
                
                Log("=== DB CHECK END ===");
            }
            catch (Exception ex)
            {
                Log($"DB CHECK ERROR: {ex.Message}");
                Log(ex.StackTrace ?? "");
            }
        }
    }
}
