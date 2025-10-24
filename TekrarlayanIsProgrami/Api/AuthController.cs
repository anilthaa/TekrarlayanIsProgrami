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
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            if (model.Email == "kamirbay18@gmail.com" && model.Password == "11447788k")
            {
                var token = GenerateJwtToken(model.Email);
                return Ok(new AuthResult { Basarili = true, Token = token });
            }

            return Unauthorized(new AuthResult { Basarili = false, Mesaj = "Geçersiz giriş" });
        }

        private string GenerateJwtToken(string email)
        {
            // Burada JWT üretme mantığını yaz
            return "örnek-jwt-token";
        }

    }
}
