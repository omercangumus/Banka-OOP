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
        // Standart yerel bağlantı
        private const string ConnectionStringPostgres = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=postgres;";

        public void StartPostgresService()
        {
            try
            {
                // Servis kontrolü
                var services = ServiceController.GetServices();
                var postgresService = services.FirstOrDefault(s => s.ServiceName.ToLower().Contains("postgresql"));

                if (postgresService != null && postgresService.Status != ServiceControllerStatus.Running)
                {
                    postgresService.Start();
                    postgresService.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                }
            }
            catch
            {
                // Yönetici izni yoksa veya hata varsa uygulamayı durdurma, devam et
                Console.WriteLine("Servis başlatılamadı (Manuel kontrol gerekebilir).");
            }
        }

        public void CreateDatabaseIfNotExists()
        {
            try
            {
                StartPostgresService();
                
                using (var conn = new NpgsqlConnection(ConnectionStringPostgres))
                {
                    conn.Open();
                    // Veritabanı var mı kontrol et
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = 'NovaBankDb'";
                    var exists = cmd.ExecuteScalar() != null;

                    if (!exists)
                    {
                        // Yoksa oluştur
                        var createCmd = conn.CreateCommand();
                        createCmd.CommandText = "CREATE DATABASE \"NovaBankDb\"";
                        createCmd.ExecuteNonQuery();
                    }
                    // Varsa HİÇBİR ŞEY YAPMA (Mevcut veriyi koru)
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Veritabanı başlatma hatası: {ex.Message}");
            }
        }
    }
}
