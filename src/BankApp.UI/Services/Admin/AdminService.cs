using BankApp.Core.Entities;
using BankApp.Infrastructure.Repositories;
using System.IO;

namespace BankApp.UI.Services.Admin
{
    public class AdminService
    {
        private readonly IAdminRepository _repository;

        public AdminService()
        {
            var connectionString = LoadConnectionString();
            _repository = new AdminRepository(connectionString);
        }

        private string LoadConnectionString()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var jsonString = File.ReadAllText(configPath);
                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(jsonString);
                        if (doc.RootElement.TryGetProperty("ConnectionStrings", out var connStrings))
                        {
                            if (connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                            {
                                var connectionString = defaultConn.GetString();
                                if (!string.IsNullOrEmpty(connectionString))
                                {
                                    return connectionString;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection string okuma hatası: {ex.Message}");
            }

            throw new InvalidOperationException("Connection string not found: DefaultConnection");
        }

        public async Task<AdminStats> GetStatsAsync()
        {
            return await _repository.GetDashboardStatsAsync();
        }

        public async Task<List<User>> GetUsersAsync(string search = "", string status = "")
        {
            return await _repository.GetUsersAsync(search, status);
        }

        public async Task BanUserAsync(int userId)
        {
            await _repository.SetUserBanStatusAsync(userId, true);

            // PHASE 5: Log the ban action
            try
            {
                var auditService = new AdminAuditService(_repository);
                await auditService.LogUserBanAsync(1, userId, true); // Admin ID placeholder
            }
            catch
            {
                // Don't break the main flow if audit fails
            }
        }

        public async Task UnbanUserAsync(int userId)
        {
            await _repository.SetUserBanStatusAsync(userId, false);

            // PHASE 5: Log the unban action
            try
            {
                var auditService = new AdminAuditService(_repository);
                await auditService.LogUserBanAsync(1, userId, false); // Admin ID placeholder
            }
            catch
            {
                // Don't break the main flow if audit fails
            }
        }

        public async Task<List<Loan>> GetPendingLoansAsync()
        {
            return await _repository.GetPendingLoansAsync();
        }

        public async Task ApproveLoanAsync(int loanId, string? note = null)
        {
            // In a real app, get admin ID from session
            var adminId = 1; // Placeholder
            await _repository.ApproveLoanAsync(loanId, adminId, note);

            // PHASE 5: Log the approval action
            try
            {
                var auditService = new AdminAuditService(_repository);
                await auditService.LogLoanDecisionAsync(adminId, loanId, "Approved", note);
            }
            catch
            {
                // Don't break the main flow if audit fails
            }
        }

        public async Task RejectLoanAsync(int loanId, string reason)
        {
            // In a real app, get admin ID from session
            var adminId = 1; // Placeholder
            await _repository.RejectLoanAsync(loanId, adminId, reason);

            // PHASE 5: Log the rejection action
            try
            {
                var auditService = new AdminAuditService(_repository);
                await auditService.LogLoanDecisionAsync(adminId, loanId, "Rejected", reason);
            }
            catch
            {
                // Don't break the main flow if audit fails
            }
        }
    }
}
