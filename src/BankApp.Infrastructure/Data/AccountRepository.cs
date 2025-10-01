using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    public class AccountRepository : IGenericRepository<Account>, IAccountRepository
    {
        private readonly DapperContext _context;

        public AccountRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Accounts\"";
                return await connection.QueryAsync<Account>(query);
            }
        }

        public async Task<Account> GetByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Accounts\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Account>(query, new { Id = id });
            }
        }

        public async Task<int> AddAsync(Account entity)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "INSERT INTO \"Accounts\" (\"CustomerId\", \"AccountNumber\", \"IBAN\", \"Balance\", \"CurrencyCode\", \"OpenedDate\") VALUES (@CustomerId, @AccountNumber, @IBAN, @Balance, @CurrencyCode, @OpenedDate) RETURNING \"Id\"";
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "DELETE FROM \"Accounts\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }

        public async Task<bool> UpdateAsync(Account entity)
        {
             using (var connection = _context.CreateConnection())
            {
                var query = "UPDATE \"Accounts\" SET \"Balance\" = @Balance WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, entity) > 0;
            }
        }

        public async Task<IEnumerable<Account>> GetByCustomerIdAsync(int customerId)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId";
                return await connection.QueryAsync<Account>(query, new { CustomerId = customerId });
            }
        }

        public async Task<Account> GetByIBANAsync(string iban)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Accounts\" WHERE \"IBAN\" = @IBAN";
                return await connection.QuerySingleOrDefaultAsync<Account>(query, new { IBAN = iban });
            }
        }
    }
}
