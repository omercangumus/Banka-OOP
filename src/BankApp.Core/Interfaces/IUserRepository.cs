#nullable enable
using BankApp.Core.Entities;
using System.Threading.Tasks;

namespace BankApp.Core.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
    }
}
