using Microsoft.AspNetCore.Components.Authorization;
using Npgsql;
using Radzen;
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
            }, "apiauth_type");
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
        public async Task<AuthResult> GirisYapAsync(string eposta ,string sifre)
        {
            var Kullanici = await _kullaniciRepo.GetByEmailAsync(eposta,sifre);
            if (Kullanici == null || Kullanici.Sifre!=sifre || Kullanici.EPosta!=eposta)
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj = $"E-posta veya Şifre Hatalı!"
                };
            }
            else
            {
                var identity = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, Kullanici.Adi + " " + Kullanici.Soyadi),
                new Claim(ClaimTypes.Email, Kullanici.EPosta),
                new Claim(ClaimTypes.NameIdentifier, Kullanici.ID.ToString())
                }, "apiauth_type");

                var kullaniciPrincipal = new ClaimsPrincipal(identity);//Blazor, AuthenticationStateProvider üzerinden principal ile kullanıcının giriş durumunu bilir.
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(kullaniciPrincipal)));//Bu satır, uygulamanın diğer bileşenlerine kullanıcının artık giriş yaptığını bildirir.
                return new AuthResult
                {
                    Basarili = true,
                    Mesaj = $"Hoşgeldiniz {Kullanici.Adi}!"
                };
            }
        }

        public async Task<AuthResult> KayitOlAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Sifre) || user.Sifre.Length < 8)
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj =$"Şifre en az 8 karakter olmalıdır."
                };
            }
                try
                {
                    // Repo metodunu User döndürecek şekilde dönüyor.
                    User kayitliUser = await _kullaniciRepo.KayitOlAsync(user.Adi, user.Soyadi, user.EPosta, user.Sifre);

                if (user.Sifre != user.SifreTekrar || user.EPosta == null || user.Sifre == null || user.SifreTekrar == null)
                {
                    return new AuthResult
                    {
                        Basarili = false,
                        Mesaj = "Kayıt sırasında bir hata oluştu.Lütfen Bilgilerinizi Kontrol Ediniz."
                    };
                }
                else if (kayitliUser != null)
                    {
                        return new AuthResult
                        {

                            Basarili = true,
                            Mesaj = "Kayıt Başarıyla Tamamlandı! Lütfen Giriş Yapın."
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
            return default;
        }

        public async Task<AuthResult> UpdateAsync(User user)
        {
            var kullanici = await _kullaniciRepo.GetByEmailAsync(user.EPosta,user.Sifre);
            if (kullanici == null) 
            {
            }

            // Yeni ClaimsIdentity oluştur
            var identity = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, kullanici.ID.ToString()),
            new Claim(ClaimTypes.Name, kullanici.Adi + " " + kullanici.Soyadi),
            new Claim(ClaimTypes.Email, kullanici.EPosta)
        }, "apiauth");
            return new AuthResult {
                Basarili = true, Mesaj = "Giriş başarılı" 
            };
        }

        //    var userPrincipal = new ClaimsPrincipal(identity);
        //    // Authentication state’i güncelle
        //   AuthenticationStateProvider.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(userPrincipal)));
        public async Task<User?> GetCurrentUserAsync() //Bu metot şu anda giriş yapmış olan kullanıcıyı sistemden bulur ve döndürür.
        {
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                return new User
                {
                    ID = int.TryParse(user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0,
                    Adi = user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value?.Split(' ').FirstOrDefault() ?? "",
                    Soyadi = user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    EPosta = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value ?? ""
                };
            }

           return null;
        }
        public Task CikisYapAsync()
        {
            _currentUser = null;
            _user = anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.CompletedTask;
        }
    }
}
