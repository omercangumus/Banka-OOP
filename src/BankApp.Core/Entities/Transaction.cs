#nullable enable
using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// İşlem entity sınıfı
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class Transaction : BaseEntity
    {
        private int _accountId;
        private string _transactionType = string.Empty;
        private decimal _amount;
        private string? _description;
        private DateTime _transactionDate = DateTime.UtcNow;

        /// <summary>Bağlı hesap ID (Foreign Key)</summary>
        public int AccountId { get => _accountId; set => _accountId = value; }

        /// <summary>İşlem Tipi (Deposit, Withdraw, TransferIn, TransferOut)</summary>
        public string TransactionType { get => _transactionType; set => _transactionType = value; }

        /// <summary>İşlem Tutarı</summary>
        public decimal Amount { get => _amount; set => _amount = value; }

        /// <summary>İşlem Açıklaması</summary>
        public string? Description { get => _description; set => _description = value; }

        /// <summary>İşlem Tarihi</summary>
        public DateTime TransactionDate { get => _transactionDate; set => _transactionDate = value; }
    }
}
