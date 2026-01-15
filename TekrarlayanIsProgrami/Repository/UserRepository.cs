using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using TekrarlayanIsProgrami.Models;
using TekrarlayanIsProgrami.Services;

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

        //aktif tüm kullanıcıları veritabanından getirir.
        public async Task<List<User>> GetAllAsync()
        {
            using var conn = GetConnection();
            var sql = @"Select * From ""Kullanici"" WHERE ""DelDate"" IS NULL";
            var result = await conn.QueryAsync<User>(sql);
            return result.ToList();
        }

        //verilen ID’ye sahip kullanıcıyı veritabanından getirir.
        public async Task<User?> GetByIDAsync(int ID)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""ID"" = @ID";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { ID = ID });
        }

        //verilen şifre sıfırlama token’ına sahip kullanıcıyı veritabanından getirir.
        public async Task<User?> GetByTokenAsync(string token)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" 
                WHERE ""ResetToken"" = @token";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { token });
        }

        //var olan bir kullanıcıyı veritabanında günceller.
        public async Task<bool> UpdateAsync(User user)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            
            string sql;
            if (string.IsNullOrWhiteSpace(user.Sifre))
            {
                sql = @"UPDATE ""Kullanici""
           SET ""Adi""=@Adi,
               ""Soyadi""=@Soyadi,
               ""EPosta""=@EPosta,
               ""Aktif""=@Aktif,
               ""KullaniciTip_ID""=@KullaniciTip_ID,
               ""UpdUser""=@UpdUser,
               ""UpdDate""=NOW()
           WHERE ""ID""=@ID";

                var result = await conn.ExecuteAsync(sql, new
                {
                    user.Adi,
                    user.Soyadi,
                    user.EPosta,
                    user.Aktif,
                    user.KullaniciTip_ID,
                    user.UpdUser,
                    user.ID
                });

                return result > 0;
            }
            else
            {
                sql = @"UPDATE ""Kullanici""
           SET ""Adi""=@Adi,
               ""Soyadi""=@Soyadi,
               ""EPosta""=@EPosta,
               ""Sifre""=@Sifre,
               ""Aktif""=@Aktif,
               ""KullaniciTip_ID""=@KullaniciTip_ID,
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
                    user.KullaniciTip_ID,
                    user.UpdUser,
                    user.ID
                });

                return result > 0;
            }
        }

        //şifre sıfırlama token’ına bağlı olarak kullanıcının şifresini günceller.
        public async Task<bool> SifreGuncelle(string token, string yeniSifre)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);

            var kullanici = await baglanti.QueryFirstOrDefaultAsync<User>(
                @"SELECT * FROM ""Kullanici"" WHERE ""ResetToken"" = @token and ""ResetTokenExpiry"" > now() at time zone 'utc'",
                new { token }
            );
            Console.WriteLine($"Kullanıcı bulundu mu: {kullanici != null}, ID: {kullanici?.ID}");

            if (kullanici == null)
                return false;

            // Şifreyi hash'le
            var hashedPassword = PasswordHelper.HashPassword(yeniSifre);

            var sql = @"UPDATE ""Kullanici"" 
                SET ""Sifre"" = @sifre,
                    ""ResetToken"" = NULL,
                    ""ResetTokenExpiry"" = NULL
                WHERE ""ID"" = @id";

            await baglanti.ExecuteAsync(sql, new { sifre = hashedPassword, id = kullanici.ID });

            return true;
        }

        //bir kullanıcıyı silinmiş olarak işaretler (fiziksel olarak silmez, soft delete yapar).
        public async Task DeleteAsync(int id, int delUser)
        {
            using var conn = GetConnection();
            var sql = @"Update ""Kullanici"" Set ""DelUser""=@DelUser, ""DelDate""=now() Where ""ID""=@ID";
            await conn.ExecuteAsync(sql, new { ID = id, DelUser = delUser });
        }

        //yeni kullanıcı kaydı işlemini yapar ve kaydedilen kullanıcıyı döndürür.
        public async Task<User> KayitOlAsync(User user)  
        {
            using var conn = GetConnection();
            user.Sifre = PasswordHelper.HashPassword(user.Sifre);
            int Tip_ID;

            if (user.KullaniciTip_ID == null || user.KullaniciTip_ID == 0)
            {
                Console.WriteLine("Hiç tip seçilmemiş, varsayılan tip atanacak.");
                Tip_ID = 2;
            }
            else
            {
                Tip_ID = user.KullaniciTip_ID;
            }
            user.KullaniciTip_ID = Tip_ID;
            user.Aktif = user.KullaniciTip_ID == 1 ? false : true;
            user.ID = await conn.ExecuteScalarAsync<int>(@"INSERT INTO ""Kullanici"" 
 (""Adi"", ""Soyadi"", ""KullaniciTip_ID"", ""EPosta"", ""Sifre"", ""Aktif"", ""CreUser"", ""CreDate"", ""ResetToken"") 
 VALUES (@Adi, @Soyadi, @KullaniciTip_ID, @EPosta, @Sifre, @Aktif, @CreUser, Now(), @ResetToken)
 RETURNING ""ID""", user);
            return user;
        }

        //verilen e-posta adresinin veritabanında kayıtlı olup olmadığını kontrol eder.
        public async Task<bool> EpostaVarMiAsync(string EPosta)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var sql = await conn.QueryFirstOrDefaultAsync<User>(
               @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta AND ""DelDate"" IS NULL", new { EPosta = EPosta });
            return sql != null;
        }

        //verilen e-posta adresine sahip kullanıcıyı getirir.
        public async Task<User?> SifreEmailAsync(string EPosta)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT ""ID"", ""EPosta"", ""Adi"", ""Sifre"" FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { EPosta });
        }

        //kullanıcının şifre sıfırlama token’ını ve süresini veritabanına kaydeder.
        public async Task SifreSifirlamaTokenKaydet(int id, string token, DateTime expiry)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"UPDATE ""Kullanici""
                SET ""ResetToken"" = @token,
                    ""ResetTokenExpiry"" = @expiry
                WHERE ""ID"" = @id";
            await baglanti.ExecuteAsync(sql, new { id, token, expiry });
        }

        //giriş yapmak isteyen kullanıcıyı doğrulamak için veritabanından getirir.
        public async Task<User?> GetByGirisAsync(string EPosta)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT * FROM ""Kullanici"" WHERE ""EPosta"" = @EPosta and ""Aktif""=true and ""DelDate"" is null";
            return await baglanti.QueryFirstOrDefaultAsync<User>(sql, new { EPosta});
        }

        //var olan bir kullanıcının şifresini günceller.
        public async Task<bool> UpdateSifreAsync(int userId, string hashedPassword)
        {
            using var conn = GetConnection();

            var sql = @"
    UPDATE ""Kullanici""
    SET ""Sifre"" = @Sifre,
        ""UpdDate"" = NOW()
    WHERE ""ID"" = @ID";

            var affected = await conn.ExecuteAsync(sql, new { ID = userId, Sifre = hashedPassword });

            return affected > 0;
        }

        //kullanıcının profil bilgilerini günceller.
        public async Task UpdateUserProfile(User user)
        {
            using var conn = GetConnection();
            var sql = @"
                        UPDATE ""Kullanici""
                        SET
                               ""Adi"" = @Adi,
                               ""Soyadi"" = @Soyadi,
                               ""UpdDate"" = NOW()
                       WHERE ""ID"" = @ID";
            await conn.ExecuteAsync(sql, user);
        }
    }
}