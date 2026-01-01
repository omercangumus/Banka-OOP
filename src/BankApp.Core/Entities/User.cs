#nullable enable
using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Kullanıcı entity sınıfı
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class User : BaseEntity
    {
        private string _username = string.Empty;
        private string _passwordHash = string.Empty;
        private string _email = string.Empty;
        private string _role = "Customer";
        private string? _fullName;
        private bool _isActive = true;
        private string? _verificationCode;
        private DateTime? _verificationCodeExpiry;
        private bool _isVerified = false;

        /// <summary>Kullanıcı Adı</summary>
        public string Username { get => _username; set => _username = value; }

        /// <summary>Şifre Hash Değeri</summary>
        public string PasswordHash { get => _passwordHash; set => _passwordHash = value; }

        /// <summary>E-Posta Adresi</summary>
        public string Email { get => _email; set => _email = value; }

        /// <summary>Kullanıcı Rolü (Admin, Staff, Customer)</summary>
        public string Role { get => _role; set => _role = value; }

        /// <summary>Tam Ad</summary>
        public string? FullName { get => _fullName; set => _fullName = value; }

        /// <summary>Aktif Durumu</summary>
        public bool IsActive { get => _isActive; set => _isActive = value; }

        /// <summary>E-Posta Doğrulama Kodu</summary>
        public string? VerificationCode { get => _verificationCode; set => _verificationCode = value; }

        /// <summary>Doğrulama Kodu Son Geçerlilik Tarihi</summary>
        public DateTime? VerificationCodeExpiry { get => _verificationCodeExpiry; set => _verificationCodeExpiry = value; }

        /// <summary>E-Posta Doğrulanma Durumu</summary>
        public bool IsVerified { get => _isVerified; set => _isVerified = value; }
    }
}
