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
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private User _currentUser;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        private readonly UserRepository _kullaniciRepo;

        public CustomAuthenticationStateProvider(UserRepository dbContext)
        {
            _kullaniciRepo = dbContext;
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity;
            if (_currentUser != null)
            {
                identity = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, _currentUser.ID.ToString()),
                new Claim(ClaimTypes.Name, _currentUser.Adi + " " + _currentUser.Soyadi),
                new Claim(ClaimTypes.Email, _currentUser.EPosta)
            }, "apiauth");
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
        public async Task<AuthResult> GirisYapAsync(string eposta ,string Sifre)
        {
            var Kullanici = await _kullaniciRepo.GetByEmailAsync(eposta);
            if (Kullanici != null)
            {
                return new AuthResult
                {
                    Basarili = true,
                    Mesaj = $"Hoşgeldiniz {Kullanici.Adi}!"
                };
            }
            var identity = new ClaimsIdentity(new[]
{
            new Claim(ClaimTypes.Name, Kullanici?.Adi + " " + Kullanici?.Soyadi),
            new Claim(ClaimTypes.Email, Kullanici.EPosta),
            new Claim(ClaimTypes.NameIdentifier, Kullanici.ID.ToString())
            }, "apiauth_type");

            var kullaniciPrincipal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(kullaniciPrincipal)));
            return new AuthResult
            {
                Basarili = false,
                Mesaj = "E-posta veya şifre hatalı!"
            };
        }

        public async Task<AuthResult> KayitOlAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Sifre) || user.Sifre.Length < 8)
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj = "Şifre en az 8 karakter olmalıdır."
                };
            }

            try
            {
                // Repo metodunu User döndürecek şekilde değiştir
                User kayitliUser = await _kullaniciRepo.KayitOlAsync(user.EPosta, user.Sifre, user.Adi, user.Soyadi);

                if (kayitliUser != null)
                {
                    return new AuthResult
                    {
                        Basarili = true,
                        Mesaj = "Kayıt Başarıyla Tamamlandı!"
                    };
                }
                else
                {
                    return new AuthResult
                    {
                        Basarili = false,
                        Mesaj = "Kayıt sırasında bir hata oluştu."
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj = $"Kayıt sırasında bir hata oluştu: {ex.Message}"
                };
            }
        }
            //// Şifre minimum 8 karakter kontrolü
            //if (string.IsNullOrWhiteSpace(user.Sifre) || user.Sifre.Length < 8)
            //{
            //    return new AuthResult
            //    {
            //        Basarili = false,
            //        Mesaj = "Şifre en az 8 karakter olmalıdır."
            //    };
            //}

            //try
            //{
            //    bool kayitBasarili = await _kullaniciRepo.KayitOlAsync(user);

            //    if (kayitBasarili)
            //    {
            //        return new AuthResult
            //        {
            //            Basarili = true,
            //            Mesaj = "Kayıt Başarıyla Tamamlandı!"
            //        };
            //    }
            //    else
            //    {
            //        return new AuthResult
            //        {
            //            Basarili = false,
            //            Mesaj = "Kayıt sırasında bir hata oluştu."
            //        };
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return new AuthResult
            //    {
            //        Basarili = false,
            //        Mesaj = $"Kayıt sırasında bir hata oluştu: {ex.Message}"
            //    };
            //}
        
        //public async Task<AuthResult> UpdateAsync(string E_Posta)
        //{
        //    var kullanici = await _kullaniciRepo.GetByEmailAsync(E_Posta);
        //    if (kullanici == null) { }

        //    // Yeni ClaimsIdentity oluştur
        //    var identity = new ClaimsIdentity(new[]
        //    {
        //    new Claim(ClaimTypes.NameIdentifier, kullanici.ID.ToString()),
        //    new Claim(ClaimTypes.Name, kullanici.Adi + " " + kullanici.Soyadi),
        //    new Claim(ClaimTypes.Email, kullanici.EPosta)
        //}, "apiauth");

        //    var userPrincipal = new ClaimsPrincipal(identity);

        //    // Authentication state’i güncelle
        //    AuthProvider.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(userPrincipal)));
        //    return new AuthResult { Basarili = true, Mesaj = "Giriş başarılı" };
        //}
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
