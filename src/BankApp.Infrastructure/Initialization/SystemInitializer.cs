using System;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using Npgsql;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Initialization
{
    /// <summary>
    /// Sistem başlatıcı - PostgreSQL servisi ve veritabanı kontrolü
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class SystemInitializer
    {
        private const string ConnectionStringPostgres = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=postgres;";
        private readonly DbInitializer _dbInitializer;

        public SystemInitializer()
        {
            _dbInitializer = new DbInitializer();
        }

        /// <summary>
        /// PostgreSQL servisini başlatır
        /// </summary>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public string StartPostgresService()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("PostgreSQL servisi kontrol ediliyor...");
                Console.WriteLine(sb.ToString());

                ServiceController postgresService = null;
                
                try
                {
                    var allServices = ServiceController.GetServices();
                    postgresService = allServices.FirstOrDefault(s => 
                        s.ServiceName.ToLower().StartsWith("postgresql"));
                }
                catch (Exception ex)
                {
                    var sbError = new StringBuilder();
                    sbError.Append("Servis listesi alınamadı: ");
                    sbError.Append(ex.Message);
                    return sbError.ToString();
                }

                if (postgresService == null)
                {
                    return "PostgreSQL servisi bulunamadı. Lütfen PostgreSQL'in yüklü olduğundan emin olun.";
                }

                var sbInfo = new StringBuilder();
                sbInfo.Append("PostgreSQL servisi bulundu: ");
                sbInfo.Append(postgresService.ServiceName);
                Console.WriteLine(sbInfo.ToString());

                if (postgresService.Status != ServiceControllerStatus.Running)
                {
                    var sbStart = new StringBuilder();
                    sbStart.Append("Servis durumu: ");
                    sbStart.Append(postgresService.Status.ToString());
                    sbStart.Append(". Başlatılıyor...");
                    Console.WriteLine(sbStart.ToString());

                    try
                    {
                        postgresService.Start();
                        postgresService.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        Console.WriteLine("PostgreSQL servisi başarıyla başlatıldı.");
                    }
                    catch (InvalidOperationException)
                    {
                        return "PostgreSQL servisi başlatılamadı. Uygulamayı Yönetici olarak çalıştırın.";
                    }
                    catch (System.ServiceProcess.TimeoutException)
                    {
                        return "PostgreSQL servisi başlatma zaman aşımına uğradı.";
                    }
                }
                else
                {
                    Console.WriteLine("PostgreSQL servisi zaten çalışıyor.");
                }

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("PostgreSQL servis kontrolü hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Veritabanını oluşturur (yoksa)
        /// </summary>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public string CreateDatabaseIfNotExists()
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

                        // Veritabanı yeni oluşturulduysa SeedData çalıştır
                        Console.WriteLine("Veritabanı ilklendiriliyor (Tablolar ve Örnek Veriler)...");
                        _dbInitializer.Initialize();
                    }
                    else
                    {
                        Console.WriteLine("NovaBankDb zaten mevcut.");
                        // Mevcut olsa bile tabloları ve eksik verileri kontrol et
                         _dbInitializer.Initialize();
                    }
                }

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Veritabanı oluşturma hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Sistemi tam olarak başlatır (Servis + DB + Tablolar)
        /// </summary>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public string InitializeSystem()
        {
            // 1. PostgreSQL servisini başlat
            string serviceResult = StartPostgresService();
            if (serviceResult != null)
            {
                // Servis hatası kritik olmayabilir (container vs), loglayıp devam edilebilir 
                // ancak Fırat standartlarında servis kontrolü istenmiş.
                return serviceResult;
            }

            // 2. Veritabanını oluştur ve ilklendir (Seed dahil)
            string dbResult = CreateDatabaseIfNotExists();
            if (dbResult != null)
            {
                return dbResult;
            }

            Console.WriteLine("Sistem başarıyla başlatıldı.");
            return null; // Tüm işlemler başarılı
        }
    }
}
