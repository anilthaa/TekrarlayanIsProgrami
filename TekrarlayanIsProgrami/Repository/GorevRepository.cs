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
            var sql = @"Select * From ""Gorev"" Where ""DelDate"" Is Null Order By ""BaslangicTarihi"" ";
            var result = await conn.QueryAsync<ItemGorev>(sql);
            return result.ToList();
        }

        public async Task<List<ItemGorev>> YaklasanGorevlerAdminAsync()
        {
            using var conn = GetConnection();
            var sql = @"Select * From ""Gorev"" Where ""DelDate"" Is Null And ""SonrakiCalismaTarihi"">=NOW() Order By ""SonrakiCalismaTarihi"" ASC";
            var result = await conn.QueryAsync<ItemGorev>(sql);
            return result.ToList();
        }

        public async Task<List<ItemGorev>> GecmisGorevlerAdminAsync()
        {
            using var conn= GetConnection();
            var sql = @"Select * From ""Gorev"" Where ""DelDate"" Is Null And ""BitisTarihi""<Now() Order By ""BitisTarihi"" DESC";
            var result = await conn.QueryAsync<ItemGorev>(sql);
            return result.ToList();
        }
        //public async Task<Gorev?> GetByIdAsync(int id)
        //{
        //    using var conn = GetConnection();
        //    return await conn.QueryFirstOrDefaultAsync<Gorev>(
        //        @"SELECT * FROM ""Gorev"" WHERE ID=@ID", new { ID = id });
        //}

        public async Task<int> InsertAsync(ItemGorev gorev)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO ""Gorev""
                      (""Kullanici_ID"", ""Baslik"", ""Kategori_ID"", ""OnemDerecesi"",
                        ""BaslangicTarihi"", ""BitisTarihi"", ""TekrarlamaTipi"",
                          ""Aktif"",""Kilitli"",""TekrarAraligi"", 
                            ""HaftaninGunu"",""AyinGunu"",""SonrakiCalismaTarihi"", ""CreUser"", ""CreDate"") 
                    VALUES 
                    (@Kullanici_ID, @Baslik, @Kategori_ID, @OnemDerecesi,
                    @BaslangicTarihi, @BitisTarihi, @TekrarlamaTipi, 
                    @Aktif ,@Kilitli ,@TekrarAraligi,
                    @HaftaninGunu,@AyinGunu,@SonrakiCalismaTarihi, @CreUser,NOW())
                    RETURNING ""ID"";";
                    
                    return await conn.ExecuteScalarAsync<int>(sql, gorev);
        }

        public async Task<bool> UpdateAsync(ItemGorev gorev)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE ""Gorev"" SET 
                        ""Kullanici_ID""=@Kullanici_ID, 
                        ""Baslik""=@Baslik,
                        ""Kategori_ID""=@Kategori_ID,
                        ""OnemDerecesi""=@OnemDerecesi,
                        ""BaslangicTarihi""=@BaslangicTarihi,
                        ""BitisTarihi""=@BitisTarihi,
                        ""TekrarlamaTipi""=@TekrarlamaTipi,
                        ""Aktif""=@Aktif,
                        ""Kilitli""=@Kilitli,
                        ""TekrarAraligi""=@TekrarAraligi,
                        ""HaftaninGunu""=@HaftaninGunu,
                        ""AyinGunu""=@AyinGunu,
                        ""SonrakiCalismaTarihi""=@SonrakiCalismaTarihi,
                        ""UpdUser""=@UpdUser,
                        ""UpdDate""=NOW()
                    WHERE ""ID""=@ID And ""DelDate"" Is Null";
            var result=await conn.ExecuteAsync(sql, gorev);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE ""Gorev"" Set ""DelUser""=@userId, ""DelDate""=Now(), ""Aktif""=false Where ""ID""=@id And ""DelDate"" Is Null;" ;
            var result= await conn.ExecuteAsync(sql, new {id, userId});
            return result > 0;
        }

        public async Task<(int aktif, int silinen, int tamamlanan)> GetIstatistikAsync()
        {
            using var conn= GetConnection();
            var sql = @"
                Select
                    Count(*) FILTER (Where ""Aktif""=true And ""DelDate"" Is Null) As aktif,
                    Count(*) FILTER (Where ""DelDate"" Is Not Null) As silinen,
                    Count(*) FILTER (Where ""Aktif"" =false And ""DelDate"" Is Null) As tamamlanan From ""Gorev"";";
            var result = await conn.QueryFirstAsync<(int aktif, int silinen, int tamamlanan)>(sql);
            return result;
        }
    }
}
