using System.ComponentModel.DataAnnotations;

namespace TekrarlayanIsProgrami.Models
{
    public class User
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Bu Alan Zorunludur.")]
        public string Adi { get; set; }

        [Required(ErrorMessage = "Bu Alan Zorunludur.")]
        public string Soyadi { get; set; }

        [Required(ErrorMessage="Bu Alan Zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        public string EPosta { get; set; }
        public int KullaniciTip_ID { get; set; }

        [Required(ErrorMessage = "Bu Alan Zorunludur.")]
        public string Sifre { get; set; }

        [Required(ErrorMessage = "Bu Alan Zorunludur.")]
        public string SifreTekrar { get; set; }

        public bool Aktif { get; set; }
        public int CreUser { get; set; }
        public DateTime CreDate { get; set; }
        public int UpdUser { get; set; }
        public DateTime UpdDate { get; set; }
        public int DelUser { get; set; }
        public DateTime DelDate { get; set; }
        public string? ResetToken {  get; set; }
        public DateTime? ResetTokenExpiry {  get; set; }
    }
}
