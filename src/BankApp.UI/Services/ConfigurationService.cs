using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using System.Net.Mail;
using System.Net;

namespace BankApp.UI.Services
{
    /// <summary>
    /// Configuration and connection testing service
    /// PHASE 2: Centralized configuration loading and testing
    /// </summary>
    public static class ConfigurationService
    {
        /// <summary>
        /// Load connection string from appsettings.json
        /// </summary>
        public static string GetConnectionString()
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
                System.Diagnostics.Debug.WriteLine($"Connection string okuma hatası: {ex.Message}");
            }

            // Fallback
            return "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;";
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        public static async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                var connectionString = GetConnectionString();
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // Test query
                using var cmd = new NpgsqlCommand("SELECT 1", connection);
                await cmd.ExecuteScalarAsync();

                return (true, "Veritabanı bağlantısı başarılı!");
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return (false, $"PostgreSQL bağlantı hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Get SMTP configuration from appsettings.json
        /// </summary>
        public static (string Host, int Port, string Email, string Password, string Name) GetSmtpConfig()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var jsonString = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(jsonString);

                    // Try "Email" section first (new format)
                    if (doc.RootElement.TryGetProperty("Email", out var emailSection))
                    {
                        var host = emailSection.TryGetProperty("SmtpHost", out var h) ? h.GetString() ?? "smtp.gmail.com" : "smtp.gmail.com";
                        var port = emailSection.TryGetProperty("SmtpPort", out var p) ? p.GetInt32() : 587;
                        var email = emailSection.TryGetProperty("SenderEmail", out var e) ? e.GetString() ?? "" : "";
                        var password = emailSection.TryGetProperty("SenderPassword", out var pw) ? pw.GetString() ?? "" : "";
                        var name = emailSection.TryGetProperty("SenderName", out var n) ? n.GetString() ?? "NovaBank Security" : "NovaBank Security";

                        return (host, port, email, password, name);
                    }

                    // Fallback to "Smtp" section (legacy)
                    if (doc.RootElement.TryGetProperty("Smtp", out var smtpSection))
                    {
                        var host = smtpSection.TryGetProperty("Host", out var h) ? h.GetString() ?? "smtp.gmail.com" : "smtp.gmail.com";
                        var port = smtpSection.TryGetProperty("Port", out var p) ? p.GetInt32() : 587;
                        var email = smtpSection.TryGetProperty("Username", out var e) ? e.GetString() ?? "" : "";
                        var password = smtpSection.TryGetProperty("Password", out var pw) ? pw.GetString() ?? "" : "";
                        var name = smtpSection.TryGetProperty("From", out var n) ? n.GetString() ?? "NovaBank Security" : "NovaBank Security";

                        return (host, port, email, password, name);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMTP config okuma hatası: {ex.Message}");
            }

            return ("smtp.gmail.com", 587, "", "", "NovaBank Security");
        }

        /// <summary>
        /// Test SMTP email sending
        /// </summary>
        public static async Task<(bool Success, string Message)> TestEmailAsync(string testEmailAddress = "novabank.com@gmail.com")
        {
            try
            {
                var (host, port, email, password, name) = GetSmtpConfig();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                    email == "your_email@gmail.com" || password == "your_app_password")
                {
                    return (false, "E-posta yapılandırması eksik! appsettings.json'da Email bölümünü kontrol edin.");
                }

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(email, password),
                    Timeout = 10000 // 10 seconds
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email, name),
                    Subject = "NovaBank - Test E-posta",
                    Body = $"<h1>Test E-posta</h1><p>Bu bir test e-postasıdır.</p><p>Gönderim zamanı: {DateTime.Now}</p>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(testEmailAddress);

                await client.SendMailAsync(mailMessage);

                return (true, $"Test e-postası başarıyla gönderildi: {testEmailAddress}");
            }
            catch (SmtpException ex)
            {
                return (false, $"SMTP hatası: {ex.Message}\n\nLütfen appsettings.json'daki Email ayarlarını kontrol edin.");
            }
            catch (Exception ex)
            {
                return (false, $"E-posta gönderme hatası: {ex.Message}");
            }
        }
    }
}
