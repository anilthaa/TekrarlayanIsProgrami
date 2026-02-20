using Npgsql;
using Radzen;

namespace TekrarlayanIsProgrami.Services
{
    public class DBSeeder
    {
        private readonly DBClass _db;
        private readonly IConfiguration _config;
        private readonly ILogger<DBSeeder> _logger;

        public DBSeeder(DBClass db, IConfiguration config, ILogger<DBSeeder> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task AdminSeed()
        {
            var email = _config["AdminUser:Email"];
            var password = _config["AdminUser:Password"];

            var yoneticiRolKontrol = await _db.SQLExecuteScalar<int>(@"SELECT COUNT(*) FROM ""KullaniciTip"" WHERE ""Adi""=@adi", new { adi = "Yönetici" });

            if (yoneticiRolKontrol == 0)
            {
                await _db.SQLExecute(@"INSERT INTO ""KullaniciTip"" (""Adi"") VALUES (@adi)", new { adi = "Yönetici" });
            }

            var YoneticiRolId = await _db.SQLExecuteScalar<int>(@"SELECT ""ID"" FROM ""KullaniciTip"" WHERE ""Adi""=@adi", new { adi = "Yönetici" });
            var yoneticiKullaniciKontrol = await _db.SQLExecuteScalar<int>(@"SELECT COUNT(*) FROM ""Kullanici"" WHERE ""EPosta""=@ePosta", new { ePosta = email });

            if (yoneticiKullaniciKontrol == 0)
            {
                await _db.SQLExecute(@"INSERT INTO ""Kullanici"" (""EPosta"", ""Sifre"", ""KullaniciTip_ID"") VALUES (@ePosta, @sifre, @kullaniciTip_Id)",
                    new
                    {
                        ePosta = email,
                        sifre = BCrypt.Net.BCrypt.HashPassword(password),
                        kullaniciTip_Id = YoneticiRolId
                    });
            }

            //Yönetici tipi için tüm yetkileri verir
            await YoneticiTipYetkiSeed(YoneticiRolId);
        }

        private async Task YoneticiTipYetkiSeed(int yoneticiTipId)
        {
            // KullaniciTipDetay tablosu var mı yok mu kontrol eder ve yoksa hata yutulur.
            try
            {
                var aktifFormlar = (await _db.SQLQueryAsync<int>(@"
                    SELECT ""ID"" FROM ""Form""
                    WHERE (""DelUser"" IS NULL OR ""DelUser"" = 0)
                ")).ToList();

                foreach (var formId in aktifFormlar)
                {
                    // Bu form için yönetici tipinde yetki var mı?
                    var mevcutYetki = await _db.SQLExecuteScalar<int>(@"
                        SELECT COUNT(*) FROM ""KullaniciTipDetay""
                        WHERE ""KullaniciTip_ID"" = @tipId
                          AND ""Form_ID"" = @formId
                          AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                    ", new { tipId = yoneticiTipId, formId });

                    if (mevcutYetki == 0)
                    {
                        // Yetki yok, yeni ekle (Yazma = 2)
                        await _db.SQLExecute(@"
                            INSERT INTO ""KullaniciTipDetay"" (""KullaniciTip_ID"", ""Form_ID"", ""Yetki"", ""CreUser"", ""CreDate"")
                            VALUES (@tipId, @formId, 2, 0, now())
                        ", new { tipId = yoneticiTipId, formId });
                    }
                    else
                    {
                        // Yetki var ama 2'den düşükse güncelle
                        await _db.SQLExecute(@"
                            UPDATE ""KullaniciTipDetay""
                            SET ""Yetki"" = 2, ""UpdDate"" = now()
                            WHERE ""KullaniciTip_ID"" = @tipId
                              AND ""Form_ID"" = @formId
                              AND ""Yetki"" < 2
                              AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                        ", new { tipId = yoneticiTipId, formId });
                    }
                }
            }
            catch(PostgresException ex) when (ex.SqlState == "42P01")
            {
                _logger.LogError("KullaniciTipDetay tablosu bulunamadı. Hata:{Message}",ex.Message);
                throw new ApplicationException("Yetki tablosu bulunamadı. Sistem yöneticisine başvurun.");
            }
        }
    }
}
