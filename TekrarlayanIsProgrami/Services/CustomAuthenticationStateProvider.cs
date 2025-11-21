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

        public CustomAuthenticationStateProvider(UserRepository dbContext, IJSRuntime Js, HttpClient http)
        {
            _kullaniciRepo = dbContext;
            _Js = Js;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Eğer _currentUser yoksa localStorage'dan yükle
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
                    // localStorage'a erişilemezse sessizce devam et
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
            await _Js.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        //
        public async Task LoadUserFromLocalStorageAsync()
        {
            var userJson = await _Js.InvokeAsync<string>("localStorage.getItem", "_currentUser");
            if (!string.IsNullOrEmpty(userJson))
            {
                _currentUser = JsonSerializer.Deserialize<User>(userJson);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }

        public async Task<AuthResult> GirisYapAsync(string eposta, string sifre)
        {
            var response=await _http.PostAsJsonAsync("api/auth/login", new {Email=eposta, Password=sifre});
            var rawLoginResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Login Response: {response.StatusCode} - {rawLoginResponse}");

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult { Basarili = false, Mesaj = "E-posta veya Şifre Hatalı." };
            }

            var result = JsonSerializer.Deserialize<AuthResult>(rawLoginResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result==null || !result.Basarili)
            {
                return new AuthResult { Basarili = false, Mesaj = "Giriş başarısız." };
            }

            await _Js.InvokeVoidAsync("localStorage.setItem", "jwtToken", result.Token);

            var kullanici=await _kullaniciRepo.GetByEmailAsync(eposta, sifre);
            if (kullanici==null)
            {
                return new AuthResult { Basarili = false, Mesaj = "Kullanıcı bulunamadı." };
            }

            await MarkUserAsAuthenticated(kullanici);

            return new AuthResult { Basarili = true, Mesaj = $"Hoşgeldiniz {kullanici.Adi}!" };
        }

        public async Task<AuthResult> KayitOlAsync(User user,int secilenKullaniciTipId)
        {
            if (string.IsNullOrWhiteSpace(user.Sifre) || user.Sifre.Length < 8)
            {
                return new AuthResult
                {
                    Basarili = false,
                    Mesaj = $"Şifre en az 8 karakter olmalıdır."
                };
            }

            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", user);

                var responseContent=await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Register Response: {response.StatusCode} - {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    var error=await response.Content.ReadFromJsonAsync<AuthResult>();
                    return error ?? new AuthResult { Basarili = false, Mesaj = "Kayıt başarısız." };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                if (result.Basarili && !string.IsNullOrEmpty(result.Token))
                {
                    await _Js.InvokeVoidAsync("localStorage.setItem", "jwtToken", result.Token);

                    var yeni = await _kullaniciRepo.GetByEmailAsync(user.EPosta, user.Sifre);
                    if (yeni != null)
                    {
                        await MarkUserAsAuthenticated(yeni);
                    }
                    return new AuthResult {Basarili=true,Mesaj=$"Kayıt Başarıyla Tamamlandı! Lütfen Giriş Yapın." };
                }
                return result;
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

        public async Task<AuthResult> UpdateAsync(User user)
        {
            var kullanici = await _kullaniciRepo.GetByEmailAsync(user.EPosta, user.Sifre);
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
            return new AuthResult
            {
                Basarili = true,
                Mesaj = "Giriş başarılı"
            };
        }

        public async Task<User?> GetCurrentUserAsync() //Bu metot şu anda giriş yapmış olan kullanıcıyı sistemden bulur ve döndürür.
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
        //Tarayıcıda kullanıcının token ı gelsin diye yazdık.
        public async Task<AuthResult> Login(string eposta, string sifre)
        {
            var loginData = new { Email = eposta, Password = sifre };
            var response = await _http.PostAsJsonAsync("api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                if (result.Basarili)
                {
                    await _Js.InvokeVoidAsync("localStorage.setItem", "jwtToken", result.Token);
                }
                return result;
            }

            return new AuthResult { Basarili = false, Mesaj = "Hata" };
        }
        public async Task CikisYapAsync()
        {
            _currentUser = null;
            await _Js.InvokeVoidAsync("localStorage.removeItem", "_currentUser");
            await _Js.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            
        }
    }
}
