#nullable enable
using System;

namespace BankApp.Core.Entities
{
    public class Transaction : BaseEntity
    {
        public int AccountId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Deposit, Withdraw, TransferIn, TransferOut
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}

