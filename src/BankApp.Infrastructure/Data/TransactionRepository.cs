#nullable enable
using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System;
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
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Transactions\" ORDER BY \"TransactionDate\" DESC";
                return await connection.QueryAsync<Transaction>(query);
            }
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Transactions\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Transaction>(query, new { Id = id });
            }
        }

        public async Task<int> AddAsync(Transaction entity)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "INSERT INTO \"Transactions\" (\"AccountId\", \"TransactionType\", \"Amount\", \"Description\", \"TransactionDate\", \"CreatedAt\") VALUES (@AccountId, @TransactionType, @Amount, @Description, @TransactionDate, @CreatedAt) RETURNING \"Id\"";
                if (entity.TransactionDate == default) entity.TransactionDate = System.DateTime.UtcNow;
                if (entity.CreatedAt == default) entity.CreatedAt = System.DateTime.UtcNow;
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
