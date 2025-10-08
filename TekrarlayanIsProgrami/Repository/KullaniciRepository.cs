using Microsoft.Extensions.Configuration;
using Npgsql;
using TekrarlayanIsProgrami.Models;
using Dapper;

namespace TekrarlayanIsProgrami.Repository
{
    public class KullaniciRepository
    {
        private readonly string _connectionString;

        public KullaniciRepository(IConfiguration configuration)
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
        public async Task<User?> GetByIDAsync(int id)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""ID"" = @ID";
            var kullanici = await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { ID= id });
            return kullanici;
        }
        public async Task<User?> GetByEmailAsync(string eposta)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { eposta });
        }
        public async Task<User?> GetByEmailAndPasswordAsync(string eposta, string sifre)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta and ""Sifre""=@Sifre";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { eposta });
        }

        public async Task UpdateAsync(User user)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE ""Kullanici"" SET 
                        Adi=@Adi, 
                        Soyadi=@Soyadi,
                        EPosta=@EPosta,
                        Sifre=@Sifre,
                        UpdUser=ID,
                        UpdDate=NOW()
                    WHERE ID=@ID";
            await conn.ExecuteAsync(sql, user);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = GetConnection();
            var sql = @"DELETE FROM ""Kullanici"" WHERE ""ID""=@ID";
            await conn.ExecuteAsync(sql, new { ID = id });
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
