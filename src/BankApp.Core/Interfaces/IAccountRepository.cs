using BankApp.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Core.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<IEnumerable<Account>> GetByCustomerIdAsync(int customerId);
        Task<Account> GetByIBANAsync(string iban);
    }
}
