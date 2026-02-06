namespace TekrarlayanIsProgrami.Models
{
    /// <summary>
    /// Kullanıcı tipine göre form/sayfa yetkisi (Form_ID + Yetki).
    /// Bu tipi taşıyan tüm kullanıcılar ilgili sayfaya tanımlanan yetki ile erişir.
    /// </summary>
    public class ItemKullaniciTipDetay : ItemBase
    {
        public int KullaniciTip_ID { get; set; }
        public int Form_ID { get; set; }
        public int Yetki { get; set; }
    }
}
