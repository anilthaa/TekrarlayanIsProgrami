namespace TekrarlayanIsProgrami.Models
{
    public class ItemGorev
    {
        public int ID { get; set; }
        public int? Kullanici_ID {  get; set; }
        public string? Baslik {  get; set; }
        public int Kategori_ID {  get; set; }
        public string? OnemDerecesi {  get; set; }
        public DateTime? BaslangicTarihi {  get; set; }
        public DateTime? BitisTarihi { get; set; }
        public string? TekrarlamaTipi {  get; set; }
        public bool Aktif {  get; set; }
        public bool Kilitli { get; set; }
        public int? TekrarAraligi {  get; set; }
        public string? HaftaninGunu {  get; set; }
        public int? AyinGunu {  get; set; }
        public DateTime? SonrakiCalismaTarihi { get; set; }
        public int? CreUser { get; set; }
        public DateTime? CreDate { get; set; }
        public int? UpdUser { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? DelUser { get; set; }
        public DateTime? DelDate { get; set; }
    }
}
