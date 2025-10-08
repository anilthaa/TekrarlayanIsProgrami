using Microsoft.AspNetCore.Components.Authorization;
using Npgsql;
using Radzen.Blazor.Markdown;
using System.Security.Claims;
using System.Threading.Tasks;
using TekrarlayanIsProgrami.Models;
using TekrarlayanIsProgrami.Repository;
using static Dapper.SqlMapper;
namespace TekrarlayanIsProgrami.Services
{
    public class CustomAuthenticationStateOrovider : AuthenticationStateProvider
    {
        private User _currentUser;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        private readonly KullaniciRepository _kullaniciRepo;
        private readonly CustomAuthenticationStateOrovider _girisSaglayici;

        public CustomAuthenticationStateOrovider(KullaniciRepository dbContext, CustomAuthenticationStateOrovider girisSaglayici)
        {
            _kullaniciRepo = dbContext;
            _girisSaglayici = girisSaglayici;
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity kimlik;
            if (_currentUser != null)
            {
                kimlik = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, _currentUser.ID.ToString()),
                new Claim(ClaimTypes.Name, _currentUser.Adi + " " + _currentUser.Soyadi),
                new Claim(ClaimTypes.Email, _currentUser.EPosta)
            }, "apiauth");
            }
            else
            {
                kimlik = new ClaimsIdentity();
            }

            var user = new ClaimsPrincipal(kimlik);
            return Task.FromResult(new AuthenticationState(user));
        }
        public async Task<GirisSonucu> GirisYapAsync(string eposta, string sifre)
        {
            var Kullanici = await _kullaniciRepo.GetByEmailAndPasswordAsync(eposta,sifre);
            if (Kullanici != null)
            {
                return new GirisSonucu
                {
                    Basarili = true,
                    Mesaj = $"Hoşgeldiniz {Kullanici.Adi}!"
                };
            }
            var kimlik = new ClaimsIdentity(new[]
{
            new Claim(ClaimTypes.Name, Kullanici?.Adi + " " + Kullanici?.Soyadi),
            new Claim(ClaimTypes.Email, Kullanici.EPosta),
            new Claim(ClaimTypes.NameIdentifier, Kullanici.ID.ToString())
            }, "apiauth_type");

            var kullaniciPrincipal = new ClaimsPrincipal(kimlik);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(kullaniciPrincipal)));
            return new GirisSonucu
            {
                Basarili = false,
                Mesaj = "E-posta veya şifre hatalı!"
            };
        }

        public async Task<GirisSonucu> KayitOlAsync(User user)
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
                bool kayitBasarili = await _kullaniciRepo.KayitOlAsync(user);

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
        public async Task<GirisSonucu> UpdateAsync(string E_Posta,string Sifre)
        {
            var kullanici = await _kullaniciRepo.GetByEmailAndPasswordAsync(E_Posta ,Sifre);
            if (kullanici == null) { }

            // Yeni ClaimsIdentity oluştur
            var identity = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, kullanici.ID.ToString()),
            new Claim(ClaimTypes.Name, kullanici.Adi + " " + kullanici.Soyadi),
            new Claim(ClaimTypes.Email, kullanici.EPosta)
        }, "apiauth");

            var userPrincipal = new ClaimsPrincipal(identity);

            // Authentication state’i güncelle
            _girisSaglayici.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(userPrincipal)));
            return new GirisSonucu { Basarili = true, Mesaj = "Giriş başarılı" };
        }
        public Task CikisYapAsync()
        {
            _currentUser = null;
            _user = anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.CompletedTask;
        }


        //public Task GuncelleAsync(ClaimsPrincipal user)
        //   {
        //       _currentUser = user;
        //       NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        //       return Task.CompletedTask;
        //   }
    }
}
