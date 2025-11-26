using System.Security.Cryptography;

namespace TekrarlayanIsProgrami.Services
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string Sifre, string stored)
        {
            var parts = stored.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);

            var pbkdf2 = new Rfc2898DeriveBytes(Sifre, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] newHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(storedHash, newHash);
        }
    }
}
