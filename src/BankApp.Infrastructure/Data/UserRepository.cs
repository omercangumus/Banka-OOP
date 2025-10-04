#nullable enable
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
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Users\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<User>(query);
            }
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Users\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Id = id });
            }
        }

        public async Task<int> AddAsync(User entity)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Ensure CreatedAt has a value
            if (entity.CreatedAt == default)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "INSERT INTO \"Users\" (\"Username\", \"PasswordHash\", \"Email\", \"Role\", \"FullName\", \"IsActive\", \"VerificationCode\", \"VerificationCodeExpiry\", \"IsVerified\", \"CreatedAt\") VALUES (@Username, @PasswordHash, @Email, @Role, @FullName, @IsActive, @VerificationCode, @VerificationCodeExpiry, @IsVerified, @CreatedAt) RETURNING \"Id\"";
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // SORUN DÜZELTİLDİ: Connection açma eklendi
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "DELETE FROM \"Users\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            // SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "UPDATE \"Users\" SET \"Username\" = @Username, \"PasswordHash\" = @PasswordHash, \"Email\" = @Email, \"Role\" = @Role, \"FullName\" = @FullName, \"IsActive\" = @IsActive, \"VerificationCode\" = @VerificationCode, \"VerificationCodeExpiry\" = @VerificationCodeExpiry, \"IsVerified\" = @IsVerified WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, entity) > 0;
            }
        }

        // Custom method for Login - SORUN DÜZELTİLDİ: Null kontrolü eklendi
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Users\" WHERE \"Username\" = @Username";
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
            }
        }

        // Custom method for Forgot Password - SORUN DÜZELTİLDİ: Null kontrolü ve connection açma eklendi
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Users\" WHERE \"Email\" = @Email";
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
            }
        }
    }
}
