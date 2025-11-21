namespace TekrarlayanIsProgrami.Models
{
    public class ItemForm : ItemBase
    {
        public string Adi { get; set; } = string.Empty;
        public bool IsMenu { get; set; }
        public int Sira { get; set; }
        public string? SayfaURL { get; set; }
        public string? Icon { get; set; }
        
    }
}
