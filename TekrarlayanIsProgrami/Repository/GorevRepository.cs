using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Repository
{
    public class GorevRepository
    {
        private readonly string _connectionString;

        public GorevRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSqlConnection"); ;
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
        //Gorev tablosunu listeleme.
        public async Task<List<ItemGorev>> GetAllAsync()
        {
            using var conn = GetConnection();
            var sql = @"Select * From ""Gorev""";
            var result = await conn.QueryAsync<ItemGorev>(sql);
            return result.ToList();
        }

        //public async Task<Gorev?> GetByIdAsync(int id)
        //{
        //    using var conn = GetConnection();
        //    return await conn.QueryFirstOrDefaultAsync<Gorev>(
        //        @"SELECT * FROM ""Gorev"" WHERE ID=@ID", new { ID = id });
        //}

        public async Task<int> İnsertAsync(ItemGorev task)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO Tasks (Kullanici_ID, Baslik, Kategori_ID, OnemDerecesi, BaslangicTarihi, BitisTarihi, TekrarlamaTip,Aktif,Kilitli,TekrarAraligi ,HaftaninGunu,AyınGunu,SonrakiCalismaTarihi) 
                    VALUES (@Kullanici_ID, @Baslik, @Kategori_ID, @OnemDerecesi, @BaslangicTarihi, @BitisTarihi, @TekrarlamaTip ,@Aktif ,@Kilitli ,@TekrarAraligi,@HaftaninGunu,@AyınGunu,@SonrakiCalismaTarihi) 
                    CreDate=Now(),
                    RETURNING ID";
            return await conn.ExecuteScalarAsync<int>(sql, task);
        }

        public async Task UpdateAsync(ItemGorev task)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE ""Gorev"" SET 
                        Kullanici_ID=@Kullanici_ID, 
                        Baslik=@Baslik,
                        Kategori_ID=@Kategori_ID,
                        OnemDerecesi=@OnemDerecesi,
                        BaslangicTarihi=@BaslangicTarihi,
                        BitisTarihi=@BitisTarihi,
                        TekrarlamaTipi=@TekrarlamaTipi,
                        Aktif=@Aktif,
                        Kilitli=@Kilitli,
                        TekrarAraligi=@TekrarAraligi,
                        HaftaninGunu=@HaftaninGunu,
                        AyınGunu=@AyınGunu,
                        SonrakiCalismaTarihi=@SonrakiCalismaTarihi,
                        UpdDate=NOW()
                    WHERE ID=@ID";
            await conn.ExecuteAsync(sql, task);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = GetConnection();
            await conn.ExecuteAsync(@"DELETE FROM ""Gorev""
                                      DelDate=Now(),
                                     WHERE ID=@ID", new { ID= id });
        }
    }
}
