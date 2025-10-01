using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
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
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Customers\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<Customer>(query);
            }
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Customers\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Customer>(query, new { Id = id });
            }
        }

        public async Task<Customer> GetByIdentityAsync(string identityNumber)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Customers\" WHERE \"IdentityNumber\" = @IdentityNumber";
                return await connection.QuerySingleOrDefaultAsync<Customer>(query, new { IdentityNumber = identityNumber });
            }
        }

        public async Task<int> AddAsync(Customer entity)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "INSERT INTO \"Customers\" (\"UserId\", \"IdentityNumber\", \"FirstName\", \"LastName\", \"PhoneNumber\", \"Email\", \"Address\", \"CustomerType\", \"DateOfBirth\") VALUES (@UserId, @IdentityNumber, @FirstName, @LastName, @PhoneNumber, @Email, @Address, @CustomerType, @DateOfBirth) RETURNING \"Id\"";
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public async Task<bool> UpdateAsync(Customer entity)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "UPDATE \"Customers\" SET \"FirstName\" = @FirstName, \"LastName\" = @LastName, \"PhoneNumber\" = @PhoneNumber, \"Email\" = @Email, \"Address\" = @Address WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, entity) > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "DELETE FROM \"Customers\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }
    }
}
