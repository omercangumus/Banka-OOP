#nullable enable
using System;

namespace BankApp.Core.Entities
{
    public class Account : BaseEntity
    {
        public int CustomerId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string IBAN { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public string CurrencyCode { get; set; } = "TRY";
        public DateTime OpenedDate { get; set; } = DateTime.UtcNow;
    }
}

