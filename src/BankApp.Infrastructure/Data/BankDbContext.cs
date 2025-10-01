using Microsoft.EntityFrameworkCore;
using BankApp.Core.Entities;

namespace BankApp.Infrastructure.Data
{
    public class BankDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Entities (Optional explicit configs if needed beyond conventions)
            modelBuilder.Entity<Customer>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Account>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(a => a.CustomerId);
                
            modelBuilder.Entity<Transaction>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.AccountId);

            // Seed Data (EF Core Style) - Optional, but we have DbInitializer too.
            // Let's rely on DbInitializer or doing it manually to stay consistent with previous code
            // Or use EnsureCreated which will run this.
        }
    }
}
