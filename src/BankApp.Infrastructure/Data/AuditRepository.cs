using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    public class AuditRepository : IAuditRepository
    {
        private readonly DapperContext _context;

        public AuditRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(AuditLog log)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "INSERT INTO \"AuditLogs\" (\"UserId\", \"Action\", \"Details\", \"IpAddress\", \"CreatedAt\") VALUES (@UserId, @Action, @Details, @IpAddress, @CreatedAt)";
                await connection.ExecuteAsync(query, log);
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"AuditLogs\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<AuditLog>(query);
            }
        }
    }
}
