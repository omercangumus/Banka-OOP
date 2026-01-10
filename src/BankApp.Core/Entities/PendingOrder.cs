using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Bekleyen Emir - Limit/Stop emirleri için
    /// Market emirleri anlık işlenir, Limit/Stop bekler
    /// </summary>
    public class PendingOrder
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int AccountId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string OrderType { get; set; } = "Limit"; // Limit, Stop, Stop-Limit
        public string Side { get; set; } = "Buy"; // Buy, Sell
        public decimal Quantity { get; set; }
        public decimal LimitPrice { get; set; }
        public decimal? StopPrice { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Filled, Cancelled, Expired
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FilledAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
    }
}
