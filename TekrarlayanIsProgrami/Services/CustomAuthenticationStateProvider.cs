using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Npgsql;
using Radzen;
using Radzen.Blazor.Markdown;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using TekrarlayanIsProgrami.Models;
using TekrarlayanIsProgrami.Repository;
using static Dapper.SqlMapper;
namespace TekrarlayanIsProgrami.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime Js;
        private User _currentUser;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        private readonly UserRepository _kullaniciRepo;

        public CustomAuthenticationStateProvider(UserRepository dbContext,IJSRuntime Js)
        {
            _kullaniciRepo = dbContext;
            this.Js = Js;
        }
        public override  Task<AuthenticationState> GetAuthenticationStateAsync()
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

        //Kullanıcının giriş yaptığını sisteme bildirmek.
        public async Task MarkUserAsAuthenticated(User user)
        {
            _currentUser = user;
            var json = JsonSerializer.Serialize(user);
            await Js.InvokeVoidAsync("localStorage.setItem", "_currentUser", json);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        //Kullanıcının çıkış yaptığını sisteme bildirmek
        public async Task MarkUserAsLoggedOut()
        {
            _currentUser = null;
            await Js.InvokeVoidAsync("localStorage.removeItem", "_currentUser");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
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
            var kullanici =await _kullaniciRepo.KayitOlAsync(user.Adi, user.Soyadi, user.EPosta, user.Sifre, user.SifreTekrar);
            if(kullanici==null)
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj=$"Lütfen Bilgilerinizi Doldurunuz!"
                };
            }
            else if (await _kullaniciRepo.EpostaVarMiAsync(user.EPosta))
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj = $"Bu E-posta Zaten Kayıtlı!"
                };
            }
            else if (string.IsNullOrWhiteSpace(user.Sifre) || user.Sifre.Length < 8)
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj =$"Şifre en az 8 karakter olmalıdır."
                };
            }
            else { 
                try
                {
                    // Repo metodunu User döndürecek şekilde dönüyor.
                    User kayitliUser = await _kullaniciRepo.KayitOlAsync(user.Adi, user.Soyadi, user.EPosta, user.Sifre, user.SifreTekrar);
                    if (kayitliUser != null)
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
