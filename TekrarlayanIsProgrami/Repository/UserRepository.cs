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
        public async Task<User?> GetByIDAsync(int id)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""ID"" = @ID";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { ID= id });
            
        }
        public async Task<User?> GetByEmailAsync(string eposta)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { eposta });
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            string sql = @"UPDATE ""Kullanici""
                   SET ""Adi""=@Adi,
                       ""Soyadi""=@Soyadi,
                       ""EPosta""=@EPosta,
                       ""Sifre""=@Sifre
                   WHERE ""ID""=@ID";
            var result = await conn.ExecuteAsync(sql, new
            {
                user.Adi,
                user.Soyadi,
                user.EPosta,
                user.Sifre,
                user.ID
            });
            return result > 0;
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

        public async Task<User> KayitOlAsync(string eposta,string sifre ,string ad ,string soyad )
        {
            using var conn = GetConnection();

            var sqlInsert = @"INSERT INTO ""Kullanici"" 
        (""Adi"", ""Soyadi"", ""KullaniciTip_ID"", ""EPosta"", ""Sifre"", ""Aktif"", ""CreUser"", ""CreDate"", ""ResetToken"") 
        VALUES (@Adi, @Soyadi, @KullaniciTip_ID, @EPosta, @Sifre, @Aktif, @CreUser, @CreDate, @ResetToken)
        RETURNING ""ID"";";

            var user = new User
            {
                Adi = ad,
                Soyadi = soyad,
                EPosta = eposta,
                Sifre = sifre,
                Aktif = true,
                CreUser = 1,
                CreDate = DateTime.Now,
                ResetToken = Guid.NewGuid().ToString()
            };

            user.ID = await conn.ExecuteScalarAsync<int>(sqlInsert, new
            {
                user.Adi,
                user.Soyadi,
                KullaniciTip_ID = 0,
                user.EPosta,
                user.Sifre,
                user.Aktif,
                user.CreUser,
                user.CreDate,
                user.ResetToken
            });

            return user;
        }

        public async Task SaveResetTokenAsync(int Id, string token)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var sql = @"UPDATE ""Kullanici"" SET ""ResetToken"" = @token WHERE ""ID"" = @id";
            await conn.ExecuteAsync(sql, new { token, id = Id });
        }

    }
}
