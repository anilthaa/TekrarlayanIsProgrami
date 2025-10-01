using Microsoft.AspNetCore.Components.Authorization;
using Npgsql;

using System.Security.Claims;
using System.Threading.Tasks;
using TekrarlayanIsProgrami.Models;
using TekrarlayanIsProgrami.Repository;
namespace TekrarlayanIsProgrami.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal anonymous=new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        private readonly KullaniciRepository _kullaniciRepository;

        public CustomAuthenticationStateProvider(KullaniciRepository dbContext)
        {
            _kullaniciRepository = dbContext;
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_user ?? anonymous));
        }

        public async Task<AuthResult> GirisYapAsync(string eposta, string sifre)
        {
            var user=await _kullaniciRepository.GirisYapAsync(eposta, sifre);
            if (user != null)
            {
                return new AuthResult
                {
                    Success = true,
                    Message = $"Hoşgeldiniz {user.Adi}!"
                };

               
            }
            return new AuthResult
            {
                Success = false,
                Message = "E-posta veya şifre hatalı!"
            };


        }

        public async Task<AuthResult> KayitOlAsync(ItemKullanici user)
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
                bool kayitBasarili = await _kullaniciRepository.KayitOlAsync(user);

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

        public void CikisYap()
        {
            _user=anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
