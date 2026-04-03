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
            // KullaniciTip tablosuna varsayilan alanini ekle (eski veritabanlari icin uyumluluk).
            await _db.SQLExecute(@"ALTER TABLE ""KullaniciTip"" 
                                   ADD COLUMN IF NOT EXISTS ""Varsayilan"" boolean DEFAULT false;");

            var varsayilanVarMi = await _db.SQLExecuteScalar<int>(@"
                SELECT COUNT(*) 
                FROM ""KullaniciTip""
                WHERE (""DelUser"" IS NULL OR ""DelUser"" = 0)
                  AND COALESCE(""Varsayilan"", false) = true
            ");

            var email = _config["AdminUser:Email"];
            var password = _config["AdminUser:Password"];
            var ad = _config["AdminUser:Adi"];
            var soyad = _config["AdminUser:Soyadi"];

            // Config boş bırakılırsa da ilk admin kullanıcı garanti oluşturulsun.
            email = string.IsNullOrWhiteSpace(email) ? "yonetici@gmail.com" : email.Trim();
            password = string.IsNullOrWhiteSpace(password) ? "yonetici123" : password;
            ad = string.IsNullOrWhiteSpace(ad) ? "Sistem" : ad.Trim();
            soyad = string.IsNullOrWhiteSpace(soyad) ? "Yönetici" : soyad.Trim();

            var yoneticiRolKontrol = await _db.SQLExecuteScalar<int>(@"SELECT COUNT(*) FROM ""KullaniciTip"" WHERE ""Adi""=@adi", new { adi = "Yönetici" });

            if (yoneticiRolKontrol == 0)
            {
                await _db.SQLExecute(@"
                    INSERT INTO ""KullaniciTip"" (""Adi"", ""Aktif"", ""Varsayilan"")
                    VALUES (@adi, true, @varsayilan)
                ", new { adi = "Yönetici", varsayilan = varsayilanVarMi == 0 });
            }
            else
            {
                await _db.SQLExecute(@"UPDATE ""KullaniciTip"" SET ""Aktif"" = true WHERE ""Adi"" = @adi", new { adi = "Yönetici" });
                // Varsayilan hic yoksa Yonetici'yi varsayilan yap.
                if (varsayilanVarMi == 0)
                {
                    await _db.SQLExecute(@"UPDATE ""KullaniciTip"" SET ""Varsayilan"" = true WHERE ""Adi"" = @adi", new { adi = "Yönetici" });
                }
            }

            var YoneticiRolId = await _db.SQLExecuteScalar<int>(@"SELECT ""ID"" FROM ""KullaniciTip"" WHERE ""Adi""=@adi", new { adi = "Yönetici" });
            var mevcutKullaniciId = await _db.SQLExecuteScalar<int>(@"
                SELECT COALESCE(MAX(""ID""), 0)
                FROM ""Kullanici""
                WHERE lower(""EPosta"") = lower(@ePosta)
            ", new { ePosta = email });

            if (mevcutKullaniciId == 0)
            {
                await _db.SQLExecute(@"INSERT INTO ""Kullanici"" (""Adi"", ""Soyadi"", ""EPosta"", ""Sifre"", ""KullaniciTip_ID"", ""Aktif"", ""CreUser"", ""CreDate"")
                                       VALUES (@adi, @soyadi, @ePosta, @sifre, @kullaniciTip_Id, true, 0, now())",
                    new
                    {
                        adi = ad,
                        soyadi = soyad,
                        ePosta = email,
                        sifre = PasswordHelper.HashPassword(password),
                        kullaniciTip_Id = YoneticiRolId
                    });
            }
            else
            {
                // Mevcut yöneticiyi rol/aktiflik ile senkronize et; ad, soyad, şifre güncelleme —
                // profilden değişen alanlar her açılışta config’e dönmesin.
                await _db.SQLExecute(@"
                    UPDATE ""Kullanici""
                    SET ""KullaniciTip_ID"" = @kullaniciTip_Id,
                        ""Aktif"" = true,
                        ""DelUser"" = 0,
                        ""DelDate"" = NULL,
                        ""UpdUser"" = 0,
                        ""UpdDate"" = now()
                    WHERE ""ID"" = @id
                ", new
                {
                    id = mevcutKullaniciId,
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
