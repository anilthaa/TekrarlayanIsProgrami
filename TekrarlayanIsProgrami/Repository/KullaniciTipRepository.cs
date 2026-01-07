using Dapper;
using Npgsql;
using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Repository
{
    public class KullaniciTipRepository
    {
        private readonly string _connectionString;
        public KullaniciTipRepository( IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSqlConnection");
        }
        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        //verilen kullanıcı ID’sine ait kullanıcı tipini veritabanından getirir.
        public async Task<ItemKullaniciTip?> GetKullaniciTipByKullaniciIDAsync(int kullaniciID)
        {
            using var baglanti = new NpgsqlConnection(_connectionString);
            var sql = @"
        SELECT kt.*
        FROM ""Kullanici"" k
        INNER JOIN ""KullaniciTip"" kt ON k.""KullaniciTip_ID"" = kt.""ID""
        WHERE k.""ID"" = @kullaniciID AND kt.""DelDate"" IS NULL
    ";
            return await baglanti.QueryFirstOrDefaultAsync<ItemKullaniciTip>(sql, new { kullaniciID });
        }

    }
}
