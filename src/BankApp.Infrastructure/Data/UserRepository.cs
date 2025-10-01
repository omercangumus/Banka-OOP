using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    // Simple Repository Implementation for User
    public class UserRepository : IGenericRepository<User>, IUserRepository
    {
        private readonly DapperContext _context;

        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Users\"";
                return await connection.QueryAsync<User>(query);
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Users\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Id = id });
            }
        }

        public async Task<int> AddAsync(User entity)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "INSERT INTO \"Users\" (\"Username\", \"PasswordHash\", \"Email\", \"Role\", \"FullName\", \"IsActive\") VALUES (@Username, @PasswordHash, @Email, @Role, @FullName, @IsActive) RETURNING \"Id\"";
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "DELETE FROM \"Users\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "UPDATE \"Users\" SET \"Username\" = @Username, \"PasswordHash\" = @PasswordHash, \"Email\" = @Email, \"Role\" = @Role, \"FullName\" = @FullName, \"IsActive\" = @IsActive WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, entity) > 0;
            }
        }

        // Custom method for Login
        public async Task<User> GetByUsernameAsync(string username)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Users\" WHERE \"Username\" = @Username";
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
            }
        }

        // Custom method for Forgot Password
        public async Task<User> GetByEmailAsync(string email)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = "SELECT * FROM \"Users\" WHERE \"Email\" = @Email";
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
            }
        }
    }
}
