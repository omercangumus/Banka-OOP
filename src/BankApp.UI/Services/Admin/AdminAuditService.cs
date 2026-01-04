using BankApp.Core.Entities;
using BankApp.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace BankApp.UI.Services.Admin
{
    /// <summary>
    /// Service for admin audit logging operations
    /// PHASE 5: Admin audit logging service
    /// </summary>
    public class AdminAuditService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminAuditService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        /// <summary>
        /// Log user ban action
        /// </summary>
        public async Task LogUserBanAsync(int adminUserId, int targetUserId, bool isBan)
        {
            try
            {
                // This would require extending IAdminRepository with audit methods
                // For now, we'll use the existing audit repository directly
                var dapperContext = new BankApp.Infrastructure.Data.DapperContext();
                var auditRepo = new BankApp.Infrastructure.Data.AuditRepository(dapperContext);

                await auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = adminUserId,
                    Action = isBan ? "UserBan" : "UserUnban",
                    Details = $"{(isBan ? "Banned" : "Unbanned")} user ID: {targetUserId}",
                    IpAddress = "127.0.0.1",
                    CreatedAt = System.DateTime.UtcNow
                });
            }
            catch
            {
                // Silently fail for audit logging - don't break main flow
            }
        }

        /// <summary>
        /// Log loan approval/rejection action
        /// </summary>
        public async Task LogLoanDecisionAsync(int adminUserId, int loanId, string decision, string? note = null)
        {
            try
            {
                var dapperContext = new BankApp.Infrastructure.Data.DapperContext();
                var auditRepo = new BankApp.Infrastructure.Data.AuditRepository(dapperContext);

                await auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = adminUserId,
                    Action = $"Loan{decision}",
                    Details = $"{decision} loan ID: {loanId}{(string.IsNullOrEmpty(note) ? "" : $" - Note: {note}")}",
                    IpAddress = "127.0.0.1",
                    CreatedAt = System.DateTime.UtcNow
                });
            }
            catch
            {
                // Silently fail for audit logging - don't break main flow
            }
        }
    }
}
