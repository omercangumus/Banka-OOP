#nullable enable
using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System;
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
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Accounts\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<Account>(query);
            }
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Accounts\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Account>(query, new { Id = id });
            }
        }

        public async Task<int> AddAsync(Account entity)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "INSERT INTO \"Accounts\" (\"CustomerId\", \"AccountNumber\", \"IBAN\", \"Balance\", \"CurrencyCode\", \"OpenedDate\", \"CreatedAt\") VALUES (@CustomerId, @AccountNumber, @IBAN, @Balance, @CurrencyCode, @OpenedDate, @CreatedAt) RETURNING \"Id\"";
                if (entity.OpenedDate == default) entity.OpenedDate = System.DateTime.UtcNow;
                if (entity.CreatedAt == default) entity.CreatedAt = System.DateTime.UtcNow;
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "DELETE FROM \"Accounts\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }

        public async Task<bool> UpdateAsync(Account entity)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "UPDATE \"Accounts\" SET \"Balance\" = @Balance WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, entity) > 0;
            }
        }

        public async Task<IEnumerable<Account>> GetByCustomerIdAsync(int customerId)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<Account>(query, new { CustomerId = customerId });
            }
        }

        public async Task<Account?> GetByIBANAsync(string iban)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (string.IsNullOrWhiteSpace(iban))
            {
                return null;
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Accounts\" WHERE \"IBAN\" = @IBAN";
                return await connection.QuerySingleOrDefaultAsync<Account>(query, new { IBAN = iban });
            }
        }
    }
}
