using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System;
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
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResult { Basarili = false, Mesaj = "ModelState hatası" });

            if (await _userRepository.EpostaVarMiAsync(model.EPosta))
                return BadRequest(new AuthResult { Basarili = false, Mesaj = "Bu e-posta zaten kayıtlı" });

            // Şifre hashleniyor
            string hashed = PasswordHelper.HashPassword(model.Sifre);

            var yeniUser = await _userRepository.KayitOlAsync(
                model.Adi,
                model.Soyadi,
                model.EPosta,
                hashed,           // ← Hash'i kaydediyoruz
                null,
                null,
                model.CreUser,
                model.KullaniciTip_ID
            );

            return Ok(new AuthResult { Basarili = true, Mesaj = "Kayıt başarılı" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userRepository.GetByGirisAsync(model.Email);
            if (user == null)
                return Unauthorized(new AuthResult { Basarili = false, Mesaj = "Kullanıcı bulunamadı" });

            // Hash doğrulaması
            bool ok = PasswordHelper.VerifyPassword(model.Password, user.Sifre);
            if (!ok)
                return Unauthorized(new AuthResult { Basarili = false, Mesaj = "Şifre yanlış" });

            var token = _jwtService.GenerateToken(user.EPosta);

            return Ok(new AuthResult
            {
                Basarili = true,
                Mesaj = "Giriş başarılı",
                Token = token,
                User = new User
                {
                    ID = user.ID,
                    Adi = user.Adi,
                    Soyadi = user.Soyadi,
                    EPosta = user.EPosta,
                    KullaniciTip_ID = user.KullaniciTip_ID
                }
            });
        }
    }
}
