namespace TekrarlayanIsProgrami.Models
{
    public class AuthResult
    {
        public bool Basarili { get; set; } 
        public string? Mesaj { get; set; } 
        public User? User { get; set; }
        public string Token { get; set; }
    }
}
