using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Npgsql;
using Radzen;
using Radzen.Blazor.Markdown;
using System.ComponentModel.DataAnnotations;
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
        private readonly NavigationManager _nav;

        public CustomAuthenticationStateProvider(UserRepository dbContext, IJSRuntime Js, HttpClient http,NavigationManager nav)
        {
            _kullaniciRepo = dbContext;
            _Js = Js;
            _http = http;
            _nav=nav;
        }

        //Blazor’da mevcut kullanıcıyı doğrulamak ve AuthenticationState oluşturmak için kullanılır.
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_currentUser == null)
            {
                try
                {
                    var userJson = await _Js.InvokeAsync<string>("localStorage.getItem", "_currentUser");
                    if (!string.IsNullOrEmpty(userJson))
                    {
                        _currentUser = JsonSerializer.Deserialize<User>(userJson);
                    }
                }
                catch
                {

                }
            }

            ClaimsIdentity identity;
            if (_currentUser != null)
            {
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _currentUser.ID.ToString()),
                    new Claim(ClaimTypes.Name, _currentUser.Adi + " " + _currentUser.Soyadi),
                    new Claim(ClaimTypes.Email, _currentUser.EPosta),
                    new Claim(ClaimTypes.Role, _currentUser.KullaniciTip_ID==1 ? "Admin" : "User")
                }, "apiauth_type");
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        //kullanıcıyı oturum açmış olarak işaretler.
        public async Task MarkUserAsAuthenticated(User user)
        {
            _currentUser = user;
            var json = JsonSerializer.Serialize(user);
            await _Js.InvokeVoidAsync("localStorage.setItem", "_currentUser", json);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        //kullanıcıyı çıkış yapmış olarak işaretler.
        public async Task MarkUserAsLoggedOut()
        {
            _currentUser = null;
            await _Js.InvokeVoidAsync("localStorage.removeItem", "_currentUser");
            await _Js.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        //tarayıcıdaki localStorage’dan kullanıcı bilgilerini yükler.
        public async Task LoadUserFromLocalStorageAsync()
        {
            var userJson = await _Js.InvokeAsync<string>("localStorage.getItem", "_currentUser");
            if (!string.IsNullOrEmpty(userJson))
            {
                _currentUser = JsonSerializer.Deserialize<User>(userJson);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }

        //kullanıcının giriş (login) işlemini gerçekleştirir ve sonucu döndürür.
        public async Task<AuthResult> GirisYapAsync(string EPosta, string Sifre)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login",
            new { Email = EPosta, Password = Sifre });

            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Login Response: {response.StatusCode} - {raw}");

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult { Basarili = false, Mesaj = "E-posta veya Şifre Hatalı." };
            }

            var result = JsonSerializer.Deserialize<AuthResult>(raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.Basarili)
            {
                return new AuthResult { Basarili = false, Mesaj = "Giriş başarısız." };
            }

            if (result.User == null)
            {
                var user = await _kullaniciRepo.GetByGirisAsync(EPosta);
                if (user != null)
                {
                    result.User = new User
                    {
                        ID = user.ID,
                        Adi = user.Adi,
                        Soyadi = user.Soyadi,
                        EPosta = user.EPosta,
                        KullaniciTip_ID = user.KullaniciTip_ID
                    };
                }
                else
                {

                }
            }

            await _Js.InvokeVoidAsync("localStorage.setItem", "jwtToken", result.Token);

            if (result.User != null)
            {
                await MarkUserAsAuthenticated(result.User);
                return new AuthResult { Basarili = true, Mesaj = $"Hoşgeldiniz {result.User.Adi}!", User = result.User, Token = result.Token };
            }
            else
            {
                return new AuthResult { Basarili = false, Mesaj = "Kullanıcı bilgisi alınamadı." };
            }
        }

        //kullanıcının kayıt (register) işlemini gerçekleştirir ve sonucu döndürür.
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
                var response = await _http.PostAsJsonAsync("api/auth/register", user);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<AuthResult>();
                    return error ?? new AuthResult { Basarili = false, Mesaj = "Kayıt başarısız." };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResult>();

                if (result != null && result.Basarili)
                {
                    return new AuthResult
                    {
                        Basarili = true,
                        Mesaj = "Kayıt başarıyla tamamlandı!",
                        User = result.User
                    };
                }

                return result ?? new AuthResult { Basarili = false, Mesaj = "Kayıt başarısız." };
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

        //Bu metot şu anda giriş yapmış olan kullanıcıyı sistemden bulur ve döndürür.
        public async Task<User?> GetCurrentUserAsync() 
        {
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
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

        //kullanıcının kayıt işlemini API üzerinden yapar ve sonucu döndürür.
        public async Task CikisYapAsync()
        {
            await MarkUserAsLoggedOut(); 
        }

        //mevcut oturum açmış kullanıcının ID’sini alır.
        public async Task<int> GetUserIdAsync()
        {
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
                return 0;

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim == null ? 0 : int.Parse(idClaim.Value);
        }
    }
}
