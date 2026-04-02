using System.ComponentModel.DataAnnotations;

namespace TekrarlayanIsProgrami.Models
{
    public class ItemKullaniciTip:ItemBase
    {
        [Required (ErrorMessage ="Bu Alan Zorunludur.")]
        public string Adi { get; set; }
        public bool Aktif { get; set; }

        // Kullanici tipleri eklenirken bir tanesi varsayilan olarak isaretlenebilir.
        // Kayit sirasinda kullanici tip id bos kalirsa bu tip atanir.
        public bool Varsayilan { get; set; }
    }
}
