using System;

namespace BankApp.Infrastructure.Events
{
    /// <summary>
    /// Portfolio and Dashboard refresh events
    /// </summary>
    public static class PortfolioEvents
    {
        /// <summary>
        /// Fired when portfolio data changes (buy/sell transactions)
        /// </summary>
        public static event EventHandler<PortfolioChangedEventArgs> PortfolioChanged;
        
        /// <summary>
        /// Fired when general transaction occurs (deposit/withdraw/transfer)
        /// </summary>
        public static event EventHandler<TransactionChangedEventArgs> TransactionChanged;
        
        /// <summary>
        /// Fired when loan status changes
        /// </summary>
        public static event EventHandler<LoanChangedEventArgs> LoanChanged;

        /// <summary>
        /// Trigger portfolio change event
        /// </summary>
        public static void OnPortfolioChanged(int userId, string changeType = "Updated")
        {
            PortfolioChanged?.Invoke(null, new PortfolioChangedEventArgs 
            { 
                UserId = userId, 
                ChangeType = changeType,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Trigger transaction change event
        /// </summary>
        public static void OnTransactionChanged(int userId, decimal amount, string transactionType)
        {
            TransactionChanged?.Invoke(null, new TransactionChangedEventArgs 
            { 
                UserId = userId, 
                Amount = amount,
                TransactionType = transactionType,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Trigger loan change event
        /// </summary>
        public static void OnLoanChanged(int userId, string changeType)
        {
            LoanChanged?.Invoke(null, new LoanChangedEventArgs 
            { 
                UserId = userId, 
                ChangeType = changeType,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public class PortfolioChangedEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public string ChangeType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TransactionChangedEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class LoanChangedEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public string ChangeType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
