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
            var sql = @"Select * From ""Kullanici"" WHERE ""DelDate"" IS NULL";
            var result=await conn.QueryAsync<User>(sql);
            return result.ToList();
        }
        public async Task<User?> GetByIDAsync(int ID)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""ID"" = @ID";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { ID= ID });
        }
        public async Task<User?> GetByEmailAsync(string EPosta, string Sifre)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta and ""Sifre""=@Sifre";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { EPosta, Sifre});
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            string sql = @"UPDATE ""Kullanici""
           SET ""Adi""=@Adi,
               ""Soyadi""=@Soyadi,
               ""EPosta""=@EPosta,
               ""Sifre""=@Sifre,
               ""Aktif""=@Aktif,
               ""UpdUser""=@UpdUser,
               ""UpdDate""=NOW()
           WHERE ""ID""=@ID";

            var result = await conn.ExecuteAsync(sql, new
            {
                user.Adi,
                user.Soyadi,
                user.EPosta,
                user.Sifre,
                user.Aktif,
                user.UpdUser, // Burada eksik olan parametre eklendi
                user.ID
            });

            return result > 0;
        }
        public async Task<bool> SifreGuncelle(string Token, string YeniSifre)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            // TODO: Token veritabanında doğrula ve kullanıcıyı bul
            var kullanici = await baglanti.QueryFirstOrDefaultAsync<User>("SELECT * FROM \"Kullanici\" WHERE \"ResetToken\" = @token", new { Token });
            if (kullanici == null)
            return false;

            var sql = @"UPDATE ""Kullanici"" SET ""Sifre"" = @sifre, ""ResetToken"" = NULL WHERE ""ID"" = @id";
            await baglanti.ExecuteAsync(sql, new { sifre = YeniSifre, id = kullanici.ID });

            return true;
        }

        public async Task DeleteAsync(int id, int delUser)
        {
            using var conn = GetConnection();
            var sql = @"Update ""Kullanici"" Set ""DelUser""=@ID, ""DelDate""=now() Where ""ID""=@ID";
            await conn.ExecuteAsync(sql, new { ID = id , DelUser=delUser});
        }

        public async Task<User?> GirisYapAsync(string eposta, string sifre)
        {
            using var conn=GetConnection();
            var sql = @"Select * From ""Kullanici"" Where ""EPosta""=@EPosta and ""Sifre""=@Sifre and ""Aktif""=true";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new {EPosta=eposta, Sifre=sifre});
        }

        public async Task<User> KayitOlAsync(string Adi, string Soyadi,string EPosta,string Sifre ,string SifreTekrar, string? ResetToken)
        {
            using var conn = GetConnection();

            var sqlInsert = @"INSERT INTO ""Kullanici"" 
        (""Adi"", ""Soyadi"", ""KullaniciTip_ID"", ""EPosta"", ""Sifre"", ""Aktif"", ""CreUser"", ""CreDate"", ""ResetToken"") 
        VALUES (@Adi, @Soyadi, @KullaniciTip_ID, @EPosta, @Sifre, @Aktif, @CreUser, @CreDate, @ResetToken)
        RETURNING ""ID"";";

            var user = new User
            {
                Adi = Adi, //Buraya @ eklenilcek gibi.
                Soyadi = Soyadi,
                EPosta = EPosta,
                Sifre = Sifre,
                Aktif = true,
                CreUser = 1,
                CreDate = DateTime.Now,
                ResetToken = ResetToken
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
        //Bu kayıt Ol için bir eposta varmı kontrolu.
        public async Task<bool> EpostaVarMiAsync(string EPosta)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var sql = await conn.QueryFirstOrDefaultAsync<User>(
               @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta", new { EPosta = EPosta });
            return sql != null;
        }
        //Bu bizim şifremizi değiştirdiğimiz zaman tokenimizi değiştirmek için yazdık.
        public async Task SaveResetTokenAsync(int Id, string token)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            // ResetToken kolonu varchar(100) olduğu için uzun JWT'lerde taşma olmaması adına kısaltıyoruz
            var tokenToStore = token != null && token.Length > 100 ? token.Substring(0, 100) : token;
            var sql = @"UPDATE ""Kullanici"" SET ""ResetToken"" = @token WHERE ""ID"" = @id";
            await conn.ExecuteAsync(sql, new { token = tokenToStore, id = Id });
        }
        public async Task<User?> SifreEmailAsync(string EPosta)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { EPosta });
        }
    }
}
