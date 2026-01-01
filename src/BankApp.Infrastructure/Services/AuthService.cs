using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using BankApp.Core.Interfaces;
using BankApp.Core.Entities;

namespace BankApp.Infrastructure.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IAuditRepository _auditRepository;

        public AuthService(IUserRepository userRepository, IEmailService emailService, IAuditRepository auditRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _auditRepository = auditRepository;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return "Kullanıcı adı boş olamaz.";
            if (string.IsNullOrWhiteSpace(password)) return "Şifre boş olamaz.";

            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null) return "Kullanıcı adı veya şifre hatalı."; // Güvenlik için detay verme

                if (!user.IsVerified) return "Hesap doğrulanmamış.";
                if (!user.IsActive) return "Hesap aktif değil.";

                if (VerifyPassword(password, user.PasswordHash))
                {
                    await LogAuditAsync(user.Id, "LoginSuccess", "Giriş yapıldı.");
                    return null; // Başarılı
                }
                
                await LogAuditAsync(user.Id, "LoginFailed", "Hatalı şifre.");
                return "Kullanıcı adı veya şifre hatalı.";
            }
            catch (Exception ex)
            {
                return $"Sistem hatası: {ex.Message}";
            }
        }

        public async Task<string> RegisterAsync(User user, string password)
        {
            try
            {
                var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
                if (existingUser != null) return "Kullanıcı adı alınmış.";
                
                user.PasswordHash = HashPassword(password);
                user.VerificationCode = "123456"; // Basitlik için sabit veya random
                user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
                user.IsVerified = false;
                user.IsActive = true;

                await _userRepository.AddAsync(user);
                await LogAuditAsync(null, "Register", $"New user: {user.Username}");
                return null;
            }
            catch (Exception ex)
            {
                return $"Kayıt hatası: {ex.Message}";
            }
        }

        private async Task LogAuditAsync(int? userId, string action, string details)
        {
            try { await _auditRepository.AddLogAsync(new AuditLog { UserId = userId ?? 0, Action = action, Details = details, CreatedAt = DateTime.UtcNow }); } catch { }
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public bool VerifyPassword(string input, string hash) => HashPassword(input) == hash;
        
        // Mevcut kodların yapısını koruyarak string dönüşü (veya orijinal) mantığı:
        public async Task SendForgotPasswordEmailAsync(string email, string code) 
        { 
             try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                // Kullanıcı varsa kodu güncelle, yoksa güvenlik gereği belli etme (veya senaryoya göre)
                if (user != null)
                {
                    user.VerificationCode = code;
                    user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
                    await _userRepository.UpdateAsync(user);
                    
                    await _emailService.SendEmailAsync(email, "Şifre Sıfırlama Kodu", $"Kodunuz: {code}");
                }
            }
            catch
            {
                // Hata yutulabilir veya loglanabilir
            }
        }

        public async Task<string> VerifyAccountAsync(string email, string code) 
        { 
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null) return "Kullanıcı bulunamadı.";
                if (user.IsVerified) return null; // Zaten doğrulanmış
                
                if (user.VerificationCode == code && user.VerificationCodeExpiry > DateTime.Now)
                {
                    user.IsVerified = true;
                    await _userRepository.UpdateAsync(user);
                    await LogAuditAsync(user.Id, "AccountVerified", "Email verified.");
                    return null;
                }
                return "Kod hatalı veya süresi dolmuş.";
            }
            catch (Exception ex)
            {
                return $"Doğrulama hatası: {ex.Message}";
            }
        }

        public string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
