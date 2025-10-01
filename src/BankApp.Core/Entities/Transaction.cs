using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApp.Core.Entities
{
    public class Transaction : BaseEntity
    {
        public int AccountId { get; set; }
        public string TransactionType { get; set; } // Deposit, Withdraw, TransferIn, TransferOut
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
