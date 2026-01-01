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
    /// Müşteri repository sınıfı - Veritabanı işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DapperContext _context;

        /// <summary>
        /// CustomerRepository yapıcı metodu
        /// </summary>
        /// <param name="context">Veritabanı bağlantı context'i</param>
        public CustomerRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm müşterileri getirir
        /// </summary>
        /// <returns>Müşteri listesi</returns>
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Customers\" ORDER BY \"CreatedAt\" DESC";
                return await connection.QueryAsync<Customer>(query);
            }
        }

        /// <summary>
        /// ID'ye göre müşteri getirir
        /// </summary>
        /// <param name="id">Müşteri ID</param>
        /// <returns>Müşteri veya null</returns>
        public async Task<Customer?> GetByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Customers\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Customer>(query, new { Id = id });
            }
        }

        /// <summary>
        /// TC Kimlik numarasına göre müşteri getirir
        /// </summary>
        /// <param name="identityNumber">TC Kimlik No</param>
        /// <returns>Müşteri veya null</returns>
        public async Task<Customer?> GetByIdentityAsync(string identityNumber)
        {
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

        /// <summary>
        /// Yeni müşteri ekler
        /// </summary>
        /// <param name="entity">Müşteri nesnesi</param>
        /// <returns>Eklenen müşterinin ID'si</returns>
        public async Task<int> AddAsync(Customer entity)
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
                var query = "INSERT INTO \"Customers\" (\"UserId\", \"IdentityNumber\", \"FirstName\", \"LastName\", \"PhoneNumber\", \"Email\", \"Address\", \"DateOfBirth\", \"CreatedAt\") VALUES (@UserId, @IdentityNumber, @FirstName, @LastName, @PhoneNumber, @Email, @Address, @DateOfBirth, @CreatedAt) RETURNING \"Id\"";
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        /// <summary>
        /// Müşteri bilgilerini günceller
        /// </summary>
        /// <param name="entity">Güncellenecek müşteri</param>
        /// <returns>İşlem başarılı mı</returns>
        public async Task<bool> UpdateAsync(Customer entity)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "UPDATE \"Customers\" SET \"FirstName\" = @FirstName, \"LastName\" = @LastName, \"PhoneNumber\" = @PhoneNumber, \"Email\" = @Email, \"Address\" = @Address, \"DateOfBirth\" = @DateOfBirth WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, entity) > 0;
            }
        }

        /// <summary>
        /// Müşteri siler
        /// </summary>
        /// <param name="id">Silinecek müşteri ID</param>
        /// <returns>İşlem başarılı mı</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "DELETE FROM \"Customers\" WHERE \"Id\" = @Id";
                return await connection.ExecuteAsync(query, new { Id = id }) > 0;
            }
        }
    }
}
