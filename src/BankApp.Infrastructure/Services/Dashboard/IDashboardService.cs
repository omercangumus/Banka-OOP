using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// Dashboard veri servisi interface
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Kullanıcının dashboard özet verilerini getirir
        /// </summary>
        Task<DashboardSummaryDto> GetSummaryAsync(int userId);
        
        /// <summary>
        /// Son işlemleri getirir
        /// </summary>
        Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int userId, int take = 10);
        
        /// <summary>
        /// Harcama dağılımı (işlem tipine göre)
        /// </summary>
        Task<List<PieSliceDto>> GetSpendingDistributionAsync(int userId, DateTime? from = null, DateTime? to = null);
        
        /// <summary>
        /// Varlık dağılımı (hesap tipine göre)
        /// </summary>
        Task<List<PieSliceDto>> GetAssetDistributionAsync(int userId);
    }
}
