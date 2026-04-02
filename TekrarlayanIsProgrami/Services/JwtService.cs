using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TekrarlayanIsProgrami.Services
{
    public class JwtService
    {
        private readonly string _secretKey = "BuJwtTokenCokGizliBirAnahtar123!";
        private readonly string _issuer = "NotiTask";
        private readonly string _audience = "Users";

        /// <summary>
        /// Oturum çerezi ile eşleştirmek için JWT doğrular; başarılıysa e-posta (Sub) döner.
        /// </summary>
        public bool TryValidateToken(string token, out string? email)
        {
            email = null;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                }, out _);

                email = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return !string.IsNullOrEmpty(email);
            }
            catch
            {
                return false;
            }
        }

        //JWT (JSON Web Token) oluşturmak için kullanılır.
        public string GenerateToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(16),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
