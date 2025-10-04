#nullable enable
using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Vadeli hesap entity - Faiz hesaplama ve vade takibi
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class TimeDepositAccount : BaseEntity
    {
        private int _customerId;
        private int _sourceAccountId;
        private decimal _principalAmount;
        private decimal _interestRate;
        private DateTime _openDate = DateTime.Now;
        private DateTime _maturityDate;
        private bool _isActive = true;
        private string _currencyCode = "TRY";

        /// <summary>Hangi müşterinin</summary>
        public int CustomerId { get => _customerId; set => _customerId = value; }

        /// <summary>Bağlı vadesiz hesap ID</summary>
        public int SourceAccountId { get => _sourceAccountId; set => _sourceAccountId = value; }

        /// <summary>Ana para tutarı</summary>
        public decimal PrincipalAmount { get => _principalAmount; set => _principalAmount = value; }

        /// <summary>Yıllık faiz oranı (örn: 45.0 = %45)</summary>
        public decimal InterestRate { get => _interestRate; set => _interestRate = value; }

        /// <summary>Açılış tarihi</summary>
        public DateTime OpenDate { get => _openDate; set => _openDate = value; }

        /// <summary>Vade bitiş tarihi</summary>
        public DateTime MaturityDate { get => _maturityDate; set => _maturityDate = value; }

        /// <summary>Aktif mi (bozulmadı mı)?</summary>
        public bool IsActive { get => _isActive; set => _isActive = value; }

        /// <summary>Para birimi</summary>
        public string CurrencyCode { get => _currencyCode; set => _currencyCode = value; }

        /// <summary>Vade gün sayısı</summary>
        public int TermDays => (MaturityDate - OpenDate).Days;

        /// <summary>Tahmini getiri (basit faiz)</summary>
        public decimal EstimatedReturn => CalculateReturn();

        /// <summary>Vade sonu toplam tutar</summary>
        public decimal TotalAtMaturity => PrincipalAmount + EstimatedReturn;

        /// <summary>Vade doldu mu?</summary>
        public bool IsMatured => DateTime.Now >= MaturityDate;

        // Navigation
        public virtual Customer? Customer { get; set; }
        public virtual Account? SourceAccount { get; set; }

        /// <summary>
        /// Basit faiz formülü ile getiri hesapla
        /// </summary>
        private decimal CalculateReturn()
        {
            // Basit faiz: A = P * r * t / 365
            return PrincipalAmount * (InterestRate / 100) * TermDays / 365;
        }

        /// <summary>
        /// Yeni vadeli hesap oluşturur
        /// </summary>
        /// <param name="customerId">Müşteri ID</param>
        /// <param name="sourceAccountId">Kaynak hesap ID</param>
        /// <param name="principal">Ana para</param>
        /// <param name="termDays">Vade gün sayısı</param>
        /// <param name="interestRate">Faiz oranı</param>
        /// <returns>Yeni vadeli hesap</returns>
        public static TimeDepositAccount Create(int customerId, int sourceAccountId, decimal principal, int termDays, decimal interestRate)
        {
            return new TimeDepositAccount
            {
                CustomerId = customerId,
                SourceAccountId = sourceAccountId,
                PrincipalAmount = principal,
                InterestRate = interestRate,
                OpenDate = DateTime.Now,
                MaturityDate = DateTime.Now.AddDays(termDays),
                IsActive = true
            };
        }
    }
}
