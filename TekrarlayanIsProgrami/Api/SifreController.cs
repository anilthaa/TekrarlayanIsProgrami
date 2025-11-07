using Microsoft.AspNetCore.Mvc;
using TekrarlayanIsProgrami.Repository;
using TekrarlayanIsProgrami.Services;

namespace TekrarlayanIsProgrami.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SifreController : ControllerBase
    {
        private readonly EPostaService _epostaService;
        private readonly UserRepository _kullaniciRepo;

        public SifreController(EPostaService epostaService, UserRepository kullaniciRepo)
        {
            _epostaService = epostaService;
            _kullaniciRepo = kullaniciRepo;
        }

        [HttpPost("sifre-unuttum")]
        public async Task<IActionResult> SifreUnuttum(string eposta)
        {
            var kullanici = await _kullaniciRepo.SifreEmailAsync(eposta);
            if (kullanici == null)
                return BadRequest("Bu e-posta adresiyle kayıtlı kullanıcı bulunamadı.");

            var token = Guid.NewGuid().ToString();
            await _kullaniciRepo.SifreGuncelle( token,kullanici.Sifre);

            await _epostaService.SendResetEpostaAsync(kullanici.EPosta, token, kullanici.Adi);

            return Ok("Şifre sıfırlama bağlantısı e-postanıza gönderildi.");
        }
    }
}
