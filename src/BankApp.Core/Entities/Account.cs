#nullable enable
using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Hesap entity sınıfı
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class Account : BaseEntity
    {
        private int _customerId;
        private string _accountNumber = string.Empty;
        private string _iban = string.Empty;
        private decimal _balance = 0;
        private string _currencyCode = "TRY";
        private DateTime _openedDate = DateTime.UtcNow;

        /// <summary>Bağlı müşteri ID (Foreign Key)</summary>
        public int CustomerId { get => _customerId; set => _customerId = value; }

        /// <summary>Hesap Numarası</summary>
        public string AccountNumber { get => _accountNumber; set => _accountNumber = value; }

        /// <summary>IBAN Numarası</summary>
        public string IBAN { get => _iban; set => _iban = value; }

        /// <summary>Hesap Bakiyesi</summary>
        public decimal Balance { get => _balance; set => _balance = value; }

        /// <summary>Para Birimi Kodu (TRY, USD, EUR)</summary>
        public string CurrencyCode { get => _currencyCode; set => _currencyCode = value; }

        /// <summary>Hesap Açılış Tarihi</summary>
        public DateTime OpenedDate { get => _openedDate; set => _openedDate = value; }
    }
}
