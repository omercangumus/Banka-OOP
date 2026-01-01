using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Entities;

namespace BankApp.Core.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId);
    }
}
