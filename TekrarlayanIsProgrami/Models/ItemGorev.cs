using System.ComponentModel.DataAnnotations;

namespace TekrarlayanIsProgrami.Models
{
    public class ItemGorev:ItemBase
    {
        public int Kullanici_ID {  get; set; }
        [Required(ErrorMessage ="Bu Alan Zorunludur.")]
        public string Baslik {  get; set; } 
        public int Kategori_ID {  get; set; }
        public string? KategoriAdi { get; set; } 
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
        public bool Tamamlandi { get; set; }
        public int AtayanKisiID { get; set; }

    }
}
