#nullable enable
using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Müşteri portföyü - Sahip olunan hisseler
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class CustomerPortfolio : BaseEntity
    {
        private int _customerId;
        private string _stockSymbol = string.Empty;
        private decimal _quantity;
        private decimal _averageCost;
        private DateTime _purchaseDate = DateTime.Now;

        /// <summary>Hangi müşterinin</summary>
        public int CustomerId { get => _customerId; set => _customerId = value; }

        /// <summary>Hisse sembolü (THYAO gibi)</summary>
        public string StockSymbol { get => _stockSymbol; set => _stockSymbol = value; }

        /// <summary>Sahip olunan adet</summary>
        public decimal Quantity { get => _quantity; set => _quantity = value; }

        /// <summary>Ortalama alış maliyeti</summary>
        public decimal AverageCost { get => _averageCost; set => _averageCost = value; }

        /// <summary>İlk alım tarihi</summary>
        public DateTime PurchaseDate { get => _purchaseDate; set => _purchaseDate = value; }

        /// <summary>Toplam yatırım tutarı</summary>
        public decimal TotalInvestment => Quantity * AverageCost;

        // Navigation property
        public virtual Customer? Customer { get; set; }
    }
}
