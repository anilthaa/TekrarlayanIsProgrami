using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Services
{
    public class DataService : DBClass
    {
        public DataService(string connectionString) : base(connectionString)
        {
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
            return new(){ Data = item.ID };
        }

        public async Task<ItemGorev[]> GetGorevler()
        {
            return (await SQLQueryAsync<ItemGorev>($@"
                Select * From ""Gorev""
                Where ""DelDate"" Is Null Order By ""BaslangicTarihi""")).ToArray();
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
                item.ID = await SQLExecuteScalar<int>(@"INSERT INTO ""Gorev"" (""Kullanici_ID"", ""Baslik"", ""Kategori_ID"", ""OnemDerecesi"",
                ""BaslangicTarihi"",""BitisTarihi"",""TekrarlamaTipi"", ""Aktif"", ""Kilitli"", ""TekrarAraligi"", ""HaftaninGunu"",
                ""AyinGunu"", ""SonrakiCalismaTarihi"", ""CreUser"", ""CreDate"") 
                VALUES (@Kullanici_ID, @Baslik, @Kategori_ID, @OnemDerecesi, @BaslangicTarihi, @BitisTarihi, @TekrarlamaTipi, @Aktif, 
                @Kilitli, @TekrarAraligi, @HaftaninGunu, @AyinGunu, @SonrakiCalismaTarihi,@CreUser, now())
                 RETURNING ""ID""", item);
            }
            else if (!item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                await SQLExecute(@"UPDATE ""Gorev"" SET
                      ""Kullanici_ID""=@Kullanici_ID, ""Baslik""=@Baslik, ""Kategori_ID""=@Kategori_ID, 
                      ""OnemDerecesi""=@OnemDerecesi, ""BaslangicTarihi""=@BaslangicTarihi, ""BitisTarihi""=@BitisTarihi,
                      ""TekrarlamaTipi""=@TekrarlamaTipi, ""Aktif""=@Aktif, ""Kilitli""=@Kilitli, ""TekrarAraligi""=@TekrarAraligi,
                      ""HaftaninGunu""=@HaftaninGunu, ""AyinGunu""=@AyinGunu, ""SonrakiCalismaTarihi""=@SonrakiCalismaTarihi, ""UpdUser""=@UpdUser, ""UpdDate""=now()
                     WHERE ""ID"" = @ID", item);
            }
            else if (!item.ID.Equals(0) && !item.DelUser.Equals(0))
            {
                await SQLExecute(@"UPDATE ""Gorev"" 
                     SET ""DelUser"" = @DelUser, ""DelDate"" = now() 
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


    }
}
