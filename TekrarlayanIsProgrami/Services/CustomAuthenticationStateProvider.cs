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
        private readonly UserRepository _userRepository;

        public CustomAuthenticationStateProvider(UserRepository dbContext)
        {
            _userRepository = dbContext;
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_user ?? anonymous));
        }

        public async Task<AuthResult> GirisYapAsync(string eposta, string sifre)
        {
            var user=await _userRepository.GirisYapAsync(eposta, sifre);
            if (user != null)
            {
                return new AuthResult
                {
                    Success = true,
                    Message = $"Hoşgeldiniz {user.Adi}!"
                };

                
                //var identity = new ClaimsIdentity(new[]
                //{
                //    new Claim(ClaimTypes.Name, user.Adi ?? user.EPosta)
                //}, "apiauth_type");

                //_user = new ClaimsPrincipal(identity);
                //NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                //return true;
            }
            return new AuthResult
            {
                Success = false,
                Message = "E-posta veya şifre hatalı!"
            };



            //var cmd = new NpgsqlCommand("Select * from kullanici where eposta=@eposta and sifre=@sifre and aktif=true", conn);

            //cmd.Parameters.AddWithValue("eposta", eposta);
            //cmd.Parameters.AddWithValue("sifre", sifre);

            //var reader = await cmd.ExecuteReaderAsync();

            //if (await reader.ReadAsync()) {
            //    var identity = new ClaimsIdentity(new[]
            //    {
            //        new Claim(ClaimTypes.Name, reader["adi"].ToString())
            //    }, "apiauth_type");
            //    _user = new ClaimsPrincipal(identity);
            //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            //    return true;
            //}
            //return false;
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
                bool kayitBasarili = await _userRepository.KayitOlAsync(user);

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

            //using var conn= _dbContext.CreateConnection();
            //await conn.OpenAsync(); 

            //var cmd = new NpgsqlCommand(@"insert into kullanici 
            //                           (adi,soyadi,eposta,sifre,aktif,creuser,credate)
            //                           values (@adi, @soyadi, @eposta, @sifre, true, @creuser, @credate", conn);

            //cmd.Parameters.AddWithValue("adi", user.Adi ?? "");
            //cmd.Parameters.AddWithValue("soyadi", user.Soyadi ?? "");
            //cmd.Parameters.AddWithValue("eposta", user.EPosta ?? "");
            //cmd.Parameters.AddWithValue("sifre", user.Sifre ?? "");
            //cmd.Parameters.AddWithValue("creuser", user.CreUser);
            //cmd.Parameters.AddWithValue("credate", DateTime.Now);
            
            //var result = await cmd.ExecuteNonQueryAsync();
            //return result > 0;
       

        public void CikisYap()
        {
            _user=anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
