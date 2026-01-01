using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Kredi başvurusu entity
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class Loan : BaseEntity
    {
        private int _customerId;
        private int _userId;
        private decimal _amount;
        private int _termMonths;
        private decimal _interestRate = 3.5m;
        private string _status = "Pending";
        private DateTime _applicationDate = DateTime.Now;
        private DateTime? _decisionDate;
        private int? _approvedById;
        private string? _rejectionReason;
        private string? _notes;

        /// <summary>Başvuran müşteri ID</summary>
        public int CustomerId { get => _customerId; set => _customerId = value; }

        /// <summary>Bağlı kullanıcı ID (login yapan)</summary>
        public int UserId { get => _userId; set => _userId = value; }

        /// <summary>Talep edilen tutar</summary>
        public decimal Amount { get => _amount; set => _amount = value; }

        /// <summary>Vade (ay)</summary>
        public int TermMonths { get => _termMonths; set => _termMonths = value; }

        /// <summary>Faiz oranı (%)</summary>
        public decimal InterestRate { get => _interestRate; set => _interestRate = value; }

        /// <summary>Durum: Pending, Approved, Rejected</summary>
        public string Status { get => _status; set => _status = value; }

        /// <summary>Başvuru tarihi</summary>
        public DateTime ApplicationDate { get => _applicationDate; set => _applicationDate = value; }

        /// <summary>Onay/Red tarihi</summary>
        public DateTime? DecisionDate { get => _decisionDate; set => _decisionDate = value; }

        /// <summary>Onaylayan Admin ID</summary>
        public int? ApprovedById { get => _approvedById; set => _approvedById = value; }

        /// <summary>Red nedeni (varsa)</summary>
        public string? RejectionReason { get => _rejectionReason; set => _rejectionReason = value; }

        /// <summary>Başvuru notu</summary>
        public string? Notes { get => _notes; set => _notes = value; }

        /// <summary>Aylık taksit tutarı</summary>
        public decimal MonthlyPayment => CalculateMonthlyPayment();

        /// <summary>Toplam geri ödeme</summary>
        public decimal TotalRepayment => MonthlyPayment * TermMonths;

        // Navigation
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Aylık taksit hesapla (basit formül)
        /// </summary>
        /// <returns>Aylık taksit tutarı</returns>
        private decimal CalculateMonthlyPayment()
        {
            if (TermMonths <= 0) return 0;
            decimal totalInterest = Amount * (InterestRate / 100) * (TermMonths / 12m);
            return (Amount + totalInterest) / TermMonths;
        }
    }
}
