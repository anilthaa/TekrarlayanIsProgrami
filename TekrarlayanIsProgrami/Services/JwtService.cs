using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TekrarlayanIsProgrami.Services
{
    public class JwtService
    {
        private readonly string _gizliAnahtar = "BuJwtTokenCokGizliBirAnahtar123!"; //Secret key en az 32 karakter olmalı.
        private readonly string _yetkiVeren = "NotiTask";        // Token’ı veren
        private readonly string _hedefKitle = "Users"; // Token’ı kullanacak olan// 1️⃣ Token oluşturma metodu
        public string GenerateToken(string username)
        {
            var GuvenlikAnahtari = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_gizliAnahtar));
            var KimlikBilgi = new SigningCredentials(GuvenlikAnahtari, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _yetkiVeren,
                audience: _hedefKitle,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: KimlikBilgi
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 2️⃣ Token doğrulama metodu
        public bool ValidateToken(string token)
        {
            var tokenİsleyici = new JwtSecurityTokenHandler();
            var anahtar = Encoding.UTF8.GetBytes(_gizliAnahtar);

            try
            {
                tokenİsleyici.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _yetkiVeren,
                    ValidAudience = _hedefKitle,
                    IssuerSigningKey = new SymmetricSecurityKey(anahtar)
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
