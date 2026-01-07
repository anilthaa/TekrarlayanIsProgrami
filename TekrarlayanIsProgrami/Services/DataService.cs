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

        //Silinmemiş kategorileri, opsiyonel olarak tek bir ID için filtreleyip, ItemKategori[] olarak döndüren metod.
        public async Task<ItemKategori[]> GetKategori(int ID = 0)
        {
            return (await SQLQueryAsync<ItemKategori>($@"select ""ID"",""Adi"" from ""Kategori""         
        where ""DelUser"" is null 
        {(ID > 0 ? $@"and ""ID"" = {ID}" : "")} 
        ORDER BY ""ID"" ")).ToArray();
        }

        //Tek bir metod ile kategori ekleme, düzenleme ve silme işlemlerini yönetir ve ID döndürür.
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

        //Tüm aktif görevleri, kategorisi ile birlikte listeleyen ve ItemGorev[] olarak döndüren metod.
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

        //görev ekleme, düzenleme ve silme işlemleri yapılır ve görev ID’si döndürülür.
        public async Task<DataResult<int>> SetGorev(ItemGorev item)
        {
            if (item.ID.Equals(0) && item.DelUser.Equals(0))
            {
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
                item.DelDate = DateTime.Now;
                
                await SQLExecute(@"UPDATE ""Gorev"" 
                     SET ""DelUser"" = @DelUser, ""DelDate"" = @DelDate 
                     WHERE ""ID"" = @ID", item);
            }
            return new() { Data = item.ID };
        }

        //kullanıcı tiplerini veritabanından getirir.
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

        //kullanıcı tipleri eklenir, güncellenir veya silinir ve ID döndürülür.
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

        //form (sayfa/menu) bilgilerini veritabanından getirir.
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

        //belirli bir kullanıcının form bazlı yetkilerini veritabanından getirir.
        public async Task<ItemKullaniciDetay[]> GetKullaniciFormYetkileriAsync(int kullaniciId)
        {
            var sql = @"
                select ""ID"", ""Form_ID"", ""Yetki"", ""Kullanici_ID"", ""CreUser"", ""CreDate"", ""UpdUser"", ""UpdDate"", ""DelUser"", ""DelDate""
                from ""KullaniciDetay""
                where ""Kullanici_ID"" = @KullaniciId and (""DelUser"" is null or ""DelUser"" = 0);
            ";

            return (await SQLQueryAsync<ItemKullaniciDetay>(sql, new { KullaniciId = kullaniciId })).ToArray();
        }

        //kullanıcının form bazlı yetkilerini ekleme, güncelleme ve silme (soft delete) işlemlerini yönetir.
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

        //kullanıcı işlemlerini loglamak (kaydetmek) için kullanılır.
        public async Task<DataResult<int>> SetKullaniciLog(ItemKullaniciLog item)
        {
            try
            {                
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
                                
                return new() { Data = item.ID };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //belirli bir kullanıcının son giriş tarihini almak için kullanılır.
        public async Task<DateTime?> GetSonGirisAsync(int kullaniciId)
        {
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
            
            var tumGirisler = await SQLQueryAsync<DateTime?>(@"
                SELECT ""IslemTarihi""
                FROM ""KullaniciLog""
                WHERE ""Kullanici_ID"" = @KullaniciId
                  AND ""IslemTipi"" = 'Giris'
                  AND (""DelUser"" IS NULL OR ""DelUser"" = 0)
                ORDER BY ""IslemTarihi"" DESC
                LIMIT 5;", new { KullaniciId = kullaniciId });
            
            foreach (var giris in tumGirisler)
            {

            }            
            return sonGiris;
        }

        //belirli bir kullanıcıya ait özel görevlerin sayısını alır.
        public async Task<int> GetYeniGorevDetaylariOzel(int kullaniciId, DateTime? sonGirisTarihi)
        {
            if (!sonGirisTarihi.HasValue)
            {
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

        //genel (kilitsiz) görevlerin sayısını almak için kullanılır.
        public async Task<int> GetYeniGorevDetaylariGenel(int kullaniciId, DateTime? sonGirisTarihi)
        {
            if (!sonGirisTarihi.HasValue)
            {
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

        //tüm kullanıcıların veya tek bir kullanıcının işlem loglarını sıralı şekilde döndürür.
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
