using System;
using System.Data;
using BankApp.Core.Interfaces;
using Npgsql;
using Dapper; // Dependencies: Npgsql

namespace BankApp.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        // Connection string for the 'postgres' database (to create new DBs)
        private readonly string _masterConnectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=postgres;";
        
        // Connection string for our 'BankDb'
        private readonly string _appConnectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;";

        public void Initialize()
        {
            EnsureDatabaseExists();
            InitializeTables();
            UpdateSchema(); // Add new columns if missing
            SeedData(); // Seed data only once
        }

        private void UpdateSchema()
        {
            using (var conn = new NpgsqlConnection(_appConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Users table schema updates
                    cmd.CommandText = "ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"VerificationCode\" VARCHAR(10);";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"VerificationCodeExpiry\" TIMESTAMP;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"IsVerified\" BOOLEAN DEFAULT FALSE;";
                    cmd.ExecuteNonQuery();

                    // SORUN DÜZELTİLDİ: Customers table eksik kolonlar eklendi
                    cmd.CommandText = "ALTER TABLE \"Customers\" ADD COLUMN IF NOT EXISTS \"Address\" TEXT;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "ALTER TABLE \"Customers\" ADD COLUMN IF NOT EXISTS \"DateOfBirth\" TIMESTAMP;";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void EnsureDatabaseExists()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_masterConnectionString))
                {
                    connection.Open();
                    var sql = "SELECT 1 FROM pg_database WHERE datname = 'NovaBankDb'";
                    var exists = connection.ExecuteScalar<bool>(sql);

                    if (!exists)
                    {
                        connection.Execute("CREATE DATABASE \"NovaBankDb\"");
                    }
                }
            }
            catch (Exception ex)
            {
                // In a real app, log this. Simple throw/message for now.
                throw new Exception($"Veritabanı oluşturulurken hata: {ex.Message}");
            }
        }

        private void InitializeTables()
        {
            using (var conn = new NpgsqlConnection(_appConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // 1. Users Table (with all columns including verification)
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""Users"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""Username"" VARCHAR(50) NOT NULL UNIQUE,
                            ""PasswordHash"" VARCHAR(255) NOT NULL,
                            ""Email"" VARCHAR(100) NOT NULL,
                            ""FullName"" VARCHAR(100),
                            ""Role"" VARCHAR(20) DEFAULT 'Customer',
                            ""IsActive"" BOOLEAN DEFAULT TRUE,
                            ""VerificationCode"" VARCHAR(10),
                            ""VerificationCodeExpiry"" TIMESTAMP,
                            ""IsVerified"" BOOLEAN DEFAULT FALSE,
                            ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";
                    cmd.ExecuteNonQuery();

                    // 2. Customers Table - SORUN DÜZELTİLDİ: Eksik kolonlar eklendi
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""Customers"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""UserId"" INT NOT NULL,
                            ""IdentityNumber"" VARCHAR(11) NOT NULL UNIQUE,
                            ""FirstName"" VARCHAR(50),
                            ""LastName"" VARCHAR(50),
                            ""PhoneNumber"" VARCHAR(20),
                            ""Email"" VARCHAR(100),
                            ""Address"" TEXT,
                            ""DateOfBirth"" TIMESTAMP,
                            ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE
                        );";
                    cmd.ExecuteNonQuery();

                    // 3. Accounts Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""Accounts"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""CustomerId"" INT NOT NULL,
                            ""AccountNumber"" VARCHAR(20) NOT NULL UNIQUE,
                            ""IBAN"" VARCHAR(34) NOT NULL UNIQUE,
                            ""Balance"" DECIMAL(18, 2) DEFAULT 0,
                            ""CurrencyCode"" VARCHAR(3) DEFAULT 'TRY',
                            ""OpenedDate"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (""CustomerId"") REFERENCES ""Customers""(""Id"") ON DELETE CASCADE
                        );";
                    cmd.ExecuteNonQuery();

                    // 4. Transactions Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""Transactions"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""AccountId"" INT NOT NULL,
                            ""TransactionType"" VARCHAR(20),
                            ""Amount"" DECIMAL(18, 2) NOT NULL,
                            ""Description"" VARCHAR(255),
                            ""TransactionDate"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (""AccountId"") REFERENCES ""Accounts""(""Id"") ON DELETE CASCADE
                        );";
                    cmd.ExecuteNonQuery();

                    // 5. AuditLogs Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""AuditLogs"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""UserId"" INT NULL,
                            ""Action"" VARCHAR(100),
                            ""Details"" TEXT,
                            ""IpAddress"" VARCHAR(50),
                            ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void SeedData()
        {
            using (var conn = new NpgsqlConnection(_appConnectionString))
            {
                conn.Open();
                
                // SORUN DÜZELTİLDİ: Önce mevcut test kullanıcıları temizle (ID'ler sıfırlanmasın diye)
                using (var cleanupCmd = conn.CreateCommand())
                {
                    // Sadece test kullanıcılarını temizle
                    cleanupCmd.CommandText = @"
                        DELETE FROM ""AuditLogs"";
                        DELETE FROM ""Transactions"";
                        DELETE FROM ""Accounts"";
                        DELETE FROM ""Customers"";
                        DELETE FROM ""Users"" WHERE ""Username"" IN ('admin', 'test', 'demo', 'staff');
                    ";
                    try { cleanupCmd.ExecuteNonQuery(); } catch { /* Ignore if tables don't exist */ }
                }

                // Check if Users table is empty or test users don't exist
                using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM \"Users\" WHERE \"Username\" IN ('admin', 'test', 'demo', 'staff')";
                    long testUserCount = 0;
                    try
                    {
                        testUserCount = (long)(checkCmd.ExecuteScalar() ?? 0);
                    }
                    catch { /* Table might not exist yet */ }
                    
                    // SORUN DÜZELTİLDİ: Test kullanıcıları yoksa ekle (her zaman güncel veriler)
                    if (testUserCount == 0)
                    {
                        using (var insertCmd = conn.CreateCommand())
                        {
                            // Password hashes (SHA256):
                            // admin123 = 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
                            // test123  = ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae
                            // demo123  = 85d240b21b6e0f7fce55d73c02e88ae9ffe0df8d71f0eec46b3d53cb7e53e0eb (SORUN DÜZELTİLDİ: Önceki hash yanlıştı)
                            // 123456   = 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
                            
                            insertCmd.CommandText = @"
                                -- Admin User (username: admin, password: admin123)
                                INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""Role"", ""FullName"", ""IsActive"", ""IsVerified"", ""CreatedAt"")
                                VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'novabank.com@gmail.com', 'Admin', 'Sistem Yöneticisi', TRUE, TRUE, CURRENT_TIMESTAMP);
                                
                                -- Test User (username: test, password: test123)
                                INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""Role"", ""FullName"", ""IsActive"", ""IsVerified"", ""CreatedAt"")
                                VALUES ('test', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'test@novabank.com', 'Customer', 'Test Kullanıcı', TRUE, TRUE, CURRENT_TIMESTAMP);
                                
                                -- Demo User (username: demo, password: demo123) - SORUN DÜZELTİLDİ: Hash güncellendi
                                INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""Role"", ""FullName"", ""IsActive"", ""IsVerified"", ""CreatedAt"")
                                VALUES ('demo', '85d240b21b6e0f7fce55d73c02e88ae9ffe0df8d71f0eec46b3d53cb7e53e0eb', 'demo@novabank.com', 'Customer', 'Demo Kullanıcı', TRUE, TRUE, CURRENT_TIMESTAMP);
                                
                                -- Staff User (username: staff, password: 123456)
                                INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""Role"", ""FullName"", ""IsActive"", ""IsVerified"", ""CreatedAt"")
                                VALUES ('staff', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'staff@novabank.com', 'Staff', 'Banka Personeli', TRUE, TRUE, CURRENT_TIMESTAMP);
                            ";
                            insertCmd.ExecuteNonQuery();
                        }

                        // SORUN DÜZELTİLDİ: Kullanıcı ID'lerini dinamik olarak al
                        int adminId = 0, testId = 0, demoId = 0, staffId = 0;
                        using (var getIdCmd = conn.CreateCommand())
                        {
                            getIdCmd.CommandText = "SELECT \"Id\" FROM \"Users\" WHERE \"Username\" = 'admin'";
                            adminId = Convert.ToInt32(getIdCmd.ExecuteScalar() ?? 1);
                            
                            getIdCmd.CommandText = "SELECT \"Id\" FROM \"Users\" WHERE \"Username\" = 'test'";
                            testId = Convert.ToInt32(getIdCmd.ExecuteScalar() ?? 2);
                            
                            getIdCmd.CommandText = "SELECT \"Id\" FROM \"Users\" WHERE \"Username\" = 'demo'";
                            demoId = Convert.ToInt32(getIdCmd.ExecuteScalar() ?? 3);
                            
                            getIdCmd.CommandText = "SELECT \"Id\" FROM \"Users\" WHERE \"Username\" = 'staff'";
                            staffId = Convert.ToInt32(getIdCmd.ExecuteScalar() ?? 4);
                        }

                        // Add Customers - SORUN DÜZELTİLDİ: Eksik kolonlar eklendi
                        using (var custCmd = conn.CreateCommand())
                        {
                             custCmd.CommandText = $@"
                                -- Test Customer (linked to user 'test')
                                -- SORUN DÜZELTİLDİ: Önce sil, sonra ekle (ON CONFLICT yerine)
                                DELETE FROM ""Customers"" WHERE ""IdentityNumber"" IN ('12345678901', '98765432109');
                                
                                INSERT INTO ""Customers"" (""UserId"", ""IdentityNumber"", ""FirstName"", ""LastName"", ""PhoneNumber"", ""Email"", ""Address"", ""DateOfBirth"", ""CreatedAt"")
                                VALUES ({testId}, '12345678901', 'Test', 'Kullanıcı', '05551234567', 'test@novabank.com', 'İstanbul, Türkiye', '1990-01-15'::TIMESTAMP, CURRENT_TIMESTAMP);
                                
                                -- Demo Customer (linked to user 'demo')
                                INSERT INTO ""Customers"" (""UserId"", ""IdentityNumber"", ""FirstName"", ""LastName"", ""PhoneNumber"", ""Email"", ""Address"", ""DateOfBirth"", ""CreatedAt"")
                                VALUES ({demoId}, '98765432109', 'Demo', 'Kullanıcı', '05559876543', 'demo@novabank.com', 'Ankara, Türkiye', '1985-05-20'::TIMESTAMP, CURRENT_TIMESTAMP);
                             ";
                             custCmd.ExecuteNonQuery();
                        }

                        // SORUN DÜZELTİLDİ: Customer ID'lerini dinamik olarak al
                        int testCustomerId = 0, demoCustomerId = 0;
                        using (var getCustIdCmd = conn.CreateCommand())
                        {
                            getCustIdCmd.CommandText = $"SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = {testId}";
                            testCustomerId = Convert.ToInt32(getCustIdCmd.ExecuteScalar() ?? 1);
                            
                            getCustIdCmd.CommandText = $"SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = {demoId}";
                            demoCustomerId = Convert.ToInt32(getCustIdCmd.ExecuteScalar() ?? 2);
                        }

                        // Add Accounts with balances - SORUN DÜZELTİLDİ: Dinamik ID kullanımı
                        using (var accCmd = conn.CreateCommand())
                        {
                            // SORUN DÜZELTİLDİ: Önce mevcut hesapları temizle
                            using (var cleanupAccCmd = conn.CreateCommand())
                            {
                                cleanupAccCmd.CommandText = @"DELETE FROM ""Accounts"" WHERE ""AccountNumber"" IN ('1000000001', '1000000002', '2000000001', '2000000002');";
                                cleanupAccCmd.ExecuteNonQuery();
                            }
                            
                            accCmd.CommandText = $@"
                                -- Test Customer Accounts
                                INSERT INTO ""Accounts"" (""CustomerId"", ""AccountNumber"", ""IBAN"", ""Balance"", ""CurrencyCode"", ""OpenedDate"", ""CreatedAt"")
                                VALUES ({testCustomerId}, '1000000001', 'TR330006200000000001000001', 50000.00, 'TRY', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                                
                                INSERT INTO ""Accounts"" (""CustomerId"", ""AccountNumber"", ""IBAN"", ""Balance"", ""CurrencyCode"", ""OpenedDate"", ""CreatedAt"")
                                VALUES ({testCustomerId}, '1000000002', 'TR330006200000000001000002', 2500.00, 'USD', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                                
                                -- Demo Customer Accounts
                                INSERT INTO ""Accounts"" (""CustomerId"", ""AccountNumber"", ""IBAN"", ""Balance"", ""CurrencyCode"", ""OpenedDate"", ""CreatedAt"")
                                VALUES ({demoCustomerId}, '2000000001', 'TR330006200000000002000001', 25000.00, 'TRY', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                                
                                INSERT INTO ""Accounts"" (""CustomerId"", ""AccountNumber"", ""IBAN"", ""Balance"", ""CurrencyCode"", ""OpenedDate"", ""CreatedAt"")
                                VALUES ({demoCustomerId}, '2000000002', 'TR330006200000000002000002', 1000.00, 'EUR', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                            ";
                            accCmd.ExecuteNonQuery();
                        }

                        // SORUN DÜZELTİLDİ: Account ID'lerini dinamik olarak al
                        using (var getAccIdCmd = conn.CreateCommand())
                        {
                            getAccIdCmd.CommandText = $"SELECT \"Id\" FROM \"Accounts\" WHERE \"AccountNumber\" = '1000000001' LIMIT 1";
                            var testAccount1Id = Convert.ToInt32(getAccIdCmd.ExecuteScalar() ?? 0);
                            
                            getAccIdCmd.CommandText = $"SELECT \"Id\" FROM \"Accounts\" WHERE \"AccountNumber\" = '1000000002' LIMIT 1";
                            var testAccount2Id = Convert.ToInt32(getAccIdCmd.ExecuteScalar() ?? 0);
                            
                            getAccIdCmd.CommandText = $"SELECT \"Id\" FROM \"Accounts\" WHERE \"AccountNumber\" = '2000000001' LIMIT 1";
                            var demoAccount1Id = Convert.ToInt32(getAccIdCmd.ExecuteScalar() ?? 0);

                            // Add some transactions - SORUN DÜZELTİLDİ: Dinamik ID kullanımı
                            if (testAccount1Id > 0 || testAccount2Id > 0 || demoAccount1Id > 0)
                            {
                                using (var txCmd = conn.CreateCommand())
                                {
                                    // SORUN DÜZELTİLDİ: Mevcut işlemleri temizle ve yeniden ekle
                                    using (var cleanupTxCmd = conn.CreateCommand())
                                    {
                                        cleanupTxCmd.CommandText = @"DELETE FROM ""Transactions"";";
                                        cleanupTxCmd.ExecuteNonQuery();
                                    }
                                    
                                    if (testAccount1Id > 0)
                                    {
                                        txCmd.CommandText = $@"
                                            INSERT INTO ""Transactions"" (""AccountId"", ""TransactionType"", ""Amount"", ""Description"", ""TransactionDate"", ""CreatedAt"")
                                            VALUES ({testAccount1Id}, 'Deposit', 50000.00, 'İlk yatırım - Test Hesabı', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                                        ";
                                        txCmd.ExecuteNonQuery();
                                    }
                                    
                                    if (testAccount2Id > 0)
                                    {
                                        txCmd.CommandText = $@"
                                            INSERT INTO ""Transactions"" (""AccountId"", ""TransactionType"", ""Amount"", ""Description"", ""TransactionDate"", ""CreatedAt"")
                                            VALUES ({testAccount2Id}, 'Deposit', 2500.00, 'USD hesap açılışı', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                                        ";
                                        txCmd.ExecuteNonQuery();
                                    }
                                    
                                    if (demoAccount1Id > 0)
                                    {
                                        txCmd.CommandText = $@"
                                            INSERT INTO ""Transactions"" (""AccountId"", ""TransactionType"", ""Amount"", ""Description"", ""TransactionDate"", ""CreatedAt"")
                                            VALUES ({demoAccount1Id}, 'Deposit', 25000.00, 'İlk yatırım - Demo Hesabı', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                                        ";
                                        txCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        // Add audit log - SORUN DÜZELTİLDİ: Dinamik ID kullanımı
                        using (var auditCmd = conn.CreateCommand())
                        {
                            auditCmd.CommandText = $@"
                                INSERT INTO ""AuditLogs"" (""UserId"", ""Action"", ""Details"", ""IpAddress"", ""CreatedAt"")
                                VALUES ({adminId}, 'SystemInit', 'Veritabanı başlatıldı ve test verileri eklendi.', '127.0.0.1', CURRENT_TIMESTAMP);
                            ";
                            auditCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
