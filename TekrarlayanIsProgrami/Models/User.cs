using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TekrarlayanIsProgrami.Models
{
    public class User
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Bu Alan Zorunludur.")]
        public string Adi { get; set; }

        [Required(ErrorMessage = "Bu Alan Zorunludur.")]
        public string Soyadi { get; set; }

        [Required(ErrorMessage = "Bu Alan Zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen Geçerli E-posta Giriniz.")]
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
        public string ResetToken { get; set; }
        public DateTime ResetTokenExpiry { get; set; }
        public User Clone()
        {
            return new User
            {
                ID = this.ID,
                Adi = this.Adi,
                Soyadi = this.Soyadi,
                EPosta = this.EPosta,
                Sifre = this.Sifre
            };
        }

    }
}
