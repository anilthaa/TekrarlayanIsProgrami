using Dapper;
using Microsoft.AspNetCore.Components.Authorization;
using Npgsql;
using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Services
{
    public class DataService : DBClass
    {
        private readonly AuthenticationStateProvider _stateProvider;
        public DataService(string connectionString, AuthenticationStateProvider stateProvider) : base(connectionString)
        {
            _stateProvider = stateProvider;
        }

        public async Task<ItemKategori[]> GetKategori(int ID = 0)
        {
            return (await SQLQueryAsync<ItemKategori>($@"select ""ID"",""Adi"" from ""Kategori""         
        where ""DelUser"" is null 
        {(ID > 0 ? $@"and ""ID"" = {ID}" : "")} 
        ORDER BY ""ID"" ")).ToArray();
        }

        public async Task<DataResult<int>> SetKategori(ItemKategori item)
        {
            if (item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                item.ID = await SQLExecuteScalar<int>(@"INSERT INTO ""Kategori"" (""Adi"", ""CreUser"", ""CreDate"")  VALUES (@Adi, @CreUser, now()) 
                 RETURNING ""ID""", item);
            }
            else if (!item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                await SQLExecute(@"UPDATE ""Kategori"" 
                     SET ""Adi"" = @Adi, ""UpdUser""=@UpdUser, ""UpdDate""=now()
                     WHERE ""ID"" = @ID", item);
            }
            else if (!item.ID.Equals(0) && !item.DelUser.Equals(0))
            {
                await SQLExecute(@"UPDATE ""Kategori"" 
                     SET ""DelUser"" = @DelUser, ""DelDate"" = now() 
                     WHERE ""ID"" = @ID", item);
            }
            return new() { Data = item.ID };
        }


        public async Task<ItemGorev[]> GetGorevler()
        {
            return (await SQLQueryAsync<ItemGorev>($@"
                Select g.""ID"", g.""Kullanici_ID"", g.""Baslik"", g.""Kategori_ID"", k.""Adi"" as ""KategoriAdi"", g.""OnemDerecesi"",
                       g.""BaslangicTarihi"", g.""BitisTarihi"", g.""TekrarlamaTipi"", g.""Aktif"", g.""Kilitli"",
                       g.""TekrarAraligi"", g.""HaftaninGunu"", g.""AyinGunu"", g.""SonrakiCalismaTarihi"", g.""Tamamlandi"",
                       g.""CreUser"", g.""CreDate"", g.""UpdUser"", g.""UpdDate"", g.""DelUser"", g.""DelDate""
                From ""Gorev"" g
                Inner Join ""Kategori"" k on g.""Kategori_ID"" = k.""ID""
                Where g.""DelDate"" Is Null
                Order By g.""ID"" desc
            ")).ToArray();
        }

        public async Task<ItemGorev[]> YaklasanGorevlerAdmin()
        {


            return (await SQLQueryAsync<ItemGorev>($@"
                Select * From ""Gorev"" Where ""DelDate"" Is Null
                And ""SonrakiCalismaTarihi"">=NOW() Order By ""SonrakiCalismaTarihi"" Asc")).ToArray();
        }

        public async Task<ItemGorev[]> GecmisGorevlerAdmin()
        {

            return (await SQLQueryAsync<ItemGorev>($@"
                Select * From ""Gorev""
                Where ""DelDate"" Is Null
                And ""BitisTarihi"" <NOW()
                Order By ""BitisTarihi"" DESC")).ToArray();
        }

        public async Task<DataResult<int>> SetGorev(ItemGorev item)
        {


            if (item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                // CreDate'i uygulama tarafından set et (timezone sorunlarını önlemek için)
                if (item.CreDate == default(DateTime))
                {
                    item.CreDate = DateTime.Now;
                }
                
                item.ID = await SQLExecuteScalar<int>(@"INSERT INTO ""Gorev"" (""Kullanici_ID"", ""Baslik"", ""Kategori_ID"", ""OnemDerecesi"",
                ""BaslangicTarihi"",""BitisTarihi"",""TekrarlamaTipi"", ""Aktif"", ""Kilitli"", ""TekrarAraligi"", ""HaftaninGunu"",
                ""AyinGunu"", ""SonrakiCalismaTarihi"", ""Tamamlandi"",""CreUser"", ""CreDate"") 
                VALUES (@Kullanici_ID, @Baslik, @Kategori_ID, @OnemDerecesi, @BaslangicTarihi, @BitisTarihi, @TekrarlamaTipi, @Aktif, 
                @Kilitli, @TekrarAraligi, @HaftaninGunu, @AyinGunu, @SonrakiCalismaTarihi,false,@CreUser, @CreDate)
                 RETURNING ""ID""", item);
            }
            else if (!item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                // UpdDate'i uygulama tarafından set et (timezone sorunlarını önlemek için)
                item.UpdDate = DateTime.Now;
                
                await SQLExecute(@"UPDATE ""Gorev"" SET
                      ""Kullanici_ID""=@Kullanici_ID, ""Baslik""=@Baslik, ""Kategori_ID""=@Kategori_ID, 
                      ""OnemDerecesi""=@OnemDerecesi, ""BaslangicTarihi""=@BaslangicTarihi, ""BitisTarihi""=@BitisTarihi,
                      ""TekrarlamaTipi""=@TekrarlamaTipi, ""Aktif""=@Aktif, ""Kilitli""=@Kilitli, ""TekrarAraligi""=@TekrarAraligi,
                      ""HaftaninGunu""=@HaftaninGunu, ""AyinGunu""=@AyinGunu, ""SonrakiCalismaTarihi""=@SonrakiCalismaTarihi,""Tamamlandi""=@Tamamlandi, ""UpdUser""=@UpdUser, ""UpdDate""=@UpdDate
                     WHERE ""ID"" = @ID", item);
            }
            else if (!item.ID.Equals(0) && !item.DelUser.Equals(0))
            {
                // DelDate'i uygulama tarafından set et (timezone sorunlarını önlemek için)
                item.DelDate = DateTime.Now;
                
                await SQLExecute(@"UPDATE ""Gorev"" 
                     SET ""DelUser"" = @DelUser, ""DelDate"" = @DelDate 
                     WHERE ""ID"" = @ID", item);
            }
            return new() { Data = item.ID };
        }

        public async Task<(int aktif, int silinen, int tamamlanan)> GetIstatistik()
        {

            var sql = @"
                Select
                    Count(*) Filter (Where ""Aktif""=True And ""DelDate"" Is Null) As aktif,
                    Count(*) Filter (Where ""DelDate"" Is Not Null) As silinen,
                    Count(*) Filter (Where ""Aktif"" = False And ""DelDate"" Is Null) As tamamlanan 
                From ""Gorev"";";

            var list = await SQLQueryAsync<(int aktif, int silinen, int tamamlanan)>(sql);
            return list.FirstOrDefault();

        }

        private async Task CheckAdminRole()
        {
            var authState = await _stateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Giriş yapmanız gerekiyor.");
            }

            if (!user.IsInRole("Administrator"))
            {
                throw new UnauthorizedAccessException("Bu verilere erişim yetkiniz yok.");
            }
        }

        public async Task<ItemKullaniciTip[]> GetKullaniciTipleri(int ID = 0)
        {
            return (await SQLQueryAsync<ItemKullaniciTip>($@"
                select ""ID"", ""Adi"", ""Aktif"", ""CreUser"", ""CreDate"", ""UpdUser"", ""UpdDate"", ""DelUser"", ""DelDate""
                from ""KullaniciTip""
                where ""DelUser"" is null
                {(ID > 0 ? $@"and ""ID"" = {ID}" : "")}
                order by ""ID""
            ")).ToArray();
        }

        public async Task<DataResult<int>> SetKullaniciTip(ItemKullaniciTip item)
        {
            if (item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                item.ID = await SQLExecuteScalar<int>(@"INSERT INTO ""KullaniciTip"" (""Adi"", ""Aktif"", ""CreUser"", ""CreDate"")
                    VALUES (@Adi, @Aktif, @CreUser, now()) RETURNING ""ID""", item);
            }
            else if (!item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                await SQLExecute(@"UPDATE ""KullaniciTip""
                        SET ""Adi"" = @Adi,
                            ""Aktif"" = @Aktif,
                            ""UpdUser"" = @UpdUser,
                            ""UpdDate"" = now()
                        WHERE ""ID"" = @ID", item);
            }
            else if (!item.ID.Equals(0) && !item.DelUser.Equals(0))
            {
                await SQLExecute(@"UPDATE ""KullaniciTip""
                        SET ""DelUser"" = @DelUser,
                            ""DelDate"" = now()
                        WHERE ""ID"" = @ID", item);
            }
            return new() { Data = item.ID };
        }

        public async Task<ItemForm[]> GetFormsAsync(bool sadeceMenuler = false)
        {
           
            var sql = $@"
                select ""ID"",""Adi"",Coalesce(""IsMenu"", False) As ""IsMenu"", Coalesce(""Sira"", 0) As ""Sira"",""SayfaURL"",""Icon"" 
                from ""Form""
                where (""DelUser"" is null or ""DelUser"" = 0)
                {(sadeceMenuler ? @"and ""IsMenu"" = true" : string.Empty)}
                order by coalesce(""Sira"", 0), ""ID"";
            ";

            return (await SQLQueryAsync<ItemForm>(sql)).ToArray();
        }

        public async Task<ItemKullaniciDetay[]> GetKullaniciFormYetkileriAsync(int kullaniciId)
        {
            var sql = @"
                select ""ID"", ""Form_ID"", ""Yetki"", ""Kullanici_ID"", ""CreUser"", ""CreDate"", ""UpdUser"", ""UpdDate"", ""DelUser"", ""DelDate""
                from ""KullaniciDetay""
                where ""Kullanici_ID"" = @KullaniciId and (""DelUser"" is null or ""DelUser"" = 0);
            ";

            return (await SQLQueryAsync<ItemKullaniciDetay>(sql, new { KullaniciId = kullaniciId })).ToArray();
        }

        public async Task<DataResult<int>> SetKullaniciFormYetkiAsync(ItemKullaniciDetay item)
        {
            if (item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                item.ID = await SQLExecuteScalar<int>(
                    @"INSERT INTO ""KullaniciDetay"" (""Form_ID"", ""Kullanici_ID"", ""Yetki"", ""CreUser"", ""CreDate"")
                      VALUES (@Form_ID, @Kullanici_ID, @Yetki, @CreUser, now())
                      RETURNING ""ID""",
                    item);
            }
            else if (!item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                await SQLExecute(
                    @"UPDATE ""KullaniciDetay""
                      SET ""Yetki"" = @Yetki,
                          ""UpdUser"" = @UpdUser,
                          ""UpdDate"" = now()
                      WHERE ""ID"" = @ID",
                    item);
            }
            else if (!item.ID.Equals(0) && !item.DelUser.Equals(0))
            {
                await SQLExecute(
                    @"UPDATE ""KullaniciDetay""
                      SET ""DelUser"" = @DelUser,
                          ""DelDate"" = now()
                      WHERE ""ID"" = @ID",
                    item);
            }

            return new() { Data = item.ID };
        }

        public async Task<DataResult<int>> SetKullaniciLog(ItemKullaniciLog item)
        {
            try
            {
                Console.WriteLine($"SetKullaniciLog çağrıldı - Kullanıcı ID: {item.Kullanici_ID}, İşlem Tipi: {item.IslemTipi}");
                
                // IslemTarihi ve CreDate'i uygulama tarafından set et (timezone sorunlarını önlemek için)
                if (item.IslemTarihi == default(DateTime))
                {
                    item.IslemTarihi = DateTime.Now;
                }
                if (item.CreDate == default(DateTime))
                {
                    item.CreDate = DateTime.Now;
                }
                
                item.ID = await SQLExecuteScalar<int>(@"
                    INSERT INTO ""KullaniciLog"" (""Kullanici_ID"", ""IslemTipi"", ""IslemTarihi"", ""Aciklama"", ""CreUser"", ""CreDate"")
                    VALUES (@Kullanici_ID, @IslemTipi, @IslemTarihi, @Aciklama, @CreUser, @CreDate)
                    RETURNING ""ID""", item);
                
                Console.WriteLine($"SetKullaniciLog tamamlandı - Dönen ID: {item.ID}");
                
                return new() { Data = item.ID };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetKullaniciLog hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<DateTime?> GetSonGirisAsync(int kullaniciId)
        {
            // En son girişten bir önceki girişi alıyoruz
            // OFFSET 1 kullanıyoruz çünkü en son giriş şu anki giriş, bir önceki girişi alıyoruz
            // Eğer sadece bir giriş logu varsa (ilk giriş), null döndürüyoruz
            var sql = @"
                SELECT ""IslemTarihi""
                FROM ""KullaniciLog""
                WHERE ""Kullanici_ID"" = @KullaniciId
                  AND ""IslemTipi"" = 'Giris'
                  AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                ORDER BY ""IslemTarihi"" DESC
                LIMIT 1 OFFSET 1;";

            var result = await SQLQueryAsync<DateTime?>(sql, new { KullaniciId = kullaniciId });
            var sonGiris = result.FirstOrDefault();
            
            // Debug: Tüm giriş loglarını kontrol et
            var tumGirisler = await SQLQueryAsync<DateTime?>(@"
                SELECT ""IslemTarihi""
                FROM ""KullaniciLog""
                WHERE ""Kullanici_ID"" = @KullaniciId
                  AND ""IslemTipi"" = 'Giris'
                  AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                ORDER BY ""IslemTarihi"" DESC
                LIMIT 5;", new { KullaniciId = kullaniciId });
            
            Console.WriteLine($"GetSonGirisAsync - Kullanıcı ID: {kullaniciId}");
            Console.WriteLine($"  Toplam giriş logu: {tumGirisler.Count()}");
            foreach (var giris in tumGirisler)
            {
                Console.WriteLine($"    - Giriş: {giris}");
            }
            Console.WriteLine($"  Bir önceki giriş (OFFSET 1): {(sonGiris.HasValue ? sonGiris.Value.ToString() : "null (ilk giriş)")}");
            
            return sonGiris;
        }

        public async Task<int> GetYeniGorevSayisiAsync(int kullaniciId, DateTime? sonGirisTarihi)
        {
            // Debug: Tüm kullanıcıya atanan görevleri kontrol et
            var debugSql = @"
                SELECT ""ID"", ""Baslik"", ""Kilitli"", ""Kullanici_ID"", ""CreDate"", ""Aktif"", ""Tamamlandi""
                FROM ""Gorev""
                WHERE ((""Kilitli"" = true AND ""Kullanici_ID"" = @KullaniciId) OR ""Kilitli"" = false)
                  AND ""Aktif"" = true
                  AND ""Tamamlandi"" = false
                  AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                ORDER BY ""CreDate"" DESC
                LIMIT 10;";
            
            var debugGorevler = await SQLQueryAsync<dynamic>(debugSql, new { KullaniciId = kullaniciId });
            Console.WriteLine($"DEBUG: Kullanıcı {kullaniciId} için toplam {debugGorevler.Count()} aktif görev bulundu (kilitli veya herkese açık)");
            foreach (var g in debugGorevler)
            {
                Console.WriteLine($"  - ID: {g.ID}, Baslik: {g.Baslik}, Kilitli: {g.Kilitli}, Kullanici_ID: {g.Kullanici_ID}, CreDate: {g.CreDate}, Aktif: {g.Aktif}, Tamamlandi: {g.Tamamlandi}");
            }
            
            if (!sonGirisTarihi.HasValue)
            {
                // Hiç çıkış yapmamışsa (ilk giriş veya sürekli giriş), bugünden sonraki tüm aktif görevleri say
                // Kullanıcıya atanan görevler (Kilitli=true ve Kullanici_ID=kullanıcı) veya herkese açık görevler (Kilitli=false)
                var sql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" >= CURRENT_DATE
                      AND ((""Kilitli"" = true AND ""Kullanici_ID"" = @KullaniciId) OR ""Kilitli"" = false);";
                
                var result = await SQLQueryAsync<int>(sql, new { KullaniciId = kullaniciId });
                Console.WriteLine($"GetYeniGorevSayisiAsync (hiç çıkış yapılmamış) - Sonuç: {result.FirstOrDefault()}");
                return result.FirstOrDefault();
            }
            else
            {
                // Son çıkıştan sonra oluşturulan görevleri say
                // Kullanıcıya atanan görevler (Kilitli=true ve Kullanici_ID=kullanıcı) veya herkese açık görevler (Kilitli=false)
                // CreDate > sonGirisTarihi kullanıyoruz (>) çünkü çıkış yaptıktan SONRA eklenen görevler yeni sayılmalı
                var sql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" > @SonGirisTarihi
                      AND ((""Kilitli"" = true AND ""Kullanici_ID"" = @KullaniciId) OR ""Kilitli"" = false);";
                
                Console.WriteLine($"SQL Sorgusu çalıştırılıyor - Kullanıcı ID: {kullaniciId}, Son Çıkış: {sonGirisTarihi.Value:yyyy-MM-dd HH:mm:ss}");
                
                var result = await SQLQueryAsync<int>(sql, new { KullaniciId = kullaniciId, SonGirisTarihi = sonGirisTarihi.Value });
                var sayi = result.FirstOrDefault();
                Console.WriteLine($"GetYeniGorevSayisiAsync (son çıkış: {sonGirisTarihi.Value:yyyy-MM-dd HH:mm:ss}) - Sonuç: {sayi}");
                
                // Debug: Eğer 0 ise, detaylı kontrol yap - tüm görevleri kontrol et
                if (sayi == 0)
                {
                    var detaySql = @"
                        SELECT ""ID"", ""Baslik"", ""Kilitli"", ""Kullanici_ID"", ""CreDate"", ""Aktif"", ""Tamamlandi"", ""DelUser""
                        FROM ""Gorev""
                        WHERE ""Aktif"" = true
                          AND ""Tamamlandi"" = false
                          AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                          AND ((""Kilitli"" = true AND ""Kullanici_ID"" = @KullaniciId) OR ""Kilitli"" = false)
                        ORDER BY ""CreDate"" DESC
                        LIMIT 20;";
                    
                    var detayGorevler = await SQLQueryAsync<dynamic>(detaySql, new { KullaniciId = kullaniciId });
                    Console.WriteLine($"Detay kontrol - Toplam {detayGorevler.Count()} aktif görev bulundu (kilitli veya herkese açık):");
                    foreach (var g in detayGorevler)
                    {
                        if (g.CreDate != null)
                        {
                            var creDate = (DateTime)g.CreDate;
                            var yeniMi = creDate > sonGirisTarihi.Value;
                            Console.WriteLine($"  - ID: {g.ID}, Baslik: {g.Baslik}, Kilitli: {g.Kilitli}, Kullanici_ID: {g.Kullanici_ID}, CreDate: {creDate}, Aktif: {g.Aktif}, Tamamlandi: {g.Tamamlandi}, DelUser: {g.DelUser}, Yeni mi: {yeniMi}");
                        }
                        else
                        {
                            Console.WriteLine($"  - ID: {g.ID}, Baslik: {g.Baslik}, Kilitli: {g.Kilitli}, Kullanici_ID: {g.Kullanici_ID}, CreDate: NULL, Aktif: {g.Aktif}, Tamamlandi: {g.Tamamlandi}, DelUser: {g.DelUser}");
                        }
                    }
                }
                
                return sayi;
            }
        }

        public async Task<int> GetYeniGorevDetaylariOzel(int kullaniciId, DateTime? sonGirisTarihi)
        {
            if (!sonGirisTarihi.HasValue)
            {
                // Hiç çıkış yapmamışsa, bugünden sonraki görevleri say
                var ozelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" >= CURRENT_DATE
                      AND ""Kilitli"" = true
                      AND ""Kullanici_ID"" = @KullaniciId;";
                var ozelResult = await SQLQueryFirstOrDefaultAsync<int>(ozelSql, new { KullaniciId = kullaniciId });
                return ozelResult;
            }
            else
            {
                // Son çıkıştan sonra eklenen görevleri say
                var ozelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" > @SonGirisTarihi
                      AND ""Kilitli"" = true
                      AND ""Kullanici_ID"" = @KullaniciId;";
                var ozelResult = await SQLQueryFirstOrDefaultAsync<int>(ozelSql, new { KullaniciId = kullaniciId, SonGirisTarihi = sonGirisTarihi.Value });
                return ozelResult;
            }
        }
        
        public async Task<int> GetYeniGorevDetaylariGenel(int kullaniciId, DateTime? sonGirisTarihi)
        {
            if (!sonGirisTarihi.HasValue)
            {
                // Hiç çıkış yapmamışsa, bugünden sonraki görevleri say
                var genelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" >= CURRENT_DATE
                      AND ""Kilitli"" = false;";
                var genelResult = await SQLQueryFirstOrDefaultAsync<int>(genelSql, new { });
                return genelResult;
            }
            else
            {
                // Son çıkıştan sonra eklenen görevleri say
                var genelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" > @SonGirisTarihi
                      AND ""Kilitli"" = false;";
                var genelResult = await SQLQueryFirstOrDefaultAsync<int>(genelSql, new { SonGirisTarihi = sonGirisTarihi.Value });
                return genelResult;
            }
        }
        public async Task<(int toplam, int genel, int ozel)> GetYeniGorevDetaylariAsync(int kullaniciId, DateTime? sonGirisTarihi)
        {
            if (!sonGirisTarihi.HasValue)
            {
                // Hiç çıkış yapmamışsa, bugünden sonraki görevleri say
                var genelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" >= CURRENT_DATE
                      AND ""Kilitli"" = false;";

                var ozelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" >= CURRENT_DATE
                      AND ""Kilitli"" = true
                      AND ""Kullanici_ID"" = @KullaniciId;";

                var genelResult = await SQLQueryAsync<int>(genelSql, new { });
                var ozelResult = await SQLQueryAsync<int>(ozelSql, new { KullaniciId = kullaniciId });

                var genel = genelResult.FirstOrDefault();
                var ozel = ozelResult.FirstOrDefault();
                var toplam = genel + ozel;

                return (toplam, genel, ozel);
            }
            else
            {
                // Son çıkıştan sonra oluşturulan görevleri say
                var genelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" > @SonGirisTarihi
                      AND ""Kilitli"" = false;";

                var ozelSql = @"
                    SELECT COUNT(*)
                    FROM ""Gorev""
                    WHERE ""Aktif"" = true
                      AND ""Tamamlandi"" = false
                      AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                      AND ""CreDate"" > @SonGirisTarihi
                      AND ""Kilitli"" = true
                      AND ""Kullanici_ID"" = @KullaniciId;";

                var genelResult = await SQLQueryAsync<int>(genelSql, new { SonGirisTarihi = sonGirisTarihi.Value });
                var ozelResult = await SQLQueryAsync<int>(ozelSql, new { KullaniciId = kullaniciId, SonGirisTarihi = sonGirisTarihi.Value });

                var genel = genelResult.FirstOrDefault();
                var ozel = ozelResult.FirstOrDefault();
                var toplam = genel + ozel;

                return (toplam, genel, ozel);
            }
        }

        public async Task<ItemKullaniciLog[]> GetKullaniciLoglari(int? kullaniciId = null)
        {
            var sql = @"
                SELECT kl.""ID"", kl.""Kullanici_ID"", kl.""IslemTipi"", kl.""IslemTarihi"", kl.""Aciklama"",
                       kl.""CreUser"", kl.""CreDate"", kl.""UpdUser"", kl.""UpdDate"", kl.""DelUser"", kl.""DelDate""   
                FROM ""KullaniciLog"" kl
                WHERE (kl.""DelUser"" IS NULL OR kl.""DelUser"" = 0)";
            
            if (kullaniciId.HasValue)
            {
                sql += @" AND kl.""Kullanici_ID"" = @KullaniciId";
            }
            
            sql += @" ORDER BY kl.""IslemTarihi"" DESC";
            
            if (kullaniciId.HasValue)
            {
                return (await SQLQueryAsync<ItemKullaniciLog>(sql, new { KullaniciId = kullaniciId.Value })).ToArray();
            }
            else
            {
                return (await SQLQueryAsync<ItemKullaniciLog>(sql)).ToArray();
            }
        }

    }
}
