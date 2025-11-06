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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userRepository.GetByEmailAsync(model.Email, model.Password);
            if (user == null)
            {
                return Unauthorized(new AuthResult { Basarili = false, Mesaj = "Geçersiz giriş bilgileri" });
            }

            // Güvenlik: parola alanlarını asla dışarı döndürmeyelim
            user.Sifre = null;
            user.SifreTekrar = null;

            var token = _jwtService.GenerateToken(user.EPosta);
            return Ok(new AuthResult
            {
                Basarili = true,
                Mesaj = "Giriş Başarılı",
                Token = token,
                User=user
            });

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(kvp => kvp.Value.Errors.Count > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new
                {
                    Basaarili = false,
                    Mesaj="Modelstate hatası",
                    Errors= errors
                });
            }

            if (await _userRepository.EpostaVarMiAsync(model.EPosta))
            {
                return BadRequest(new AuthResult { Basarili = false, Mesaj = "Bu e-posta zaten kayıtlı" });
            }

            var yeniUser = await _userRepository.KayitOlAsync(model.Adi, model.Soyadi, model.EPosta, model.Sifre, model.SifreTekrar, ResetToken: null);

            var token = _jwtService.GenerateToken(model.EPosta);
            await _userRepository.SaveResetTokenAsync(yeniUser.ID, token);


            return Ok(new AuthResult
            {
                Basarili=true,
                Mesaj="Kayıt başarılı",
                Token = token
            });
        }


        //private string GenerateJwtToken(string email)
        //{
        //    // Burada JWT üretme mantığını yaz
        //    return _jwtService.GenerateToken(email);
        //}


    }
}
