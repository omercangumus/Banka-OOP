using BankApp.Core.Entities;

namespace BankApp.Core.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        // Custom transaction specific methods if needed
    }
}
