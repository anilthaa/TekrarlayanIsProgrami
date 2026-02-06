namespace TekrarlayanIsProgrami.Models
{
    public class ItemForm : ItemBase
    {
        public string Adi { get; set; } = string.Empty;
        public bool IsMenu { get; set; }
        public int Sira { get; set; }
        public string? SayfaURL { get; set; }
        public string? Icon { get; set; }
        public int? UstMenu_ID { get; set; }
        public List<int> YetkiliRol_ID { get; set; } = new();
        public List<int> YetkiliKullanicilar { get; set; } = new();
        public List<ItemFormRol> FormRoller { get; set; } = new List<ItemFormRol>();

    }
}
