#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using BankApp.Core.Interfaces;

namespace BankApp.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _senderEmail = "novabank.com@gmail.com";
        private readonly string _senderPassword = "qbnh ihos fife dnuz";
        private readonly string _senderName = "NovaBank Security";

        // Static event for UI to subscribe to simulated emails - SORUN DÜZELTİLDİ: Nullable annotation eklendi
        public static event Action<string, string, string>? OnEmailSimulated;

        public SmtpEmailService()
        {
            // Try to load from appsettings.json
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var jsonString = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(jsonString);
                    var emailSection = doc.RootElement.GetProperty("Email");
                    
                    _smtpHost = emailSection.GetProperty("SmtpHost").GetString() ?? "smtp.gmail.com";
                    _smtpPort = emailSection.GetProperty("SmtpPort").GetInt32();
                    _senderEmail = emailSection.GetProperty("SenderEmail").GetString() ?? "";
                    _senderPassword = emailSection.GetProperty("SenderPassword").GetString() ?? "";
                    _senderName = emailSection.GetProperty("SenderName").GetString() ?? "NovaBank Security";
                }
                else
                {
                    // Fallback to environment variables or defaults
                    _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
                    _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
                    _senderEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL") ?? "your_email@gmail.com";
                    _senderPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "your_app_password";
                    _senderName = "NovaBank Security";
                }
            }
            catch
            {
                // Default fallback values
                _smtpHost = "smtp.gmail.com";
                _smtpPort = 587;
                _senderEmail = "your_email@gmail.com";
                _senderPassword = "your_app_password";
                _senderName = "NovaBank Security";
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Check if email is configured properly
            if (_senderEmail == "your_email@gmail.com" || _senderPassword == "your_app_password")
            {
                // Log warning for development/testing
                System.Diagnostics.Debug.WriteLine($"[EMAIL] Simulated email to {to}: {subject}");
                System.Diagnostics.Debug.WriteLine($"[EMAIL] Body: {body}");
                
                // Trigger event for UI to show message (if subscribed)
                OnEmailSimulated?.Invoke(to, subject, body);
                
                // Don't throw, just return for development mode
                await Task.CompletedTask;
                return;
            }

            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_senderEmail, _senderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Email gönderilemedi: {ex.Message}");
            }
        }
    }
}


