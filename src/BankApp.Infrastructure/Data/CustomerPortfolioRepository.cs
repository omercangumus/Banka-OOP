#nullable enable
using BankApp.Core.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Data
{
    /// <summary>
    /// Müşteri portföyü repository - Yatırım pozisyonları CRUD işlemleri
    /// </summary>
    public class CustomerPortfolioRepository
    {
        private readonly DapperContext _context;

        public CustomerPortfolioRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Müşterinin tüm pozisyonlarını getirir
        /// </summary>
        public async Task<IEnumerable<CustomerPortfolio>> GetByCustomerIdAsync(int customerId)
        {
            using var conn = _context.CreateConnection();
            return await conn.QueryAsync<CustomerPortfolio>(
                @"SELECT * FROM ""CustomerPortfolios"" WHERE ""CustomerId"" = @CustomerId ORDER BY ""PurchaseDate"" DESC",
                new { CustomerId = customerId });
        }

        /// <summary>
        /// Belirli bir sembol için müşterinin pozisyonunu getirir
        /// </summary>
        public async Task<CustomerPortfolio?> GetBySymbolAsync(int customerId, string symbol)
        {
            using var conn = _context.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<CustomerPortfolio>(
                @"SELECT * FROM ""CustomerPortfolios"" WHERE ""CustomerId"" = @CustomerId AND ""StockSymbol"" = @Symbol",
                new { CustomerId = customerId, Symbol = symbol });
        }

        /// <summary>
        /// Yeni pozisyon ekler
        /// </summary>
        public async Task<int> AddAsync(CustomerPortfolio portfolio)
        {
            using var conn = _context.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO ""CustomerPortfolios"" (""CustomerId"", ""StockSymbol"", ""Quantity"", ""AverageCost"", ""PurchaseDate"", ""CreatedAt"")
                  VALUES (@CustomerId, @StockSymbol, @Quantity, @AverageCost, @PurchaseDate, NOW())
                  RETURNING ""Id""",
                portfolio);
        }

        /// <summary>
        /// Pozisyonu günceller (miktar ve ortalama maliyet)
        /// </summary>
        public async Task<bool> UpdateAsync(CustomerPortfolio portfolio)
        {
            using var conn = _context.CreateConnection();
            var affected = await conn.ExecuteAsync(
                @"UPDATE ""CustomerPortfolios"" 
                  SET ""Quantity"" = @Quantity, ""AverageCost"" = @AverageCost 
                  WHERE ""Id"" = @Id",
                portfolio);
            return affected > 0;
        }

        /// <summary>
        /// Pozisyonu siler (miktar 0 olduğunda)
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _context.CreateConnection();
            var affected = await conn.ExecuteAsync(
                @"DELETE FROM ""CustomerPortfolios"" WHERE ""Id"" = @Id",
                new { Id = id });
            return affected > 0;
        }

        /// <summary>
        /// AL işlemi: Pozisyon varsa güncelle, yoksa yeni ekle
        /// </summary>
        public async Task BuyAsync(int customerId, string symbol, decimal quantity, decimal price)
        {
            var existing = await GetBySymbolAsync(customerId, symbol);
            
            if (existing != null)
            {
                // Mevcut pozisyonu güncelle - ortalama maliyet hesapla
                decimal totalCost = (existing.Quantity * existing.AverageCost) + (quantity * price);
                decimal newQuantity = existing.Quantity + quantity;
                decimal newAvgCost = totalCost / newQuantity;
                
                existing.Quantity = newQuantity;
                existing.AverageCost = newAvgCost;
                await UpdateAsync(existing);
            }
            else
            {
                // Yeni pozisyon ekle
                var portfolio = new CustomerPortfolio
                {
                    CustomerId = customerId,
                    StockSymbol = symbol,
                    Quantity = quantity,
                    AverageCost = price,
                    PurchaseDate = DateTime.Now
                };
                await AddAsync(portfolio);
            }
        }

        /// <summary>
        /// SAT işlemi: Pozisyondan düş, 0 olursa sil
        /// </summary>
        public async Task<bool> SellAsync(int customerId, string symbol, decimal quantity)
        {
            var existing = await GetBySymbolAsync(customerId, symbol);
            
            if (existing == null || existing.Quantity < quantity)
            {
                return false; // Yetersiz pozisyon
            }
            
            existing.Quantity -= quantity;
            
            if (existing.Quantity <= 0)
            {
                await DeleteAsync(existing.Id);
            }
            else
            {
                await UpdateAsync(existing);
            }
            
            return true;
        }

        /// <summary>
        /// Müşterinin toplam portföy değerini hesaplar
        /// </summary>
        public async Task<decimal> GetTotalValueAsync(int customerId)
        {
            using var conn = _context.CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<decimal?>(
                @"SELECT COALESCE(SUM(""Quantity"" * ""AverageCost""), 0) 
                  FROM ""CustomerPortfolios"" 
                  WHERE ""CustomerId"" = @CustomerId",
                new { CustomerId = customerId });
            return result ?? 0;
        }
    }
}
