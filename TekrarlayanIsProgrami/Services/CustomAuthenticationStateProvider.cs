using Microsoft.AspNetCore.Components.Authorization;
using Npgsql;
using VeriKatmani.Models;
using System.Security.Claims;
using System.Threading.Tasks;
namespace TekrarlayanIsProgrami.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal anonymous=new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        private readonly VeriKatmani.DbContext _dbContext;

        public CustomAuthenticationStateProvider(VeriKatmani.DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_user ?? anonymous));
        }

        public async Task<bool> GirisYapAsync(string eposta, string sifre)
        {
            using var conn = _dbContext.CreateConnection();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("Select * from kullanici where eposta=@eposta and sifre=@sifre and aktif=true", conn);

            cmd.Parameters.AddWithValue("eposta", eposta);
            cmd.Parameters.AddWithValue("sifre", sifre);

            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync()) {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, reader["adi"].ToString())
                }, "apiauth_type");
                _user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return true;
            }
            return false;
        }

        public async Task<bool> KayitOlAsync(VeriKatmani.Models.User user)
        {
            using var conn= _dbContext.CreateConnection();
            await conn.OpenAsync(); 

            var cmd = new NpgsqlCommand(@"insert into kullanici 
                                       (adi,soyadi,eposta,sifre,aktif,creuser,credate)
                                       values (@adi, @soyadi, @eposta, @sifre, true, @creuser, @credate", conn);

            cmd.Parameters.AddWithValue("adi", user.Adi ?? "");
            cmd.Parameters.AddWithValue("soyadi", user.Soyadi ?? "");
            cmd.Parameters.AddWithValue("eposta", user.EPosta ?? "");
            cmd.Parameters.AddWithValue("sifre", user.Sifre ?? "");
            cmd.Parameters.AddWithValue("creuser", user.CreUser);
            cmd.Parameters.AddWithValue("credate", DateTime.Now);
            
            var result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public void CikisYap()
        {
            _user=anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
