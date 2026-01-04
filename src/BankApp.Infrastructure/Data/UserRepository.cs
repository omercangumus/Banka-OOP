#nullable enable
using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    /// <summary>
    /// Kullanıcı repository sınıfı - Veritabanı işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class UserRepository : IGenericRepository<User>, IUserRepository
    {
        private readonly DapperContext _context;

        /// <summary>
        /// UserRepository yapıcı metodu
        /// </summary>
        /// <param name="context">Veritabanı bağlantı context'i</param>
        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm kullanıcıları getirir
        /// </summary>
        /// <returns>Kullanıcı listesi</returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Users\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<User>(query);
            }
        }

        /// <summary>
        /// ID'ye göre kullanıcı getirir
        /// </summary>
        /// <param name="id">Kullanıcı ID</param>
        /// <returns>Kullanıcı veya null</returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Users\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Id = id });
            }
        }

        /// <summary>
        /// Yeni kullanıcı ekler
        /// </summary>
        /// <param name="entity">Kullanıcı nesnesi</param>
        /// <returns>Eklenen kullanıcının ID'si</returns>
        public async Task<int> AddAsync(User entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

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

        /// <summary>
        /// Kullanıcı siler
        /// </summary>
        /// <param name="id">Silinecek kullanıcı ID</param>
        /// <returns>İşlem başarılı mı</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "DELETE FROM \"Users\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }

        /// <summary>
        /// Kullanıcı bilgilerini günceller
        /// </summary>
        /// <param name="entity">Güncellenecek kullanıcı</param>
        /// <returns>İşlem başarılı mı</returns>
        public async Task<bool> UpdateAsync(User entity)
        {
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

        /// <summary>
        /// Kullanıcı adına göre kullanıcı getirir
        /// </summary>
        /// <param name="username">Kullanıcı adı</param>
        /// <returns>Kullanıcı veya null</returns>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    connection.Open();
                    var query = "SELECT * FROM \"Users\" WHERE \"Username\" = @Username";
                    return await connection.QueryFirstOrDefaultAsync<User>(query, new { Username = username });
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                // SORUN DÜZELTİLDİ: Veritabanı bağlantı hatasını yeniden fırlat
                System.Diagnostics.Debug.WriteLine($"GetByUsernameAsync DB Hatası: {ex.Message}");
                throw; // Hata üst seviyede yakalanacak
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByUsernameAsync Genel Hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// E-posta adresine göre kullanıcı getirir
        /// </summary>
        /// <param name="email">E-posta adresi</param>
        /// <returns>Kullanıcı veya null</returns>
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
