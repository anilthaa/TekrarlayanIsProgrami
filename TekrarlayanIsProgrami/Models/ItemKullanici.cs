using System.ComponentModel.DataAnnotations;

namespace TekrarlayanIsProgrami.Models
{
    public class ItemKullanici
    {
        public int ID { get; set; }
        public string? Adi { get; set; }
        public string? Soyadi { get; set; }
        public string? EPosta { get; set; }
        public int KullaniciTip_ID { get; set; }
        [Required]
        [StringLength(50, MinimumLength =8, ErrorMessage ="Şifre en az 8 karakter olmalı.")]
        public string Sifre { get; set; }
        public bool Aktif { get; set; }
        public int CreUser { get; set; }
        public DateTime CreDate { get; set; }
        public int UpdUser { get; set; }
        public DateTime UpdDate { get; set; }
        public int DelUser { get; set; }
        public DateTime DelDate { get; set; }
        public string? ResetToken {  get; set; }
        public DateTime ResetTokenExpiry {  get; set; }
    }
}
