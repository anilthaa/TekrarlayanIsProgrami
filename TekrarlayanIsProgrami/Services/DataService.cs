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
        public DataResult<int> SetKategori(ItemKategori item)
        {
            if (item.ID != 0 && item.DelUser != 0)
            {
                SQLExecute(@"UPDATE ""Kategori"" 
                     SET ""DelUser"" = @DelUser, ""DelDate"" = now() 
                     WHERE ""ID"" = @ID", item);
            }
            else if (item.ID == 0)
            {
                var newId = SQLExecuteScalar<int>(
                    @"INSERT INTO ""Kategori"" (""Adi"", ""CreUser"", ""CreDate"") 
              VALUES (@Adi, @CreUser, now()) 
              RETURNING ""ID""", item);
                item.ID = newId;
            }
            else
            {
                SQLExecute(@"UPDATE ""Kategori"" 
                     SET ""Adi"" = @Adi, ""UpdUser"" = @UpdUser, ""UpdDate"" = now() 
                     WHERE ""ID"" = @ID", item);
            }
            return new DataResult<int> { Data = item.ID };
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
                And ""SonrakiCalismaTarihi>=NOW() Order By ""SonrakiCalismaTarihi"" Asc")).ToArray();
        }

        public async Task<ItemGorev[]> GecmisGorevlerAdmin()
        {
            return (await SQLQueryAsync<ItemGorev>($@"
                Select * From ""Gorev""
                Where ""DelDate"" Is Null
                And ""BitisTarihi"" <NOW()
                Order By ""BitisTarihi"" DESC")).ToArray();
        }

        public DataResult<int> SetGorev(ItemGorev gorev)
        {
            var result = new DataResult<int> { SonucKodu = -1 };

            try
            {
                if (gorev.ID!=0 && gorev.DelUser!=0)
                {
                    SQLExecute(@"
                        Update ""Gorev"" Set ""DelUser""=@DelUser,
                        ""DelDate""=NOW(), ""Aktif""=false
                        Where ""ID""=@ID And ""DelDate"" Is Null", gorev);
                }
                else if(gorev.ID==0)
                {
                    var newId = SQLExecuteScalar<int>(@"
                        Insert Into ""Gorev"" (""Kullanici_ID"",""Baslik"", ""Kategori_ID"",
                        ""OnemDerecesi"", ""BaslangicTarihi"",
                        ""BitisTarihi"", ""TekrarlamaTipi"",
                        ""Aktif"",""Kilitli"",""TekrarAraligi"",
                        ""HaftaninGunu"",""AyinGunu"",""SonrakiCalismaTarihi"",
                        ""CreUser"",""CreDate"") Values
                        (@Kullanici_ID, @Baslik, @Kategori_ID, @OnemDerecesi,
                        @BaslangicTarihi, @BitisTarihi, @TekrarlamaTipi,
                        @Aktif, @Kilitli, @TekrarAraligi, @HaftaninGunu,
                        @AyinGunu, @SonrakiCalismaTarihi, @CreUser,
                        NOW()) RETURNING ""ID""", gorev);

                    gorev.ID = newId;
                }
                else
                {
                    SQLExecute(@"
                        Update ""Gorev"" Set
                        ""Kullanici_ID""=@Kullanici_ID,
                        ""Baslik""=@Baslik,
                        ""Kategori_ID""=@Kategori_ID,
                        ""OnemDerecesi""=@OnemDerecesi,
                        ""BaslangicTarihi""=@BaslangicTarihi"",
                        ""BitisTarihi""=@BitisTarihi,
                        ""TekrarlamaTipi""=@TekrarlamaTipi,
                        ""Aktif""=@Aktif,
                        ""Kilitli""=@Kilitli,
                        ""TekrarAraligi""=@TekrarAraligi,
                        ""HaftaninGunu""=@HaftaninGunu,
                        ""AyinGunu""=@AyinGunu,
                        ""SonrakiCalismaTarihi""=@SonrakiCalismaTarihi,
                        ""UpdUser""=@UpdUser,
                        ""UpdDate""=NOW()
                    Where ""ID""=@ID And ""DelDate"" Is Null", gorev);
                }
                result.SonucKodu = 0;
                result.Data=gorev.ID;
            }
            catch (Exception ex)
            {
                result.SonucKodu=-1;
                result.SonucAciklama = ex.Message;
            }
            return result;
        }

        
    }
}
