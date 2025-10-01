using Microsoft.AspNetCore.Components.Authorization;
using Npgsql;

using System.Security.Claims;
using System.Threading.Tasks;
using TekrarlayanIsProgrami.Models;
using TekrarlayanIsProgrami.Repository;
namespace TekrarlayanIsProgrami.Services
{
    public class KimlikDogrulamaService : AuthenticationStateProvider
    {
        private ClaimsPrincipal anonymous=new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        private readonly KullaniciRepository _kullaniciRepository;

        public KimlikDogrulamaService(KullaniciRepository dbContext)
        {
            _kullaniciRepository = dbContext;
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_user ?? anonymous));
        }

        public async Task<GirisSonucu> GirisYapAsync(string eposta, string sifre)
        {
            var user=await _kullaniciRepository.GirisYapAsync(eposta, sifre);
            if (user != null)
            {
                return new GirisSonucu
                {
                    Basarili = true,
                    Mesaj = $"Hoşgeldiniz {user.Adi}!"
                };

               
            }
            return new GirisSonucu
            {
                Basarili  = false,
                Mesaj = "E-posta veya şifre hatalı!"
            };


        }

        public async Task<GirisSonucu> KayitOlAsync(ItemKullanici user)
        {
            // Şifre minimum 8 karakter kontrolü
            if (string.IsNullOrWhiteSpace(user.Sifre) || user.Sifre.Length < 8)
            {
                return new GirisSonucu
                {
                    Basarili = false,
                    Mesaj = "Şifre en az 8 karakter olmalıdır."
                };
            }

            try
            {
                bool kayitBasarili = await _kullaniciRepository.KayitOlAsync(user);

                if (kayitBasarili)
                {
                    return new GirisSonucu
                    {
                        Basarili = true,
                        Mesaj = "Kayıt Başarıyla Tamamlandı!"
                    };
                }
                else
                {
                    return new GirisSonucu
                    {
                        Basarili = false,
                        Mesaj = "Kayıt sırasında bir hata oluştu."
                    };
                }
            }
            catch (Exception ex)
            {
                return new GirisSonucu
                {
                    Basarili = false,
                    Mesaj = $"Kayıt sırasında bir hata oluştu: {ex.Message}"
                };
            }
        }

        public void CikisYap()
        {
            _user=anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
