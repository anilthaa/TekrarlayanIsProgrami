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
        private readonly UserRepository _repo;

        public CustomAuthenticationStateProvider(UserRepository dbContext)
        {
            _repo = dbContext;
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
        public async Task<AuthResult> GirisYapAsync(string eposta, string sifre)
        {
            var Kullanici = await _repo.GetByEmailAsync(eposta);
            if (Kullanici != null)
            {
                return new AuthResult
                {
                    Success = true,
                    Message = $"Hoşgeldiniz {Kullanici.Adi}!"
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
            return new AuthResult
            {
                Success = false,
                Message = "E-posta veya şifre hatalı!"
            };
        }

        public async Task<AuthResult> KayitOlAsync(User user)
        {
            // Şifre minimum 8 karakter kontrolü
            if (string.IsNullOrWhiteSpace(user.Sifre) || user.Sifre.Length < 8)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Şifre en az 8 karakter olmalıdır."
                };
            }

            try
            {
                bool kayitBasarili = await _repo.KayitOlAsync(user);

                if (kayitBasarili)
                {
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Kayıt Başarıyla Tamamlandı!"
                    };
                }
                else
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Kayıt sırasında bir hata oluştu."
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Kayıt sırasında bir hata oluştu: {ex.Message}"
                };
            }
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
