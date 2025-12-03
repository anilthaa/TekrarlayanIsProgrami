using System.ComponentModel.DataAnnotations;

namespace TekrarlayanIsProgrami.Models
{
    public class ItemKullaniciTip:ItemBase
    {
        [Required (ErrorMessage ="Bu Alan Zorunludur.")]
        public string Adi { get; set; }
        public bool Aktif { get; set; }
    }
}
