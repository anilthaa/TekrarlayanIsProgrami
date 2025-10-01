using Microsoft.Extensions.Configuration;
using Npgsql;
using TekrarlayanIsProgrami.Models;
using Dapper;

namespace TekrarlayanIsProgrami.Repository
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSqlConnection");
        }
        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<List<User>> GetAllAsync()
        {
            using var conn=GetConnection();
            var sql = @"Select * From ""Kullanici""";
            var result=await conn.QueryAsync<User>(sql);
            return result.ToList();
        }

        public async Task UpdateAsync(User user)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE ""Kullanici"" SET 
                        Adi=@Adi, 
                        Soyadi=@Soyadi,
                        EPosta=@EPosta,
                        KullaniciTip_ID=1,
                        Sifre=@Sifre,
                        Aktif=@Aktif,
                        UpdUser=ID,
                        UpdDate=NOW()
                    WHERE ID=@ID";
            await conn.ExecuteAsync(sql, user);
        }

        public async Task DeleteAsync(int id)
        {
           using var conn = GetConnection();
            await conn.ExecuteAsync(@"DELETE FROM ""Kullanici""
                                   DelUser=ID,
                                     DelDate=Now()
                                     WHERE ID=@ID", new { ID = id });
        }

        public async Task<User?> GirisYapAsync(string eposta, string sifre)
        {
            using var conn=GetConnection();
            var sql = @"Select * From ""Kullanici"" Where ""EPosta""=@EPosta and ""Sifre""=@Sifre and ""Aktif""=true";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new {EPosta=eposta, Sifre=sifre});
        }

        public async Task<bool> KayitOlAsync(User user)
        {
            using var conn = GetConnection();
            var sql = @"Insert Into ""Kullanici"" (""Adi"", ""Soyadi"",""KullaniciTip_ID"",""EPosta"",""Sifre"",""Aktif"",""CreUser"",""CreDate"") values (@Adi, @Soyadi,@KullaniciTip_ID,@EPosta, @Sifre, @Aktif, @CreUser, @CreDate)";

            var result = await conn.ExecuteAsync(sql, new
            {

                user.Adi,
                user.Soyadi,
                KullaniciTip_ID = 0,
                user.EPosta,
                user.Sifre,
                user.Aktif,
                user.CreUser,
                CreDate = DateTime.Now

            }); 
            return result > 0;
        }
    }
}
