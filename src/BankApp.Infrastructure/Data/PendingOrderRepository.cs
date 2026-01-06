using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Dapper;
using BankApp.Core.Entities;

namespace BankApp.Infrastructure.Data
{
    /// <summary>
    /// Bekleyen Emirler Repository - Limit/Stop emirleri için
    /// </summary>
    public class PendingOrderRepository
    {
        private readonly DapperContext _context;

        public PendingOrderRepository(DapperContext context)
        {
            _context = context;
        }

        public PendingOrderRepository()
        {
            _context = new DapperContext();
        }

        /// <summary>
        /// Tablo yoksa oluştur
        /// </summary>
        public async Task EnsureTableExistsAsync()
        {
            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ""PendingOrders"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""CustomerId"" INTEGER NOT NULL,
                    ""AccountId"" INTEGER NOT NULL,
                    ""Symbol"" VARCHAR(20) NOT NULL,
                    ""OrderType"" VARCHAR(20) NOT NULL DEFAULT 'Limit',
                    ""Side"" VARCHAR(10) NOT NULL DEFAULT 'Buy',
                    ""Quantity"" DECIMAL(18,8) NOT NULL,
                    ""LimitPrice"" DECIMAL(18,8) NOT NULL,
                    ""StopPrice"" DECIMAL(18,8),
                    ""Status"" VARCHAR(20) NOT NULL DEFAULT 'Pending',
                    ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""FilledAt"" TIMESTAMP,
                    ""CancelledAt"" TIMESTAMP,
                    ""CancelReason"" VARCHAR(255)
                )");
            Debug.WriteLine("[DATA] PendingOrders table ensured");
        }

        /// <summary>
        /// Yeni bekleyen emir ekle
        /// </summary>
        public async Task<int> CreateAsync(PendingOrder order)
        {
            await EnsureTableExistsAsync();
            
            using var conn = _context.CreateConnection();
            var id = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO ""PendingOrders"" 
                (""CustomerId"", ""AccountId"", ""Symbol"", ""OrderType"", ""Side"", ""Quantity"", ""LimitPrice"", ""StopPrice"", ""Status"", ""CreatedAt"")
                VALUES (@CustomerId, @AccountId, @Symbol, @OrderType, @Side, @Quantity, @LimitPrice, @StopPrice, @Status, @CreatedAt)
                RETURNING ""Id""", order);
            
            Debug.WriteLine($"[DATA] PendingOrder created Id={id} Symbol={order.Symbol} Type={order.OrderType} Side={order.Side} Qty={order.Quantity} Price={order.LimitPrice}");
            return id;
        }

        /// <summary>
        /// Müşterinin bekleyen emirlerini getir
        /// </summary>
        public async Task<IEnumerable<PendingOrder>> GetPendingByCustomerIdAsync(int customerId)
        {
            await EnsureTableExistsAsync();
            
            using var conn = _context.CreateConnection();
            var orders = await conn.QueryAsync<PendingOrder>(@"
                SELECT * FROM ""PendingOrders"" 
                WHERE ""CustomerId"" = @CustomerId AND ""Status"" = 'Pending'
                ORDER BY ""CreatedAt"" DESC", new { CustomerId = customerId });
            
            Debug.WriteLine($"[DATA] PendingOrders.GetPending customerId={customerId} count={System.Linq.Enumerable.Count(orders)}");
            return orders;
        }

        /// <summary>
        /// Tüm bekleyen emirleri getir (müşteri bazlı değil)
        /// </summary>
        public async Task<IEnumerable<PendingOrder>> GetAllPendingAsync()
        {
            await EnsureTableExistsAsync();
            
            using var conn = _context.CreateConnection();
            return await conn.QueryAsync<PendingOrder>(@"
                SELECT * FROM ""PendingOrders"" 
                WHERE ""Status"" = 'Pending'
                ORDER BY ""CreatedAt"" DESC");
        }

        /// <summary>
        /// Emri iptal et
        /// </summary>
        public async Task<bool> CancelAsync(int orderId, string reason = "Kullanıcı tarafından iptal edildi")
        {
            using var conn = _context.CreateConnection();
            var affected = await conn.ExecuteAsync(@"
                UPDATE ""PendingOrders"" 
                SET ""Status"" = 'Cancelled', ""CancelledAt"" = @Now, ""CancelReason"" = @Reason
                WHERE ""Id"" = @Id AND ""Status"" = 'Pending'",
                new { Id = orderId, Now = DateTime.UtcNow, Reason = reason });
            
            Debug.WriteLine($"[DATA] PendingOrder.Cancel orderId={orderId} affected={affected} reason={reason}");
            return affected > 0;
        }

        /// <summary>
        /// Emri doldur (gerçekleştir)
        /// </summary>
        public async Task<bool> FillAsync(int orderId)
        {
            using var conn = _context.CreateConnection();
            var affected = await conn.ExecuteAsync(@"
                UPDATE ""PendingOrders"" 
                SET ""Status"" = 'Filled', ""FilledAt"" = @Now
                WHERE ""Id"" = @Id AND ""Status"" = 'Pending'",
                new { Id = orderId, Now = DateTime.UtcNow });
            
            Debug.WriteLine($"[DATA] PendingOrder.Fill orderId={orderId} affected={affected}");
            return affected > 0;
        }

        /// <summary>
        /// Belirli bir emri getir
        /// </summary>
        public async Task<PendingOrder?> GetByIdAsync(int orderId)
        {
            using var conn = _context.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PendingOrder>(@"
                SELECT * FROM ""PendingOrders"" WHERE ""Id"" = @Id", new { Id = orderId });
        }
    }
}
