using System;

namespace BankApp.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// Dashboard özet verileri için DTO
    /// </summary>
    public class DashboardSummaryDto
    {
        /// <summary>Toplam Net Varlık (Hesaplar + Varlıklar - Krediler)</summary>
        public decimal NetWorth { get; set; }
        
        /// <summary>Toplam Kredi Borcu (Aktif kredilerin kalan borcu)</summary>
        public decimal TotalDebt { get; set; }
        
        /// <summary>Toplam Hesap Bakiyesi</summary>
        public decimal TotalBalance { get; set; }
        
        /// <summary>Toplam Varlık Değeri</summary>
        public decimal TotalAssets { get; set; }
        
        /// <summary>Aylık Harcama (son 30 gün)</summary>
        public decimal MonthlySpending { get; set; }
        
        /// <summary>Aylık Gelir (son 30 gün)</summary>
        public decimal MonthlyIncome { get; set; }
        
        /// <summary>Aktif Kredi Sayısı</summary>
        public int ActiveLoanCount { get; set; }
        
        /// <summary>Toplam İşlem Sayısı</summary>
        public int TotalTransactionCount { get; set; }
    }
}
