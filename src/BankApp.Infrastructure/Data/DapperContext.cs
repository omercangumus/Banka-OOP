using Npgsql;
using System.Data;

namespace BankApp.Infrastructure.Data
{
    public class DapperContext
    {
        private readonly string _connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;";

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
