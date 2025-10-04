#nullable enable
using BankApp.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Core.Interfaces
{
    public interface IAuditRepository
    {
        Task AddLogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetAllLogsAsync();
    }
}
