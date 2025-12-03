using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TekrarlayanIsProgrami.Services
{
    public class UrlSigningService
    {
        private readonly string _secretKey = "BuUrlImzaCokGizliBirAnahtar123456789!@#"; // En az 32 karakter

        /// <summary>
        /// URL parametrelerini şifreleyip tek bir token olarak döndürür
        /// </summary>
        public string SignUrl(string baseUrl, Dictionary<string, string> parameters)
        {
            // Parametreleri JSON'a çevir
            var json = JsonSerializer.Serialize(parameters);
            
            // JSON'ı şifrele (AES)
            var encrypted = Encrypt(json);
            
            // URL'e sadece token'ı ekle
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}token={Uri.EscapeDataString(encrypted)}";
        }

        /// <summary>
        /// Şifrelenmiş token'dan parametreleri çıkarır
        /// </summary>
        public Dictionary<string, string> ParseSignedUrl(string url, out bool isValid)
        {
            isValid = false;
            var result = new Dictionary<string, string>();

            try
            {
                var uri = new Uri(url);
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                if (!query.ContainsKey("token"))
                    return result;

                var token = query["token"].ToString();
                if (string.IsNullOrEmpty(token))
                    return result;

                // Token'ı çöz
                var decrypted = Decrypt(token);
                if (string.IsNullOrEmpty(decrypted))
                    return result;

                // JSON'dan parametreleri al
                result = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted) ?? new Dictionary<string, string>();
                isValid = true;

                return result;
            }
            catch
            {
                return result;
            }
        }

        /// <summary>
        /// URL imzasını doğrular (token çözülebiliyorsa geçerli)
        /// </summary>
        public bool VerifyUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                if (!query.ContainsKey("token"))
                    return false;

                var token = query["token"].ToString();
                if (string.IsNullOrEmpty(token))
                    return false;

                // Token'ı çöz, başarılıysa geçerli
                var decrypted = Decrypt(token);
                return !string.IsNullOrEmpty(decrypted);
            }
            catch
            {
                return false;
            }
        }

        private string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_secretKey.Substring(0, 32)); // 32 byte key
            aes.IV = new byte[16]; // IV için secret key'den türet
            Array.Copy(Encoding.UTF8.GetBytes(_secretKey.Substring(16, 16)), 0, aes.IV, 0, 16);

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            var encrypted = msEncrypt.ToArray();
            return Convert.ToBase64String(encrypted).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private string Decrypt(string cipherText)
        {
            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText.Replace("-", "+").Replace("_", "/"));

                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_secretKey.Substring(0, 32)); // 32 byte key
                aes.IV = new byte[16]; // IV için secret key'den türet
                Array.Copy(Encoding.UTF8.GetBytes(_secretKey.Substring(16, 16)), 0, aes.IV, 0, 16);

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipherBytes);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}

