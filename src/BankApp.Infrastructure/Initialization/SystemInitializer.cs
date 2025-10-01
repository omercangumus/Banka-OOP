using System;
using System.Linq;
using System.Threading;
using System.Data;
using Npgsql;

namespace BankApp.Infrastructure.Initialization
{
    public class SystemInitializer
    {
        private const string ConnectionStringPostgres = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=postgres;";

        public void StartPostgresService()
        {
            // ServiceController logic removed to avoid platform dependencies in build
            Console.WriteLine("PostgreSQL servisi kontrolü devre dışı bırakıldı (Build Only).");
        }

        public void CreateDatabaseIfNotExists()
        {
            try
            {
                Console.WriteLine("Veritabanı kontrol ediliyor...");
                using (var conn = new NpgsqlConnection(ConnectionStringPostgres))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = 'NovaBankDb'";
                    var exists = cmd.ExecuteScalar() != null;

                    if (!exists)
                    {
                        Console.WriteLine("NovaBankDb bulunamadı. Oluşturuluyor...");
                        var createCmd = conn.CreateCommand();
                        createCmd.CommandText = "CREATE DATABASE \"NovaBankDb\"";
                        createCmd.ExecuteNonQuery();
                        Console.WriteLine("NovaBankDb başarıyla oluşturuldu.");
                    }
                    else
                    {
                        Console.WriteLine("NovaBankDb zaten mevcut.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veritabanı oluşturma hatası: {ex.Message}");
                // Rethrow to stop app if DB is critical
                throw;
            }
        }
    }
}
