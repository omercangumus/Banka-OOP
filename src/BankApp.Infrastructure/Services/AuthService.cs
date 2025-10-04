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

        // In-memory OTP store removed in favor of Database persistence


        public AuthService(IUserRepository userRepository, IEmailService emailService, IAuditRepository auditRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _auditRepository = auditRepository;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username), "Kullanıcı adı boş olamaz.");
            }
            
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password), "Şifre boş olamaz.");
            }

            // Step 1: Get user
            Core.Entities.User user;
            try
            {
                user = await _userRepository.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                throw new Exception($"Kullanıcı sorgusu hatası: {ex.Message}");
            }

            if (user == null)
            {
                // Step 2a: Log failed attempt (user not found)
                try
                {
                    await _auditRepository.AddLogAsync(new AuditLog 
                    { 
                        Action = "LoginFailed", 
                        Details = $"User not found: {username ?? "null"}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch { /* Ignore audit errors */ }
                return false;
            }

            // Step 2b: Check if user is verified (SORUN: Eksik kontrol eklendi)
            if (!user.IsVerified)
            {
                try
                {
                    await _auditRepository.AddLogAsync(new AuditLog 
                    { 
                        UserId = user.Id,
                        Action = "LoginFailed", 
                        Details = "Account not verified.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch { /* Ignore audit errors */ }
                throw new Exception("Hesabınız henüz doğrulanmamış. Lütfen e-postanızdaki doğrulama kodunu kullanın.");
            }

            // Step 2c: Check if user is active
            if (!user.IsActive)
            {
                try
                {
                    await _auditRepository.AddLogAsync(new AuditLog 
                    { 
                        UserId = user.Id,
                        Action = "LoginFailed", 
                        Details = "Account is inactive.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch { /* Ignore audit errors */ }
                throw new Exception("Hesabınız aktif değil. Lütfen yönetici ile iletişime geçin.");
            }

            // Step 3: Verify password
            bool passwordValid;
            try
            {
                string storedHash = user.PasswordHash ?? "";
                passwordValid = VerifyPassword(password ?? "", storedHash);
            }
            catch (Exception ex)
            {
                throw new Exception($"Şifre doğrulama hatası: {ex.Message}");
            }

            if (passwordValid)
            {
                // Step 4a: Log success
                try
                {
                    await _auditRepository.AddLogAsync(new AuditLog 
                    { 
                        UserId = user.Id, 
                        Action = "LoginSuccess", 
                        Details = "User logged in.",
                        IpAddress = "127.0.0.1",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch { /* Ignore audit errors */ }
                return true;
            }

            // Step 4b: Log failure
            try
            {
                await _auditRepository.AddLogAsync(new AuditLog 
                { 
                    UserId = user.Id, 
                    Action = "LoginFailed", 
                    Details = "Invalid password.",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch { /* Ignore audit errors */ }
            return false;
        }

        public string HashPassword(string password)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Şifre boş olamaz.");
            }
            
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

        public async Task<bool> RegisterAsync(User user, string password)
        {
            // Check if user exists
            var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
            if (existingUser != null) throw new Exception("Kullanıcı adı zaten var.");
            
            var existingEmail = await _userRepository.GetByEmailAsync(user.Email);
            if (existingEmail != null) throw new Exception("E-posta adresi zaten kayıtlı.");

            // Hash password
            user.PasswordHash = HashPassword(password);
            
            // Generate Verification Code
            user.VerificationCode = GenerateOTP();
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
            user.IsVerified = false;
            user.IsActive = true;

            // Save User
            await _userRepository.AddAsync(user);
            
            // Send Email
            string subject = "Hesap Doğrulama - BankApp";
            string body = $"<h3>Hoşgeldiniz!</h3><p>Hesabınızı doğrulamak için kodunuz: <b>{user.VerificationCode}</b></p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);
            
            await _auditRepository.AddLogAsync(new AuditLog { Action = "Register", Details = $"New user registered: {user.Username}" });

            return true;
        }

        public async Task<bool> VerifyAccountAsync(string email, string code)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (user.IsVerified) return true; // Already verified

            if (user.VerificationCode != code)
                throw new Exception("Hatalı doğrulama kodu.");

            if (user.VerificationCodeExpiry < DateTime.Now)
                throw new Exception("Doğrulama kodunun süresi dolmuş. Lütfen tekrar kayıt olun veya kodu yenileyin.");

            user.IsVerified = true;
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;

            await _userRepository.UpdateAsync(user);
            await _auditRepository.AddLogAsync(new AuditLog { UserId = user.Id, Action = "AccountVerified", Details = "Email verified." });
            
            return true;
        }

        public async Task SendForgotPasswordEmailAsync(string email, string otpCode)
        {
            // Update User with OTP
            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                user.VerificationCode = otpCode;
                user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
                await _userRepository.UpdateAsync(user);
            }

            string subject = "Şifre Sıfırlama Kodu - BankApp";
            string body = $"<h3>BankApp Güvenlik</h3><p>Şifre sıfırlama kodunuz: <b>{otpCode}</b></p><p>Bu kodu kimseyle paylaşmayınız.</p>";
            
            await _emailService.SendEmailAsync(email, subject, body);
        }
    }
}
