using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApp.Core.Entities
{
    public class Account : BaseEntity
    {
        public int CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string IBAN { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime OpenedDate { get; set; } = DateTime.Now;

        // Navigation Property: Relation to Customer won't be strictly enforced by EF without ID mapping,
        // but for Dapper we use ID.
    }
}
