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
using static System.Net.WebRequestMethods;
namespace TekrarlayanIsProgrami.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _Js;
        private User _currentUser;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        private readonly UserRepository _kullaniciRepo;
        private readonly HttpClient _http;

        public CustomAuthenticationStateProvider(UserRepository dbContext, IJSRuntime Js, HttpClient http)
        {
            _kullaniciRepo = dbContext;
            _Js = Js;
            _http = http;
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
        //Kullanıcının giriş yaptığını sisteme bildirmek.
        public async Task MarkUserAsAuthenticated(User user)
        {
            _currentUser = user;
            var json = JsonSerializer.Serialize(user);
            await _Js.InvokeVoidAsync("localStorage.setItem", "_currentUser", json);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        //Kullanıcının çıkış yaptığını sisteme bildirmek
        public async Task MarkUserAsLoggedOut()
        {
            _currentUser = null;
            await _Js.InvokeVoidAsync("localStorage.removeItem", "_currentUser");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        //
        public async Task LoadUserFromLocalStorageAsync()
        {
            var userJson = await _Js.InvokeAsync<string>("localStorage.getItem", "_currentUser");
            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonSerializer.Deserialize<User>(userJson);
                if (user != null)
                {
                    _currentUser = user;
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }
            }
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

                await MarkUserAsAuthenticated(Kullanici);

                return new AuthResult { Basarili = true, Mesaj=$"Hoşgeldiniz {Kullanici.Adi}" };
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
            //var kullanici =await _kullaniciRepo.KayitOlAsync(user.Adi, user.Soyadi, user.EPosta, user.Sifre, user.SifreTekrar);
            //if(kullanici==null)
            //{
            //    return new AuthResult
            //    {
            //        Basarili = false,
            //        Mesaj=$"Lütfen Bilgilerinizi Doldurunuz!"
            //    };
            //}
            if (await _kullaniciRepo.EpostaVarMiAsync(user.EPosta))
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj = $"Bu E-posta Zaten Kayıtlı!"
                };
            }
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
                return new AuthResult { Basarili = false, Mesaj = "Bilinmeyen bir hata oluştu." };
                
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

            if (user.Identity?.IsAuthenticated==true)
            {
                return new User
                {
                    ID = int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0,
                    Adi = user.FindFirst(ClaimTypes.Name)?.Value?.Split(' ').FirstOrDefault() ?? "",
                    Soyadi = user.FindFirst(ClaimTypes.Name)?.Value?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    EPosta = user.FindFirst(ClaimTypes.Email)?.Value ?? ""
                };
            }

           return null;
        }
        //Tarayıcıda kullanıcının token ı gelsin diye yazdık.
        public async Task<AuthResult> Login(string eposta, string sifre)
        {
            var loginData = new { Email = eposta, Password = sifre };
            var response = await _http.PostAsJsonAsync("api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                return result; // AuthResult içinde Token var
            }

            return null;
        }
        public async Task CikisYapAsync()
        {
            _currentUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            await _Js.InvokeVoidAsync("localStorage.removeItem", "_currentUser");
        }
    }
}
