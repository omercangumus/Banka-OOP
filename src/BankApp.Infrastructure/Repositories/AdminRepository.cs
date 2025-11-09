using System.Data;
using Npgsql;
using BankApp.Core.Entities;

namespace BankApp.Infrastructure.Repositories
{
    public interface IAdminRepository
    {
        Task<AdminStats> GetDashboardStatsAsync();
        Task<List<User>> GetUsersAsync(string search = "", string status = "");
        Task SetUserBanStatusAsync(int userId, bool isBanned);
        Task<List<Loan>> GetPendingLoansAsync();
        Task ApproveLoanAsync(int loanId, int adminId, string? note = null);
        Task RejectLoanAsync(int loanId, int adminId, string reason);
    }

    public class AdminStats
    {
        public int TotalUsers { get; set; }
        public decimal TotalDeposits { get; set; }
        public int ActiveLoans { get; set; }
        public int BannedUsers { get; set; }
    }

    public class AdminRepository : IAdminRepository
    {
        private readonly string _connectionString;

        public AdminRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<AdminStats> GetDashboardStatsAsync()
        {
            var stats = new AdminStats();
            
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Total Users
                try
                {
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", connection))
                    {
                        stats.TotalUsers = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
                catch { stats.TotalUsers = 0; }

                // Total Deposits
                try
                {
                    using (var cmd = new NpgsqlCommand("SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\"", connection))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        stats.TotalDeposits = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }
                }
                catch { stats.TotalDeposits = 0; }

                // Active Loans - with quoted column name
                try
                {
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Loans\" WHERE \"Status\" = 'Approved'", connection))
                    {
                        stats.ActiveLoans = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
                catch { stats.ActiveLoans = 0; }

                // Banned Users
                try
                {
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\" WHERE \"IsActive\" = false", connection))
                    {
                        stats.BannedUsers = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
                catch { stats.BannedUsers = 0; }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDashboardStatsAsync Error: {ex.Message}");
            }

            return stats;
        }

        public async Task<List<User>> GetUsersAsync(string search = "", string status = "")
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT \"Id\", \"Username\", \"Email\", \"Role\", \"FullName\", \"IsActive\", \"CreatedAt\" FROM \"Users\"";
            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrEmpty(search))
            {
                conditions.Add("(\"Username\" ILIKE @search OR \"Email\" ILIKE @search OR \"FullName\" ILIKE @search)");
                parameters.Add(new NpgsqlParameter("@search", $"%{search}%"));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Banned")
                {
                    try
                    {
                        conditions.Add("\"IsBanned\" = true");
                    }
                    catch
                    {
                        conditions.Add("\"IsActive\" = false");
                    }
                }
                else if (status == "Active")
                {
                    try
                    {
                        conditions.Add("\"IsBanned\" = false AND \"IsActive\" = true");
                    }
                    catch
                    {
                        conditions.Add("\"IsActive\" = true");
                    }
                }
                else if (status == "Admin")
                {
                    conditions.Add("\"Role\" = 'Admin'");
                }
                else if (status == "Customer")
                {
                    conditions.Add("\"Role\" = 'Customer'");
                }
            }

            if (conditions.Any())
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
            }

            sql += " ORDER BY \"CreatedAt\" DESC";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            var users = new List<User>();

            while (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32("Id"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    Role = reader.GetString("Role"),
                    FullName = reader.IsDBNull("FullName") ? null : reader.GetString("FullName"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
                users.Add(user);
            }

            return users;
        }

        public async Task SetUserBanStatusAsync(int userId, bool isBanned)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // Try with IsBanned column first
                var sql = "UPDATE \"Users\" SET \"IsBanned\" = @isBanned, \"IsActive\" = @isActive WHERE \"Id\" = @userId";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@isBanned", isBanned);
                cmd.Parameters.AddWithValue("@isActive", !isBanned);
                cmd.Parameters.AddWithValue("@userId", userId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // Fallback: Only use IsActive column
                var sql = "UPDATE \"Users\" SET \"IsActive\" = @isActive WHERE \"Id\" = @userId";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@isActive", !isBanned);
                cmd.Parameters.AddWithValue("@userId", userId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Loan>> GetPendingLoansAsync()
        {
            var loans = new List<Loan>();
            
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // First check if Loans table exists
                var sql = @"SELECT l.""Id"", l.""UserId"", l.""Amount"", l.""TermMonths"", l.""InterestRate"", 
                            l.""Status"", l.""ApplicationDate"" 
                            FROM ""Loans"" l 
                            WHERE l.""Status"" = 'Pending' 
                            ORDER BY l.""ApplicationDate"" DESC";

                using var cmd = new NpgsqlCommand(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var loan = new Loan
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        TermMonths = reader.GetInt32(reader.GetOrdinal("TermMonths")),
                        InterestRate = reader.GetDecimal(reader.GetOrdinal("InterestRate")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        ApplicationDate = reader.GetDateTime(reader.GetOrdinal("ApplicationDate"))
                    };
                    loans.Add(loan);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPendingLoansAsync Error: {ex.Message}");
                // Return empty list if query fails (table may not exist)
            }

            return loans;
        }

        public async Task ApproveLoanAsync(int loanId, int adminId, string? note = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "UPDATE \"Loans\" SET \"Status\" = 'Approved', \"DecisionDate\" = @decisionDate, \"ApprovedById\" = @adminId, \"Notes\" = @note WHERE \"Id\" = @loanId";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@decisionDate", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@adminId", adminId);
            cmd.Parameters.AddWithValue("@note", (object?)note ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@loanId", loanId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RejectLoanAsync(int loanId, int adminId, string reason)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "UPDATE \"Loans\" SET \"Status\" = 'Rejected', \"DecisionDate\" = @decisionDate, \"ApprovedById\" = @adminId, \"RejectionReason\" = @reason WHERE \"Id\" = @loanId";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@decisionDate", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@adminId", adminId);
            cmd.Parameters.AddWithValue("@reason", reason);
            cmd.Parameters.AddWithValue("@loanId", loanId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
