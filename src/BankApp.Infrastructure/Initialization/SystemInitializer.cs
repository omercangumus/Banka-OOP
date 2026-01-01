using System;
using System.Linq;
using System.Threading;
using System.Data;
using System.ServiceProcess;
using Npgsql;

namespace BankApp.Infrastructure.Initialization
{
    public class SystemInitializer
    {
        private const string ConnectionStringPostgres = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=postgres;";

        public void StartPostgresService()
        {
            try
            {
                Console.WriteLine("PostgreSQL servisi taranıyor...");
                var services = ServiceController.GetServices();
                var postgresService = services.FirstOrDefault(s => s.ServiceName.ToLower().Contains("postgresql"));

                if (postgresService != null)
                {
                    if (postgresService.Status != ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("Servis başlatılıyor...");
                        postgresService.Start();
                        postgresService.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        Console.WriteLine("PostgreSQL servisi başarıyla başlatıldı.");
                    }
                    else
                    {
                        Console.WriteLine("Servis zaten çalışıyor.");
                    }
                }
                else
                {
                    Console.WriteLine("UYARI: 'postgresql' servisi bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Servis hatası: {ex.Message}");
            }
        }

        public void CreateDatabaseIfNotExists()
        {
            try
            {
                StartPostgresService();
                Console.WriteLine("Veritabanı kontrol ediliyor...");
                using (var conn = new NpgsqlConnection(ConnectionStringPostgres))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = 'NovaBankDb'";
                    var exists = cmd.ExecuteScalar() != null;

                    if (!exists)
                    {
                        Console.WriteLine("NovaBankDb oluşturuluyor...");
                        var createCmd = conn.CreateCommand();
                        createCmd.CommandText = "CREATE DATABASE \"NovaBankDb\"";
                        createCmd.ExecuteNonQuery();
                        Console.WriteLine("Oluşturuldu.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Hatası: {ex.Message}");
                throw;
            }
        }
    }
}
