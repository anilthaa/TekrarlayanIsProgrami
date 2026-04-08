using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using TekrarlayanIsProgrami.Models;
using TekrarlayanIsProgrami.Repository;
using TekrarlayanIsProgrami.Services;

namespace TekrarlayanIsProgrami.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserRepository _userRepository;
        public AuthController(JwtService jwtService, UserRepository userRepository)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Tarayıcıya auth çerezi yazar; Blazor ilk render'da HttpContext üzerinden kullanıcıyı okuyabilir (localStorage/JS gerekmez).
        /// </summary>
        [HttpPost("establish-session")]
        public async Task<IActionResult> EstablishSession([FromBody] EstablishSessionRequest model)
        {
            if (model?.User == null || string.IsNullOrWhiteSpace(model.Token))
                return BadRequest();

            if (!_jwtService.TryValidateToken(model.Token, out var email) || email == null)
                return Unauthorized();

            if (!string.Equals(email.Trim(), model.User.EPosta?.Trim(), StringComparison.OrdinalIgnoreCase))
                return Unauthorized();

            var eposta = model.User.EPosta ?? email;
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, model.User.ID.ToString()),
                new Claim(ClaimTypes.Name, (model.User.Adi ?? "") + " " + (model.User.Soyadi ?? "")),
                new Claim(ClaimTypes.Email, eposta),
                new Claim("KullaniciTip_ID", model.User.KullaniciTip_ID.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "Identity.Application");

            await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(identity));
            return Ok();
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SignOutSession()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return Ok();
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResult { Basarili = false, Mesaj = "ModelState hatası" });

            if (await _userRepository.EpostaVarMiAsync(model.EPosta))
                return BadRequest(new AuthResult { Basarili = false, Mesaj = "Bu e-posta zaten kayıtlı" });

            var token = _jwtService.GenerateToken(model.EPosta);
            
            var tokenToStore = !string.IsNullOrEmpty(token) && token.Length > 100 ? token.Substring(0, 100) : token;

            var yeniUser = await _userRepository.KayitOlAsync(model);

            return Ok(new AuthResult
            {
                Basarili = true,
                Mesaj = "Kayıt başarılı",
                Token = token,
                User = new User
                {
                    ID = yeniUser.ID,
                    Adi = yeniUser.Adi,
                    Soyadi = yeniUser.Soyadi,
                    EPosta = yeniUser.EPosta,
                    KullaniciTip_ID = yeniUser.KullaniciTip_ID
                }
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userRepository.GetByGirisAsync(model.Email);
            if (user == null)
                return Unauthorized(new AuthResult { Basarili = false, Mesaj = "Kullanıcı bulunamadı" });

            bool ok = PasswordHelper.VerifyPassword(model.Password, user.Sifre);
            if (!ok)
                return Unauthorized(new AuthResult { Basarili = false, Mesaj = "Şifre yanlış" });

            var token = _jwtService.GenerateToken(user.EPosta);

            // Başarılı girişte sunucu tarafı auth cookie yazılır.
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, (user.Adi ?? "") + " " + (user.Soyadi ?? "")),
                new Claim(ClaimTypes.Email, user.EPosta ?? ""),
                new Claim("KullaniciTip_ID", user.KullaniciTip_ID.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "Identity.Application");
            await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(identity));

            return Ok(new AuthResult
            {
                Basarili = true,
                Mesaj = "Giriş başarılı",
                Token = token,
                User = new User
                {
                    ID = user.ID,
                    Adi = user.Adi ?? "",
                    Soyadi = user.Soyadi ?? "",
                    EPosta = user.EPosta ?? "",
                    KullaniciTip_ID = user.KullaniciTip_ID
                }
            });
        }
    }

    public class EstablishSessionRequest
    {
        public string Token { get; set; } = "";
        public User User { get; set; } = null!;
    }
}
