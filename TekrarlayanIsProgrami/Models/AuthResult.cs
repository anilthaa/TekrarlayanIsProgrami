namespace TekrarlayanIsProgrami.Models
{
    public class AuthResult
    {
        public bool Success { get; set; } //İşlemin başarılı olmadığını belirtir.
        public string? Message { get; set; } //İşlemin sonucu hakkında bilgi verir.
    }
}
