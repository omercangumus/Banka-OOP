#nullable enable
using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DapperContext _context;

        public CustomerRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Customers\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<Customer>(query);
            }
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Customers\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Customer>(query, new { Id = id });
            }
        }

        public async Task<Customer?> GetByIdentityAsync(string identityNumber)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return null;
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Customers\" WHERE \"IdentityNumber\" = @IdentityNumber";
                return await connection.QuerySingleOrDefaultAsync<Customer>(query, new { IdentityNumber = identityNumber });
            }
        }

        public async Task<int> AddAsync(Customer entity)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Ensure CreatedAt has a value
            if (entity.CreatedAt == default)
            {
                entity.CreatedAt = System.DateTime.UtcNow;
            }
            
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "INSERT INTO \"Customers\" (\"UserId\", \"IdentityNumber\", \"FirstName\", \"LastName\", \"PhoneNumber\", \"Email\", \"Address\", \"DateOfBirth\", \"CreatedAt\") VALUES (@UserId, @IdentityNumber, @FirstName, @LastName, @PhoneNumber, @Email, @Address, @DateOfBirth, @CreatedAt) RETURNING \"Id\"";
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public async Task<bool> UpdateAsync(Customer entity)
        {
            // SORUN DÜZELTİLDİ: Connection açma ve DateOfBirth kolonu eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "UPDATE \"Customers\" SET \"FirstName\" = @FirstName, \"LastName\" = @LastName, \"PhoneNumber\" = @PhoneNumber, \"Email\" = @Email, \"Address\" = @Address, \"DateOfBirth\" = @DateOfBirth WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, entity) > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "DELETE FROM \"Customers\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }
    }
}
