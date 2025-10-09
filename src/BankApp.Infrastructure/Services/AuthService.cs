using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BankApp.Core.Interfaces;
using BankApp.Core.Entities;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Kimlik doğrulama servisi
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IAuditRepository _auditRepository;

        /// <summary>
        /// AuthService yapıcı metodu
        /// </summary>
        /// <param name="userRepository">Kullanıcı repository</param>
        /// <param name="emailService">E-posta servisi</param>
        /// <param name="auditRepository">Denetim logu repository</param>
        public AuthService(IUserRepository userRepository, IEmailService emailService, IAuditRepository auditRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _auditRepository = auditRepository;
        }

        /// <summary>
        /// Kullanıcı girişi yapar
        /// </summary>
        /// <param name="username">Kullanıcı adı</param>
        /// <param name="password">Şifre</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                // Parametre kontrolü
                if (string.IsNullOrWhiteSpace(username))
                {
                    return "Kullanıcı adı boş olamaz.";
                }
                
                if (string.IsNullOrWhiteSpace(password))
                {
                    return "Şifre boş olamaz.";
                }

                // Kullanıcıyı bul
                User? user;
                try
                {
                    user = await _userRepository.GetByUsernameAsync(username);
                }
                catch (Exception ex)
                {
                    var sbError = new StringBuilder();
                    sbError.Append("Kullanıcı sorgusu hatası: ");
                    sbError.Append(ex.Message);
                    return sbError.ToString();
                }

                if (user == null)
                {
                    await LogAuditSafe(null, "LoginFailed", BuildString("User not found: ", username));
                    return "Kullanıcı bulunamadı.";
                }

                // Hesap doğrulanmış mı?
                if (!user.IsVerified)
                {
                    await LogAuditSafe(user.Id, "LoginFailed", "Account not verified.");
                    return "Hesabınız henüz doğrulanmamış. Lütfen e-postanızdaki doğrulama kodunu kullanın.";
                }

                // Hesap aktif mi?
                if (!user.IsActive)
                {
                    await LogAuditSafe(user.Id, "LoginFailed", "Account is inactive.");
                    return "Hesabınız aktif değil. Lütfen yönetici ile iletişime geçin.";
                }

                // Şifre doğrulama
                bool passwordValid;
                try
                {
                    string storedHash = user.PasswordHash ?? "";
                    passwordValid = VerifyPassword(password, storedHash);
                }
                catch (Exception ex)
                {
                    var sbError = new StringBuilder();
                    sbError.Append("Şifre doğrulama hatası: ");
                    sbError.Append(ex.Message);
                    return sbError.ToString();
                }

                if (passwordValid)
                {
                    await LogAuditSafe(user.Id, "LoginSuccess", "User logged in.");
                    return null; // Başarılı
                }

                await LogAuditSafe(user.Id, "LoginFailed", "Invalid password.");
                return "Şifre hatalı.";
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Giriş işlemi hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Yeni kullanıcı kaydeder
        /// </summary>
        /// <param name="user">Kullanıcı bilgileri</param>
        /// <param name="password">Şifre</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string?> RegisterAsync(User user, string password)
        {
            try
            {
                if (user == null)
                {
                    return "Kullanıcı bilgileri boş olamaz.";
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return "Şifre boş olamaz.";
                }

                // Kullanıcı adı kontrolü
                var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    return "Kullanıcı adı zaten var.";
                }
                
                // E-posta kontrolü
                var existingEmail = await _userRepository.GetByEmailAsync(user.Email);
                if (existingEmail != null)
                {
                    return "E-posta adresi zaten kayıtlı.";
                }

                // Şifreyi hashle
                user.PasswordHash = HashPassword(password);
                
                // Doğrulama kodu oluştur
                user.VerificationCode = GenerateOTP();
                user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
                user.IsVerified = false;
                user.IsActive = true;

                // Kullanıcıyı kaydet
                await _userRepository.AddAsync(user);
                
                // E-posta gönder
                var sbSubject = new StringBuilder();
                sbSubject.Append("Hesap Doğrulama - BankApp");
                
                var sbBody = new StringBuilder();
                sbBody.Append("<h3>Hoşgeldiniz!</h3>");
                sbBody.Append("<p>Hesabınızı doğrulamak için kodunuz: <b>");
                sbBody.Append(user.VerificationCode);
                sbBody.Append("</b></p>");
                
                await _emailService.SendEmailAsync(user.Email, sbSubject.ToString(), sbBody.ToString());
                
                await LogAuditSafe(null, "Register", BuildString("New user registered: ", user.Username));

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Kayıt hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Hesabı doğrular
        /// </summary>
        /// <param name="email">E-posta adresi</param>
        /// <param name="code">Doğrulama kodu</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string?> VerifyAccountAsync(string email, string code)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return "Kullanıcı bulunamadı.";
                }

                if (user.IsVerified)
                {
                    return null; // Zaten doğrulanmış
                }

                if (user.VerificationCode != code)
                {
                    return "Hatalı doğrulama kodu.";
                }

                if (user.VerificationCodeExpiry < DateTime.Now)
                {
                    return "Doğrulama kodunun süresi dolmuş. Lütfen tekrar kayıt olun veya kodu yenileyin.";
                }

                user.IsVerified = true;
                user.VerificationCode = null;
                user.VerificationCodeExpiry = null;

                await _userRepository.UpdateAsync(user);
                await LogAuditSafe(user.Id, "AccountVerified", "Email verified.");
                
                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Doğrulama hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Şifre sıfırlama e-postası gönderir
        /// </summary>
        /// <param name="email">E-posta adresi</param>
        /// <param name="otpCode">OTP kodu</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string?> SendForgotPasswordEmailAsync(string email, string otpCode)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                {
                    user.VerificationCode = otpCode;
                    user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);
                    await _userRepository.UpdateAsync(user);
                }

                var sbSubject = new StringBuilder();
                sbSubject.Append("Şifre Sıfırlama Kodu - BankApp");
                
                var sbBody = new StringBuilder();
                sbBody.Append("<h3>BankApp Güvenlik</h3>");
                sbBody.Append("<p>Şifre sıfırlama kodunuz: <b>");
                sbBody.Append(otpCode);
                sbBody.Append("</b></p>");
                sbBody.Append("<p>Bu kodu kimseyle paylaşmayınız.</p>");
                
                await _emailService.SendEmailAsync(email, sbSubject.ToString(), sbBody.ToString());
                
                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("E-posta gönderme hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Şifreyi hashler
        /// </summary>
        /// <param name="password">Şifre</param>
        /// <returns>Hash değeri</returns>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }
            
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Şifreyi doğrular
        /// </summary>
        /// <param name="inputPassword">Girilen şifre</param>
        /// <param name="storedHash">Kayıtlı hash</param>
        /// <returns>Eşleşme durumu</returns>
        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }

        /// <summary>
        /// OTP kodu oluşturur
        /// </summary>
        /// <returns>6 haneli OTP kodu</returns>
        public string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        /// <summary>
        /// StringBuilder ile string birleştirme yapar
        /// </summary>
        private string BuildString(params string[] parts)
        {
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                sb.Append(part);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Audit log kaydı ekler (hata yakalar)
        /// </summary>
        private async Task LogAuditSafe(int? userId, string action, string details)
        {
            try
            {
                await _auditRepository.AddLogAsync(new AuditLog 
                { 
                    UserId = userId,
                    Action = action, 
                    Details = details,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch 
            { 
                // Audit hataları göz ardı edilir
            }
        }
    }
}
