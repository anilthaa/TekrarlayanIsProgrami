using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Repository
{
    public class GorevLoglariRepository
    {
        private readonly string _connectionString;

        public GorevLoglariRepository( IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSqlConnection");

        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        //Kullanici tablosunu listeledik.
        public async Task<List<ItemGorevLoglari>> GetAllAsync()
        {
            using var conn = GetConnection();
            var sql = @"Select * From ""GorevLoglari""";
            var result = await conn.QueryAsync<ItemGorevLoglari>(sql);
            return result.ToList();
        }

        //Kullanici tablosu güncellemek için.
        public async Task UpdateAsync(ItemGorevLoglari TaskLogs)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE ""GorevLoglari"" SET 
                        Gorev_ID=@Gorev_ID,
                        UpdUser=ID,
                        UpdDate=NOW()
                    WHERE ID=@ID";
            await conn.ExecuteAsync(sql, TaskLogs);
        }

        //Kullanici tablosundan silme işlemi.

        public async Task DeleteAsync(int id)
        {
            using var conn = GetConnection();
            await conn.ExecuteAsync(@"DELETE FROM ""GorevLoglari""
                                     DelUser=ID,
                                     DelDate=Now()
                                     WHERE ID=@ID", new { ID = id });
        }

    }
}
