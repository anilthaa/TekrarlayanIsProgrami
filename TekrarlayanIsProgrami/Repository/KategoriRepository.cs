using Dapper;
using Npgsql;
using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Repository
{
    public class KategoriRepository
    {
        private readonly string _connectionString;
        public KategoriRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSqlConnection");
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<List<ItemKategori>> GetAllAsync()
        {
            using var conn = GetConnection();
            var sql = @"Select * from ""Kategori"" Where ""DelDate"" is null order by ""Adi""";
            var result=await conn.QueryAsync<ItemKategori>(sql);
            return result.ToList();
        }

        public async Task<ItemKategori?> GetByIdAsync(int id)
        {
            using var conn = GetConnection();
            var sql = @"Select * From ""Kategori"" Where ""ID""=@ID And ""DelDate"" Is Null";
            return await conn.QueryFirstOrDefaultAsync<ItemKategori?>(sql, new {ID=id});
        }

        public async Task<bool> AddAsync(ItemKategori kategori)
        {
            using var conn = GetConnection();
            var sql = @"Insert Into ""Kategori"" (""Adi"", ""CreUser"",""CreDate"") Values (@Adi, @CreUser, @CreDate)";
            var result=await conn.ExecuteAsync(sql, new
            {
                kategori.Adi,
                kategori.CreUser,
                CreDate=DateTime.Now
            });
            return result > 0;
        }

        public async Task<bool> UpdateAsync(ItemKategori kategori)
        {
            using var conn = GetConnection();
            var sql = @"Update ""Kategori"" Set ""Adi""=@Adi, ""UpdUser""=@UpdUser, ""UpdDate""=@UpdDate Where ""ID""=@ID And ""DelDate"" Is Null";
            var result = await conn.ExecuteAsync(sql, new
            {
                kategori.Adi,
                kategori.UpdUser,
                UpdDate=DateTime.Now,
                kategori.ID
            });
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            using var conn = GetConnection();
            var sql = @"Update ""Kategori"" Set ""DelUser""=@DelUser, ""DelDate""=@DelDate Where ""ID""=@ID And ""DelDate"" Is Null";
            var result = await conn.ExecuteAsync(sql, new
            {
                DelUser=userId,
                DelDate=DateTime.Now,
                ID=id,
            });
            return result > 0;
        }
    }
}
