#nullable enable
using BankApp.Core.Entities;
using BankApp.Core.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    /// <summary>
    /// Hesap repository sınıfı - Veritabanı işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class AccountRepository : IGenericRepository<Account>, IAccountRepository
    {
        private readonly DapperContext _context;

        /// <summary>
        /// AccountRepository yapıcı metodu
        /// </summary>
        /// <param name="context">Veritabanı bağlantı context'i</param>
        public AccountRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm hesapları getirir
        /// </summary>
        /// <returns>Hesap listesi</returns>
        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Accounts\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<Account>(query);
            }
        }

        /// <summary>
        /// ID'ye göre hesap getirir
        /// </summary>
        /// <param name="id">Hesap ID</param>
        /// <returns>Hesap veya null</returns>
        public async Task<Account?> GetByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Accounts\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Account>(query, new { Id = id });
            }
        }

        /// <summary>
        /// Yeni hesap ekler
        /// </summary>
        /// <param name="entity">Hesap nesnesi</param>
        /// <returns>Eklenen hesabın ID'si</returns>
        public async Task<int> AddAsync(Account entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "INSERT INTO \"Accounts\" (\"CustomerId\", \"AccountNumber\", \"IBAN\", \"Balance\", \"CurrencyCode\", \"OpenedDate\", \"CreatedAt\") VALUES (@CustomerId, @AccountNumber, @IBAN, @Balance, @CurrencyCode, @OpenedDate, @CreatedAt) RETURNING \"Id\"";
                if (entity.OpenedDate == default) entity.OpenedDate = DateTime.UtcNow;
                if (entity.CreatedAt == default) entity.CreatedAt = DateTime.UtcNow;
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        /// <summary>
        /// Hesap siler
        /// </summary>
        /// <param name="id">Silinecek hesap ID</param>
        /// <returns>İşlem başarılı mı</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "DELETE FROM \"Accounts\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }

        /// <summary>
        /// Hesap bilgilerini günceller
        /// </summary>
        /// <param name="entity">Güncellenecek hesap</param>
        /// <returns>İşlem başarılı mı</returns>
        public async Task<bool> UpdateAsync(Account entity)
        {
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

        /// <summary>
        /// Müşteri ID'sine göre hesapları getirir
        /// </summary>
        /// <param name="customerId">Müşteri ID</param>
        /// <returns>Hesap listesi</returns>
        public async Task<IEnumerable<Account>> GetByCustomerIdAsync(int customerId)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Accounts\" WHERE \"CustomerId\" = @CustomerId ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<Account>(query, new { CustomerId = customerId });
            }
        }

        /// <summary>
        /// IBAN'a göre hesap getirir
        /// </summary>
        /// <param name="iban">IBAN numarası</param>
        /// <returns>Hesap veya null</returns>
        public async Task<Account?> GetByIBANAsync(string iban)
        {
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
