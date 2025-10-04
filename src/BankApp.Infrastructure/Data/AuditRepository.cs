using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System;
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
            // Ensure CreatedAt has a value
            if (log.CreatedAt == default)
            {
                log.CreatedAt = DateTime.UtcNow;
            }
            
            // Ensure Action is not null
            if (string.IsNullOrEmpty(log.Action))
            {
                log.Action = "Unknown";
            }
            
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "INSERT INTO \"AuditLogs\" (\"UserId\", \"Action\", \"Details\", \"IpAddress\", \"CreatedAt\") VALUES (@UserId, @Action, @Details, @IpAddress, @CreatedAt)";
                await connection.ExecuteAsync(query, new 
                {
                    log.UserId,
                    log.Action,
                    Details = log.Details ?? "",
                    IpAddress = log.IpAddress ?? "127.0.0.1",
                    log.CreatedAt
                });
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"AuditLogs\" ORDER BY \"CreatedAt\" DESC LIMIT 1000";
                return await connection.QueryAsync<AuditLog>(query);
            }
        }
    }
}

