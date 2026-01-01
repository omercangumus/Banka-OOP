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
    /// İşlem repository sınıfı - Veritabanı işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class TransactionRepository : IGenericRepository<Transaction>, ITransactionRepository
    {
        private readonly DapperContext _context;

        /// <summary>
        /// TransactionRepository yapıcı metodu
        /// </summary>
        /// <param name="context">Veritabanı bağlantı context'i</param>
        public TransactionRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm işlemleri getirir
        /// </summary>
        /// <returns>İşlem listesi</returns>
        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Transactions\" ORDER BY \"TransactionDate\" DESC";
                return await connection.QueryAsync<Transaction>(query);
            }
        }

        /// <summary>
        /// ID'ye göre işlem getirir
        /// </summary>
        /// <param name="id">İşlem ID</param>
        /// <returns>İşlem veya null</returns>
        public async Task<Transaction?> GetByIdAsync(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "SELECT * FROM \"Transactions\" WHERE \"Id\" = @Id";
                return await connection.QuerySingleOrDefaultAsync<Transaction>(query, new { Id = id });
            }
        }

        /// <summary>
        /// Yeni işlem ekler
        /// </summary>
        /// <param name="entity">İşlem nesnesi</param>
        /// <returns>Eklenen işlemin ID'si</returns>
        public async Task<int> AddAsync(Transaction entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                var query = "INSERT INTO \"Transactions\" (\"AccountId\", \"TransactionType\", \"Amount\", \"Description\", \"TransactionDate\", \"CreatedAt\") VALUES (@AccountId, @TransactionType, @Amount, @Description, @TransactionDate, @CreatedAt) RETURNING \"Id\"";
                if (entity.TransactionDate == default) entity.TransactionDate = DateTime.UtcNow;
                if (entity.CreatedAt == default) entity.CreatedAt = DateTime.UtcNow;
                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
        }

        /// <summary>
        /// İşlem silme - Desteklenmiyor
        /// </summary>
        /// <param name="id">İşlem ID</param>
        /// <returns>NotImplementedException fırlatır</returns>
        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException("İşlemler silinemez.");
        }

        /// <summary>
        /// İşlem güncelleme - Desteklenmiyor
        /// </summary>
        /// <param name="entity">İşlem nesnesi</param>
        /// <returns>NotImplementedException fırlatır</returns>
        public Task<bool> UpdateAsync(Transaction entity)
        {
            throw new NotImplementedException("İşlemler güncellenemez.");
        }
    }
}
