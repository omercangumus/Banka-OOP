#nullable enable
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BankApp.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BankApp.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private string _smtpHost = "smtp.gmail.com";
        private int _smtpPort = 587;
        private string _senderEmail = "novabank.com@gmail.com";
        private string _senderPassword = "qbnh ihos fife dnuz";
        private string _senderName = "NovaBank Security";

        // Static event for UI to subscribe to simulated emails
        public static event Action<string, string, string>? OnEmailSimulated;

        public SmtpEmailService()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var jsonString = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(jsonString);
                    
                    if (doc.RootElement.TryGetProperty("Email", out var emailSection))
                    {
                        if (emailSection.TryGetProperty("SmtpHost", out var host))
                            _smtpHost = host.GetString() ?? "smtp.gmail.com";
                        if (emailSection.TryGetProperty("SmtpPort", out var port))
                            _smtpPort = port.GetInt32();
                        if (emailSection.TryGetProperty("SenderEmail", out var email))
                            _senderEmail = email.GetString() ?? "";
                        if (emailSection.TryGetProperty("SenderPassword", out var password))
                            _senderPassword = password.GetString() ?? "";
                        if (emailSection.TryGetProperty("SenderName", out var name))
                            _senderName = name.GetString() ?? "NovaBank Security";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email config load error: {ex.Message}");
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Check if email is configured properly
            if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword) || 
                _senderEmail == "your_email@gmail.com" || _senderPassword == "your_app_password")
            {
                System.Diagnostics.Debug.WriteLine($"[EMAIL] Simulated email to {to}: {subject}");
                OnEmailSimulated?.Invoke(to, subject, body);
                return;
            }

            try
            {
                // Create message using MimeKit
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_senderName, _senderEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                // Create HTML body
                var builder = new BodyBuilder();
                builder.HtmlBody = body;
                message.Body = builder.ToMessageBody();

                // Send using MailKit SmtpClient
                using var client = new SmtpClient();
                
                // Connect with STARTTLS
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                
                // Authenticate
                await client.AuthenticateAsync(_senderEmail, _senderPassword);
                
                // Send
                await client.SendAsync(message);
                
                // Disconnect
                await client.DisconnectAsync(true);

                System.Diagnostics.Debug.WriteLine($"[EMAIL] Successfully sent to {to}: {subject}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EMAIL ERROR] {ex.Message}");
                throw new Exception($"Email gönderilemedi: {ex.Message}", ex);
            }
        }
    }
}
