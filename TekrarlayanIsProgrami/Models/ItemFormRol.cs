namespace TekrarlayanIsProgrami.Models
{
    public class ItemFormRol
    {
        public int ID { get; set; }          // Opsiyonel, Primary Key
        public int Menu_ID { get; set; }      // Menu tablosuna FK
        public int Rol_ID { get; set; }       // Kullanıcının rolü (0 = rolü olmayan kullanıcılar)

        // Navigation property
        public ItemForm Form { get; set; }
    }
}
