using Npgsql;
using System.Data;
using System.IO;
using System.Text.Json;

namespace BankApp.Infrastructure.Data
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext()
        {
            // SORUN DÜZELTİLDİ: appsettings.json'dan connection string okuma eklendi
            _connectionString = LoadConnectionString();
        }

        private string LoadConnectionString()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var jsonString = File.ReadAllText(configPath);
                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        using var doc = JsonDocument.Parse(jsonString);
                        if (doc.RootElement.TryGetProperty("ConnectionStrings", out var connStrings))
                        {
                            if (connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                            {
                                var connectionString = defaultConn.GetString();
                                if (!string.IsNullOrEmpty(connectionString))
                                {
                                    return connectionString;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log hatası olabilir ama şimdilik sessizce fallback'e geç
                System.Diagnostics.Debug.WriteLine($"Connection string okuma hatası: {ex.Message}");
            }

            // Fallback connection string - SORUN DÜZELTİLDİ: Null kontrolü eklendi
            return "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;";
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
