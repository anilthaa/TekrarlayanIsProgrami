using System.ComponentModel.DataAnnotations;

namespace TekrarlayanIsProgrami.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Adi { get; set; }
        public string Soyadi { get; set; }
        public string EPosta { get; set; }
        public int KullaniciTip_ID { get; set; }
        public string Sifre { get; set; }
        public string SifreTekrar { get; set; }
        public bool Aktif { get; set; }
        public int CreUser { get; set; }
        public DateTime CreDate { get; set; }
        public int UpdUser { get; set; }
        public DateTime UpdDate { get; set; }
        public int DelUser { get; set; }
        public DateTime DelDate { get; set; }
        public string ResetToken {  get; set; }
        public DateTime ResetTokenExpiry {  get; set; }
    }
}
