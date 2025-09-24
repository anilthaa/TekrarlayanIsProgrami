namespace TekrarlayanIsProgrami.Services

{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.Authorization;
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal anonymous=new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _user;
        
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_user ?? anonymous));
        }

        public void GirisYap(string username)
        {
            var identity = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, username)
        }, "apiauth_type");

            _user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void CikisYap()
        {
            _user=anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
