using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BankApp.Infrastructure.Data;
using Dapper;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// DEBUG ortamında test verisi oluşturur.
    /// Release derlemesinde bu servis hiçbir şey yapmaz.
    /// </summary>
    public static class DebugSeedService
    {
        private static bool _seeded = false;
        
        /// <summary>
        /// Database'i temizler ve yeniden seed eder (FORCE RESET)
        /// </summary>
        public static async Task ForceResetAndSeedAsync()
        {
#if DEBUG
            try
            {
                var context = new DapperContext();
                using var conn = context.CreateConnection();
                
                Debug.WriteLine("[DEBUG SEED] !!! FORCE RESET STARTING !!!");
                
                // Eksik tabloları oluştur
                await EnsureTablesExistAsync(conn);
                
                // Delete in correct order (foreign key constraints) - ignore errors if tables don't exist
                try { await conn.ExecuteAsync("DELETE FROM \"CustomerPortfolios\""); } catch { }
                try { await conn.ExecuteAsync("DELETE FROM \"Transactions\""); } catch { }
                try { await conn.ExecuteAsync("DELETE FROM \"Loans\""); } catch { }
                try { await conn.ExecuteAsync("DELETE FROM \"Accounts\""); } catch { }
                try { await conn.ExecuteAsync("DELETE FROM \"Customers\""); } catch { }
                try { await conn.ExecuteAsync("DELETE FROM \"Users\" WHERE \"Username\" = '1'"); } catch { }
                
                Debug.WriteLine("[DEBUG SEED] All test data deleted!");
                
                // Reset flag and reseed
                _seeded = false;
                await SeedTestDataAsync();
                
                Debug.WriteLine("[DEBUG SEED] !!! FORCE RESET COMPLETE !!!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG SEED] Force reset error: {ex.Message}");
            }
#endif
        }
        
        private static async Task EnsureTablesExistAsync(System.Data.IDbConnection conn)
        {
            // Loans tablosu
            await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ""Loans"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""CustomerId"" INTEGER NOT NULL,
                    ""UserId"" INTEGER NOT NULL,
                    ""Amount"" DECIMAL(18,2) NOT NULL,
                    ""TermMonths"" INTEGER NOT NULL,
                    ""InterestRate"" DECIMAL(5,2) NOT NULL,
                    ""Status"" VARCHAR(50) NOT NULL DEFAULT 'Pending',
                    ""ApplicationDate"" TIMESTAMP NOT NULL DEFAULT NOW(),
                    ""DecisionDate"" TIMESTAMP,
                    ""ApprovedById"" INTEGER,
                    ""RejectionReason"" TEXT,
                    ""Notes"" TEXT,
                    ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
                )");
            Debug.WriteLine("[DEBUG SEED] Loans table ensured");
            
            // CustomerPortfolios tablosu
            await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ""CustomerPortfolios"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""CustomerId"" INTEGER NOT NULL,
                    ""StockSymbol"" VARCHAR(20) NOT NULL,
                    ""Quantity"" DECIMAL(18,8) NOT NULL,
                    ""AverageCost"" DECIMAL(18,2) NOT NULL,
                    ""PurchaseDate"" TIMESTAMP NOT NULL DEFAULT NOW(),
                    ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
                )");
            Debug.WriteLine("[DEBUG SEED] CustomerPortfolios table ensured");
            
            // Stocks tablosu
            await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ""Stocks"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Symbol"" VARCHAR(20) NOT NULL UNIQUE,
                    ""Name"" VARCHAR(200),
                    ""CurrentPrice"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                    ""PreviousPrice"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                    ""ChangePercent"" DECIMAL(8,4) NOT NULL DEFAULT 0,
                    ""Volatility"" DECIMAL(8,4),
                    ""MarketCap"" DECIMAL(18,2),
                    ""Sector"" VARCHAR(100),
                    ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
                )");
            Debug.WriteLine("[DEBUG SEED] Stocks table ensured");
        }
        
        /// <summary>
        /// DEBUG modunda test verilerini ekler. Idempotent - zaten varsa tekrar eklemez.
        /// </summary>
        public static async Task SeedTestDataAsync()
        {
#if DEBUG
            if (_seeded) return;
            _seeded = true;
            
            try
            {
                var context = new DapperContext();
                using var conn = context.CreateConnection();
                
                Debug.WriteLine("[DEBUG SEED] === DATABASE STATUS CHECK ===");
                
                // Log current counts
                var userCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Users\"");
                var customerCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Customers\"");
                var accountCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Accounts\"");
                var txCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Transactions\"");
                var portfolioCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"CustomerPortfolios\"");
                
                Debug.WriteLine($"[DEBUG SEED] Users={userCount}, Customers={customerCount}, Accounts={accountCount}, Transactions={txCount}, Portfolios={portfolioCount}");
                
                // Check if test user already exists
                var existingUserId = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT \"Id\" FROM \"Users\" WHERE \"Username\" = '1' LIMIT 1");
                
                if (existingUserId.HasValue)
                {
                    Debug.WriteLine($"[DEBUG SEED] Test kullanıcı zaten mevcut: UserId={existingUserId}");
                    
                    // Check if customer exists for this user
                    var existingCustomerId = await conn.QueryFirstOrDefaultAsync<int?>(
                        "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                        new { UserId = existingUserId.Value });
                    
                    if (existingCustomerId.HasValue)
                    {
                        // Check if accounts exist
                        var existingAccountCount = await conn.QueryFirstOrDefaultAsync<int>(
                            "SELECT COUNT(*) FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId",
                            new { CustomerId = existingCustomerId.Value });
                        
                        Debug.WriteLine($"[DEBUG SEED] CustomerId={existingCustomerId}, AccountCount={existingAccountCount}");
                        
                        if (existingAccountCount > 0)
                        {
                            Debug.WriteLine("[DEBUG SEED] Veriler zaten mevcut, seed atlanıyor.");
                            return;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("[DEBUG SEED] ⚠️ User var ama Customer yok! Customer ekleniyor...");
                    }
                }
                
                Debug.WriteLine("[DEBUG SEED] Test verileri ekleniyor...");
                
                int userId;
                
                // 1. Test User (username=1, password=1) - only create if not exists
                if (existingUserId.HasValue)
                {
                    userId = existingUserId.Value;
                    // Şifre hash'ini güncelle (format uyumsuzluğu olabilir)
                    string passwordHash = HashPassword("1");
                    await conn.ExecuteAsync(
                        "UPDATE \"Users\" SET \"PasswordHash\" = @Hash WHERE \"Id\" = @Id",
                        new { Hash = passwordHash, Id = userId });
                    Debug.WriteLine($"[DEBUG SEED] Mevcut User kullanılıyor: Id={userId}, password hash güncellendi");
                }
                else
                {
                    string passwordHash = HashPassword("1");
                    userId = await conn.QuerySingleAsync<int>(@"
                        INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""Role"", ""FullName"", ""IsActive"", ""IsVerified"", ""CreatedAt"")
                        VALUES ('1', @PasswordHash, 'test@novabank.com', 'Customer', 'Test Kullanıcı', true, true, NOW())
                        RETURNING ""Id""",
                        new { PasswordHash = passwordHash });
                    Debug.WriteLine($"[DEBUG SEED] User eklendi: Id={userId}");
                }
                
                // 2. Customer
                var customerId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO ""Customers"" (""UserId"", ""IdentityNumber"", ""FirstName"", ""LastName"", ""PhoneNumber"", ""Email"", ""Address"", ""DateOfBirth"", ""CreatedAt"")
                    VALUES (@UserId, '12345678901', 'Test', 'Kullanıcı', '5551234567', 'test@novabank.com', 'İstanbul, Türkiye', '1990-01-15', NOW())
                    RETURNING ""Id""",
                    new { UserId = userId });
                
                Debug.WriteLine($"[DEBUG SEED] Customer eklendi: Id={customerId}");
                
                // 3. Accounts (2 hesap)
                var accountId1 = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO ""Accounts"" (""CustomerId"", ""AccountNumber"", ""IBAN"", ""Balance"", ""CurrencyCode"", ""OpenedDate"", ""CreatedAt"")
                    VALUES (@CustomerId, '1001234567', 'TR330006100519786457841326', 15750.50, 'TRY', NOW() - INTERVAL '6 months', NOW())
                    RETURNING ""Id""",
                    new { CustomerId = customerId });
                
                var accountId2 = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO ""Accounts"" (""CustomerId"", ""AccountNumber"", ""IBAN"", ""Balance"", ""CurrencyCode"", ""OpenedDate"", ""CreatedAt"")
                    VALUES (@CustomerId, '1001234568', 'TR330006100519786457841327', 8500.00, 'TRY', NOW() - INTERVAL '3 months', NOW())
                    RETURNING ""Id""",
                    new { CustomerId = customerId });
                
                Debug.WriteLine($"[DEBUG SEED] Accounts eklendi: Id1={accountId1}, Id2={accountId2}");
                
                // 4. Transactions (son 30 günde çeşitli işlemler)
                var transactions = new[]
                {
                    (accountId1, "Deposit", 5000m, "Maaş Yatırma", -1),
                    (accountId1, "Withdraw", 450m, "Market Alışverişi", -2),
                    (accountId1, "Withdraw", 850m, "Elektrik Faturası", -5),
                    (accountId1, "TransferOut", 1200m, "Kira Ödemesi", -7),
                    (accountId1, "Withdraw", 320m, "Restoran", -10),
                    (accountId1, "Deposit", 2500m, "Ek Gelir", -12),
                    (accountId1, "Withdraw", 180m, "Ulaşım", -15),
                    (accountId2, "Deposit", 3000m, "Birikim Transferi", -3),
                    (accountId2, "Withdraw", 500m, "Online Alışveriş", -8),
                    (accountId1, "TransferOut", 750m, "Arkadaşa Transfer", -20)
                };
                
                foreach (var (accId, type, amount, desc, daysAgo) in transactions)
                {
                    var txDate = DateTime.Now.AddDays(daysAgo);
                    await conn.ExecuteAsync(@"
                        INSERT INTO ""Transactions"" (""AccountId"", ""TransactionType"", ""Amount"", ""Description"", ""TransactionDate"", ""CreatedAt"")
                        VALUES (@AccountId, @Type, @Amount, @Description, @TxDate, NOW())",
                        new { AccountId = accId, Type = type, Amount = amount, Description = desc, TxDate = txDate });
                }
                
                Debug.WriteLine($"[DEBUG SEED] {transactions.Length} Transaction eklendi");
                
                // 5. Loans (1 Approved, 1 Pending)
                await conn.ExecuteAsync(@"
                    INSERT INTO ""Loans"" (""CustomerId"", ""UserId"", ""Amount"", ""TermMonths"", ""InterestRate"", ""Status"", ""ApplicationDate"", ""DecisionDate"", ""CreatedAt"")
                    VALUES (@CustomerId, @UserId, 25000, 12, 2.5, 'Approved', NOW() - INTERVAL '2 months', NOW() - INTERVAL '1 month', NOW())",
                    new { CustomerId = customerId, UserId = userId });
                
                await conn.ExecuteAsync(@"
                    INSERT INTO ""Loans"" (""CustomerId"", ""UserId"", ""Amount"", ""TermMonths"", ""InterestRate"", ""Status"", ""ApplicationDate"", ""Notes"", ""CreatedAt"")
                    VALUES (@CustomerId, @UserId, 50000, 24, 3.0, 'Pending', NOW() - INTERVAL '3 days', 'Ev tadilat kredisi', NOW())",
                    new { CustomerId = customerId, UserId = userId });
                
                Debug.WriteLine("[DEBUG SEED] Loans eklendi (1 Approved, 1 Pending)");
                
                // 6. CustomerPortfolio (yatırım varlıkları)
                try
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO ""CustomerPortfolios"" (""CustomerId"", ""StockSymbol"", ""Quantity"", ""AverageCost"", ""PurchaseDate"", ""CreatedAt"")
                        VALUES 
                        (@CustomerId, 'THYAO', 50, 245.50, NOW() - INTERVAL '2 months', NOW()),
                        (@CustomerId, 'GARAN', 100, 78.25, NOW() - INTERVAL '1 month', NOW()),
                        (@CustomerId, 'BTC', 0.15, 42000, NOW() - INTERVAL '3 weeks', NOW())",
                        new { CustomerId = customerId });
                    
                    Debug.WriteLine("[DEBUG SEED] CustomerPortfolios eklendi");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG SEED] CustomerPortfolios tablosu yok veya hata: {ex.Message}");
                }
                
                // 7. Stocks tablosu varsa fiyat güncelle
                try
                {
                    await conn.ExecuteAsync(@"
                        UPDATE ""Stocks"" SET ""CurrentPrice"" = 265.75, ""ChangePercent"" = 8.25 WHERE ""Symbol"" = 'THYAO';
                        UPDATE ""Stocks"" SET ""CurrentPrice"" = 82.50, ""ChangePercent"" = 5.43 WHERE ""Symbol"" = 'GARAN';");
                    Debug.WriteLine("[DEBUG SEED] Stocks fiyatları güncellendi");
                }
                catch { }
                
                Debug.WriteLine("[DEBUG SEED] ✅ Tüm test verileri başarıyla eklendi!");
                Debug.WriteLine("[DEBUG SEED] Login: username=1, password=1");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG SEED] ❌ Hata: {ex.Message}");
            }
#endif
        }
        
        private static string HashPassword(string password)
        {
            // AuthService ile aynı format: hex string (x2)
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
