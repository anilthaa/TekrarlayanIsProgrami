using System.ComponentModel.DataAnnotations;

namespace TekrarlayanIsProgrami.Models
{
    public class ItemKategori:ItemBase
    {
        [Required(ErrorMessage ="Bu Alan Zorunludur.")]
        public string Adi { get; set; }
   
    }
}
