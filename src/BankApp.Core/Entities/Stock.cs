using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Hisse senedi entity - Borsa simülasyonu için
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class Stock : BaseEntity
    {
        private string _symbol = string.Empty;
        private string _name = string.Empty;
        private decimal _currentPrice;
        private decimal _previousPrice;
        private decimal _volatility = 1.0m;
        private decimal _marketCap;
        private string _sector = string.Empty;

        /// <summary>Hisse sembolü (THYAO, GARAN, AKBNK gibi)</summary>
        public string Symbol { get => _symbol; set => _symbol = value; }

        /// <summary>Hisse adı (Türk Hava Yolları gibi)</summary>
        public string Name { get => _name; set => _name = value; }

        /// <summary>Güncel fiyat (TL)</summary>
        public decimal CurrentPrice { get => _currentPrice; set => _currentPrice = value; }

        /// <summary>Önceki fiyat - Değişim hesabı için</summary>
        public decimal PreviousPrice { get => _previousPrice; set => _previousPrice = value; }

        /// <summary>Oynaklık oranı (0.5 - 5.0 arası)</summary>
        public decimal Volatility { get => _volatility; set => _volatility = value; }

        /// <summary>Piyasa değeri (simüle)</summary>
        public decimal MarketCap { get => _marketCap; set => _marketCap = value; }

        /// <summary>Sektör</summary>
        public string Sector { get => _sector; set => _sector = value; }

        /// <summary>Güncel değişim yüzdesi</summary>
        public decimal ChangePercent => PreviousPrice > 0 
            ? ((CurrentPrice - PreviousPrice) / PreviousPrice) * 100 
            : 0;
    }
}
