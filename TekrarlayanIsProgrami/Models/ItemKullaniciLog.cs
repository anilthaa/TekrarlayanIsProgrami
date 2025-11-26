namespace TekrarlayanIsProgrami.Models
{
    public class ItemKullaniciLog:ItemBase
    {
        public int Kullanici_ID { get; set; }
        public string IslemTipi { get; set; } = string.Empty;
        public DateTime IslemTarihi { get; set; }
        public string? Aciklama { get; set; }

    }
}
