using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for Dictionary
using BankApp.Core.Interfaces;
using BankApp.Core.Entities;

namespace BankApp.Infrastructure.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IAuditRepository _auditRepository;

        // In-memory OTP store (Dictionary<email, otp>)
        private static readonly Dictionary<string, string> _otpStore = new Dictionary<string, string>();

        public AuthService(IUserRepository userRepository, IEmailService emailService, IAuditRepository auditRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _auditRepository = auditRepository;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                await _auditRepository.AddLogAsync(new AuditLog { Action = "LoginFailed", Details = $"User not found: {username}" });
                return false;
            }

            if (VerifyPassword(password, user.PasswordHash))
            {
                await _auditRepository.AddLogAsync(new AuditLog { UserId = user.Id, Action = "LoginSuccess", Details = "User logged in.", IpAddress = "127.0.0.1" });
                return true;
            }

            await _auditRepository.AddLogAsync(new AuditLog { UserId = user.Id, Action = "LoginFailed", Details = "Invalid password." });
            return false;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }

        public string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task SendForgotPasswordEmailAsync(string email, string otpCode)
        {
            string subject = "Şifre Sıfırlama Kodu - BankApp";
            string body = $"<h3>BankApp Güvenlik</h3><p>Şifre sıfırlama kodunuz: <b>{otpCode}</b></p><p>Bu kodu kimseyle paylaşmayınız.</p>";
            
            await _emailService.SendEmailAsync(email, subject, body);
        }
    }
}
