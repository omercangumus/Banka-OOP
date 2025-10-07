using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    /// <summary>
    /// Denetim logu repository sınıfı - Veritabanı işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class AuditRepository : IAuditRepository
    {
        private readonly DapperContext _context;

        /// <summary>
        /// AuditRepository yapıcı metodu
        /// </summary>
        /// <param name="context">Veritabanı bağlantı context'i</param>
        public AuditRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Yeni denetim logu ekler
        /// </summary>
        /// <param name="log">Denetim log nesnesi</param>
        public async Task AddLogAsync(AuditLog log)
        {
            if (log.CreatedAt == default)
            {
                log.CreatedAt = DateTime.UtcNow;
            }
            
            if (string.IsNullOrEmpty(log.Action))
            {
                log.Action = "Unknown";
            }
            
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

        /// <summary>
        /// Tüm denetim loglarını getirir
        /// </summary>
        /// <returns>Denetim log listesi (Son 1000)</returns>
        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"AuditLogs\" ORDER BY \"CreatedAt\" DESC LIMIT 1000";
                return await connection.QueryAsync<AuditLog>(query);
            }
        }
    }
}
