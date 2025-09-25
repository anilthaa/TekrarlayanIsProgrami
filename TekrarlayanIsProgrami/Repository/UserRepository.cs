using Microsoft.Extensions.Configuration;
using Npgsql;
using TekrarlayanIsProgrami.Models;
using Dapper;

namespace TekrarlayanIsProgrami.Repository
{
    public class UserRepository
    {
        private readonly NpgsqlConnection _connection;
        public UserRepository(IConfiguration configuration)
        {
            _connection = new NpgsqlConnection(configuration.GetConnectionString("PostgreSqlConnection"));
        }
        public async Task<List<User>> GetAll()
        {
            var sql = @"SELECT * FROM ""Kullanici""";   // Bizim tablo adı
            var result = await _connection.QueryAsync<User>(sql);
            return result.ToList();
        }
    
    }
}
