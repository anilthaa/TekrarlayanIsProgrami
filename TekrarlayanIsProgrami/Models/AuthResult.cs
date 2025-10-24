namespace TekrarlayanIsProgrami.Models
{
    public class AuthResult
    {
        public bool Basarili { get; set; } //İşlemin başarılı olmadığını belirtir.
        public string? Mesaj { get; set; } //İşlemin sonucu hakkında bilgi verir.
        public User? User { get; set; }
        public string Token { get; set; }
    }
}
