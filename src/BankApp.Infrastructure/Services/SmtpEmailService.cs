using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BankApp.Core.Interfaces;

namespace BankApp.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        // TODO: Replace with real credentials or move to App Settings
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _senderEmail = "your_email@gmail.com";
        private readonly string _senderPassword = "your_app_password";

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_senderEmail, "BankApp Security"),
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
                // Log failure
                throw new Exception($"Email gönderilemedi: {ex.Message}");
            }
        }
    }
}
