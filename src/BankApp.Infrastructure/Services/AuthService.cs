using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Npgsql;
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
                // SORUN DÜZELTİLDİ: Veritabanı bağlantı hatası kontrolü
                User? user = null;
                try
                {
                    user = await _userRepository.GetByUsernameAsync(username);
                }
                catch (Npgsql.NpgsqlException dbEx)
                {
                    // Veritabanı bağlantı hatası
                    if (dbEx.Message.Contains("Failed to connect") || dbEx.Message.Contains("5432") || dbEx.Message.Contains("timeout"))
                    {
                        return "Veritabanı bağlantı hatası! PostgreSQL servisi çalışıyor mu? Lütfen servisleri kontrol edin.";
                    }
                    return $"Veritabanı hatası: {dbEx.Message}";
                }
                catch (Exception dbEx)
                {
                    return $"Kullanıcı sorgulama hatası: {dbEx.Message}";
                }

                if (user == null) 
                {
                    await LogAuditAsync(null, "LoginFailed", "Kullanıcı bulunamadı.");
                    return "Kullanıcı adı veya şifre hatalı."; // Güvenlik için detay verme
                }

                // PHASE 5: IsVerified kontrolü kaldırıldı - doğrulanmamış kullanıcılar da giriş yapabilir
                // Özellik kısıtlamaları UI tarafında yapılacak (AppEvents.CurrentSession.IsVerified check)
                
                if (!user.IsActive) return "Hesap aktif değil.";
                // PHASE 5: Add IsBanned check (fallback to IsActive = false if column doesn't exist)
                // Note: IsBanned property may not exist yet, so we use reflection or fallback
                try
                {
                    // Try to check IsBanned property via reflection
                    var isBannedProp = user.GetType().GetProperty("IsBanned");
                    if (isBannedProp != null)
                    {
                        var isBanned = (bool?)isBannedProp.GetValue(user);
                        if (isBanned == true) return "Hesabınız yasaklanmıştır. Lütfen yönetici ile iletişime geçin.";
                    }
                    else
                    {
                        // Fallback: If IsBanned column doesn't exist, use IsActive
                        if (!user.IsActive) return "Hesabınız yasaklanmıştır. Lütfen yönetici ile iletişime geçin.";
                    }
                }
                catch
                {
                    // Fallback: If IsBanned column doesn't exist, use IsActive
                    if (!user.IsActive) return "Hesabınız yasaklanmıştır. Lütfen yönetici ile iletişime geçin.";
                }

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
                // SORUN DÜZELTİLDİ: Daha detaylı hata mesajı
                string errorMsg = $"Sistem hatası: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\nİç Hata: {ex.InnerException.Message}";
                }
                return errorMsg;
            }
        }

        public async Task<string> RegisterAsync(User user, string password)
        {
            try
            {
                var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
                if (existingUser != null) return "Kullanıcı adı alınmış.";
                
                // Email kontrolü
                var existingEmail = await _userRepository.GetByEmailAsync(user.Email);
                if (existingEmail != null) return "Bu e-posta adresi zaten kayıtlı.";
                
                user.PasswordHash = HashPassword(password);
                user.VerificationCode = GenerateOTP(); // 6 haneli random kod
                user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
                user.IsVerified = false;
                user.IsActive = true;

                await _userRepository.AddAsync(user);
                await LogAuditAsync(null, "Register", $"New user: {user.Username}");
                
                // Doğrulama emaili gönder
                try
                {
                    var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>NovaBank Hesap Doğrulama</h2>
                        <p>Merhaba <strong>{user.FullName ?? user.Username}</strong>,</p>
                        <p>NovaBank'a kayıt olduğunuz için teşekkür ederiz.</p>
                        <p>Hesabınızı doğrulamak için aşağıdaki kodu kullanın:</p>
                        <div style='background: #3498db; color: white; padding: 15px 30px; font-size: 24px; font-weight: bold; display: inline-block; border-radius: 5px; letter-spacing: 3px;'>
                            {user.VerificationCode}
                        </div>
                        <p style='margin-top: 20px;'>Bu kod 15 dakika içinde geçerliliğini yitirecektir.</p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='color: #7f8c8d; font-size: 12px;'>Bu e-postayı siz talep etmediyseniz, lütfen dikkate almayın.</p>
                    </body>
                    </html>";
                    
                    await _emailService.SendEmailAsync(user.Email, "NovaBank - Hesap Doğrulama Kodu", emailBody);
                    System.Diagnostics.Debug.WriteLine($"[AUTH] Verification email sent to {user.Email}");
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[AUTH] Email send failed: {emailEx.Message}");
                    // Email gönderilemese de kayıt başarılı sayılır, kod UI'da gösterilir
                }
                
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
        
        /// <summary>
        /// 6 haneli OTP (One-Time Password) kodu üretir
        /// </summary>
        /// <returns>6 haneli sayısal kod</returns>
        public string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        
        public async Task SendForgotPasswordEmailAsync(string email, string code) 
        { 
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                {
                    user.VerificationCode = code;
                    user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
                    await _userRepository.UpdateAsync(user);
                    
                    var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #e74c3c;'>NovaBank Şifre Sıfırlama</h2>
                        <p>Merhaba <strong>{user.FullName ?? user.Username}</strong>,</p>
                        <p>Şifre sıfırlama talebiniz alındı.</p>
                        <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:</p>
                        <div style='background: #e74c3c; color: white; padding: 15px 30px; font-size: 24px; font-weight: bold; display: inline-block; border-radius: 5px; letter-spacing: 3px;'>
                            {code}
                        </div>
                        <p style='margin-top: 20px;'>Bu kod 15 dakika içinde geçerliliğini yitirecektir.</p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='color: #7f8c8d; font-size: 12px;'>Bu e-postayı siz talep etmediyseniz, hesabınızın güvenliği için lütfen şifrenizi değiştirin.</p>
                    </body>
                    </html>";
                    
                    await _emailService.SendEmailAsync(email, "NovaBank - Şifre Sıfırlama Kodu", emailBody);
                    System.Diagnostics.Debug.WriteLine($"[AUTH] Password reset email sent to {email}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Password reset email failed: {ex.Message}");
                throw; // Hatayı yukarı ilet
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
    }
}
