using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Services
{
    public class DataService : DBClass
    {
        public async Task<ItemKategori[]> GetKategori(int ID = 0)
        {
            return (await SQLQueryAsync<ItemKategori>($@"select ""ID"",""Adi"" from ""Kategori""         
        where ""DelUser"" is null 
        {(ID > 0 ? $@"and ""ID"" = {ID}" : "")} 
        ORDER BY ""ID"" ")).ToArray();
        }
        public DataResult<int> SetKategori(ItemKategori item)
        {
            if (item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                item.ID = SQLExecuteScalar<int>(@"INSERT INTO ""Kategori"" (""Adi"", ""CreUser"", ""CreDate"") VALUES (@Adi, @CreUser, now()) RETURNING ""ID""", item);
            }
            else if (!item.ID.Equals(0) && item.DelUser.Equals(0))
            {
                SQLExecute(@"update ""Kategori"" set ""Adi"" = @Adi, ""UpdUser"" = @UpdUser, ""UpdDate"" = now() where ""ID"" = @ID", item);
            }
            else if (!item.ID.Equals(0) && !item.DelUser.Equals(0))
            {
                SQLExecute(@"update ""Sehir"" set ""DelUser"" = @DelUser, ""DelDate"" = now() where ""ID"" = @ID", item);
            }
            return new() { Data = item.ID };
        }
    }
}
