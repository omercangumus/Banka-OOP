#nullable enable
using BankApp.Core.Entities;

namespace BankApp.Core.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        // Custom customer methods if any
    }
}
