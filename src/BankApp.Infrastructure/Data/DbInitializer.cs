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
        private readonly string _masterConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1";
        
        // Connection string for our 'BankDb'
        private readonly string _appConnectionString = "Host=localhost;Port=5432;Database=NovaBankDb;Username=postgres;Password=1";

        public void Initialize()
        {
            EnsureDatabaseExists();
            InitializeTables();
            SeedData();
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
                    // 1. Users Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""Users"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""Username"" VARCHAR(50) NOT NULL UNIQUE,
                            ""PasswordHash"" VARCHAR(255) NOT NULL,
                            ""Email"" VARCHAR(100) NOT NULL,
                            ""FullName"" VARCHAR(100),
                            ""Role"" VARCHAR(20) DEFAULT 'Customer',
                            ""IsActive"" BOOLEAN DEFAULT TRUE,
                            ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";
                    cmd.ExecuteNonQuery();

                    // 2. Customers Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""Customers"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""UserId"" INT NOT NULL,
                            ""IdentityNumber"" VARCHAR(11) NOT NULL UNIQUE,
                            ""FirstName"" VARCHAR(50),
                            ""LastName"" VARCHAR(50),
                            ""PhoneNumber"" VARCHAR(20),
                            ""Email"" VARCHAR(100),
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
                
                // Check if Users table is empty
                using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM \"Users\"";
                    long count = (long)checkCmd.ExecuteScalar();
                    
                    if (count == 0)
                    {
                        using (var insertCmd = conn.CreateCommand())
                        {
                            // Admin User
                            // Hash for 'admin123' (Example hash) - Real Service will use proper hashing
                            insertCmd.CommandText = @"
                                INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""Role"", ""FullName"")
                                VALUES ('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'admin@bank.com', 'Admin', 'System Administrator');
                                
                                INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""Role"", ""FullName"")
                                VALUES ('customer1', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'test@bank.com', 'Customer', 'Test Customer');
                            ";
                            insertCmd.ExecuteNonQuery();
                        }

                        // Get User IDs to link Customer
                        // Simplified for this example, assuming Serial IDs 1 and 2
                        using (var custCmd = conn.CreateCommand())
                        {
                             custCmd.CommandText = @"
                                INSERT INTO ""Customers"" (""UserId"", ""IdentityNumber"", ""FirstName"", ""LastName"", ""Email"")
                                VALUES (2, '12345678901', 'Test', 'Customer', 'test@bank.com');
                             ";
                             custCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
