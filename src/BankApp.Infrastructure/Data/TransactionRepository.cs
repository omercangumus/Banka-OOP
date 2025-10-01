using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    public class TransactionRepository : IGenericRepository<Transaction>, ITransactionRepository
    {
        private readonly DapperContext _context;

        public TransactionRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Transactions\" ORDER BY \"TransactionDate\" DESC";
                return await connection.QueryAsync<Transaction>(query);
            }
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
             using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Transactions\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Transaction>(query, new { Id = id });
            }
        }

        public async Task<int> AddAsync(Transaction entity)
        {
             using (var connection = _context.CreateConnection())
            {
                var query = "INSERT INTO \"Transactions\" (\"AccountId\", \"TransactionType\", \"Amount\", \"Description\", \"TransactionDate\") VALUES (@AccountId, @TransactionType, @Amount, @Description, @TransactionDate) RETURNING \"Id\"";
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new System.NotImplementedException("Transactions cannot be deleted.");
        }

        public Task<bool> UpdateAsync(Transaction entity)
        {
            throw new System.NotImplementedException("Transactions cannot be updated.");
        }
    }
}
