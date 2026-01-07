using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TekrarlayanIsProgrami.Services
{
    public class UrlSigningService
    {
        private readonly string _secretKey = "BuUrlImzaCokGizliBirAnahtar123456789!@#";

        //verilen URL’yi parametrelerle birlikte imzalý hale getirir.
        public string SignUrl(string baseUrl, Dictionary<string, string> parameters)
        {
            var json = JsonSerializer.Serialize(parameters);
            
            var encrypted = Encrypt(json);
            
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}token={Uri.EscapeDataString(encrypted)}";
        }

        //SignUrl ile oluþturulmuþ imzalý URL’yi çözer ve içindeki parametreleri okur.
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

                var decrypted = Decrypt(token);
                if (string.IsNullOrEmpty(decrypted))
                    return result;

                result = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted) ?? new Dictionary<string, string>();
                isValid = true;

                return result;
            }
            catch
            {
                return result;
            }
        }

        //bir URL’nin geçerli bir imzalý token içerip içermediðini kontrol eder.
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

                var decrypted = Decrypt(token);
                return !string.IsNullOrEmpty(decrypted);
            }
            catch
            {
                return false;
            }
        }

        //düz metni AES ile þifreleyip URL-safe Base64 formatýnda döndürür.
        private string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_secretKey.Substring(0, 32));
            aes.IV = new byte[16]; 
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

        //Encrypt metodu ile þifrelenmiþ veriyi çözerek orijinal düz metni geri döndürür.
        private string Decrypt(string cipherText)
        {
            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText.Replace("-", "+").Replace("_", "/"));

                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_secretKey.Substring(0, 32));
                aes.IV = new byte[16]; 
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

