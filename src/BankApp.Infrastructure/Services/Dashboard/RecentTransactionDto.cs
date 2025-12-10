using System;

namespace BankApp.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// Son işlemler için DTO
    /// </summary>
    public class RecentTransactionDto
    {
        public int Id { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string AccountNumber { get; set; }
    }
}
