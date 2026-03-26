namespace TekrarlayanIsProgrami.Models
{
    public class ItemGorevLoglari
    {
        public int ID { get; set; }
        public int Gorev_ID { get; set; }
        public string Baslik { get; set; } = "";
        public string IslemTipi { get; set; }
        public string AlanAdi { get; set; }
        public string EskiDeger { get; set; }
        public string YeniDeger { get; set; }
        public int IslemKullanici_ID { get; set; }
        public DateTime IslemTarih { get; set; }
    }
}
