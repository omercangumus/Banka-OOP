#nullable enable
using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Müşteri entity sınıfı
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class Customer : BaseEntity
    {
        private int _userId;
        private string _identityNumber = string.Empty;
        private string? _firstName;
        private string? _lastName;
        private string? _phoneNumber;
        private string? _email;
        private string? _address;
        private DateTime? _dateOfBirth;

        /// <summary>Bağlı kullanıcı ID (Foreign Key)</summary>
        public int UserId { get => _userId; set => _userId = value; }

        /// <summary>TC Kimlik No</summary>
        public string IdentityNumber { get => _identityNumber; set => _identityNumber = value; }

        /// <summary>Müşteri Adı</summary>
        public string? FirstName { get => _firstName; set => _firstName = value; }

        /// <summary>Müşteri Soyadı</summary>
        public string? LastName { get => _lastName; set => _lastName = value; }

        /// <summary>Telefon Numarası</summary>
        public string? PhoneNumber { get => _phoneNumber; set => _phoneNumber = value; }

        /// <summary>E-Posta Adresi</summary>
        public string? Email { get => _email; set => _email = value; }

        /// <summary>Adres Bilgisi</summary>
        public string? Address { get => _address; set => _address = value; }

        /// <summary>Doğum Tarihi</summary>
        public DateTime? DateOfBirth { get => _dateOfBirth; set => _dateOfBirth = value; }
    }
}
