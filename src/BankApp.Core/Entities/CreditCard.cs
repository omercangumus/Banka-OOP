#nullable enable
using System;
using System.Linq;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Kredi kartı entity - Görsel kart tasarımı için tüm bilgiler
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class CreditCard : BaseEntity
    {
        private int _customerId;
        private string _cardNumber = string.Empty;
        private string _cvv = string.Empty;
        private DateTime _expiryDate;
        private decimal _totalLimit;
        private decimal _availableLimit;
        private decimal _currentDebt;
        private int _cutoffDay = 15;
        private string _cardType = "Virtual";
        private string _colorTheme = "Purple";
        private bool _isActive = true;
        private string _cardHolderName = string.Empty;

        /// <summary>Hangi müşterinin</summary>
        public int CustomerId { get => _customerId; set => _customerId = value; }

        /// <summary>16 haneli kart numarası</summary>
        public string CardNumber { get => _cardNumber; set => _cardNumber = value; }

        /// <summary>3 haneli güvenlik kodu</summary>
        public string CVV { get => _cvv; set => _cvv = value; }

        /// <summary>Son kullanma tarihi</summary>
        public DateTime ExpiryDate { get => _expiryDate; set => _expiryDate = value; }

        /// <summary>Toplam limit (TL)</summary>
        public decimal TotalLimit { get => _totalLimit; set => _totalLimit = value; }

        /// <summary>Kullanılabilir limit</summary>
        public decimal AvailableLimit { get => _availableLimit; set => _availableLimit = value; }

        /// <summary>Güncel borç</summary>
        public decimal CurrentDebt { get => _currentDebt; set => _currentDebt = value; }

        /// <summary>Hesap kesim günü (1-28)</summary>
        public int CutoffDay { get => _cutoffDay; set => _cutoffDay = value; }

        /// <summary>Kart tipi: Virtual, Physical, Gold, Platinum</summary>
        public string CardType { get => _cardType; set => _cardType = value; }

        /// <summary>Kart renk teması: Purple, Gold, Black, Blue</summary>
        public string ColorTheme { get => _colorTheme; set => _colorTheme = value; }

        /// <summary>Aktif mi?</summary>
        public bool IsActive { get => _isActive; set => _isActive = value; }

        /// <summary>Kart sahibi ismi (kartın üzerinde görünecek)</summary>
        public string CardHolderName { get => _cardHolderName; set => _cardHolderName = value; }

        // Navigation
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Maskelenmiş kart numarası (**** **** **** 1234)
        /// </summary>
        public string MaskedCardNumber => CardNumber?.Length >= 16 
            ? $"**** **** **** {CardNumber.Substring(12)}" 
            : "****";

        /// <summary>
        /// Yeni sanal kart oluşturur (statik factory method)
        /// </summary>
        /// <param name="customerId">Müşteri ID</param>
        /// <param name="holderName">Kart sahibi adı</param>
        /// <param name="limit">Kart limiti</param>
        /// <returns>Yeni kredi kartı</returns>
        public static CreditCard CreateVirtualCard(int customerId, string holderName, decimal limit)
        {
            var random = new Random();
            
            // Visa formatında kart numarası (4 ile başlar)
            string cardNumber = "4" + string.Join("", Enumerable.Range(0, 15).Select(_ => random.Next(0, 10)));
            
            // Luhn algoritması ile son haneyi düzelt
            cardNumber = ApplyLuhnChecksum(cardNumber.Substring(0, 15));
            
            return new CreditCard
            {
                CustomerId = customerId,
                CardNumber = cardNumber,
                CVV = random.Next(100, 999).ToString("D3"),
                ExpiryDate = DateTime.Now.AddYears(4),
                TotalLimit = limit,
                AvailableLimit = limit,
                CurrentDebt = 0,
                CardType = "Virtual",
                ColorTheme = new[] { "Purple", "Gold", "Black", "Blue" }[random.Next(4)],
                CardHolderName = holderName.ToUpper(),
                IsActive = true
            };
        }

        /// <summary>
        /// Luhn algoritması ile geçerli kart numarası oluşturur
        /// </summary>
        /// <param name="partialNumber">Kısmi kart numarası</param>
        /// <returns>Tam geçerli kart numarası</returns>
        private static string ApplyLuhnChecksum(string partialNumber)
        {
            int sum = 0;
            bool alternate = true;
            
            for (int i = partialNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(partialNumber[i].ToString());
                
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }
                
                sum += digit;
                alternate = !alternate;
            }
            
            int checkDigit = (10 - (sum % 10)) % 10;
            return partialNumber + checkDigit;
        }
    }
}
