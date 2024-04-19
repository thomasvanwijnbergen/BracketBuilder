using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Security.Claims;

namespace BracketBuilder.Auth
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private ClaimsPrincipal _guest = new ClaimsPrincipal(new ClaimsIdentity());
        private const string SessionKey = "UserSession";

        public AuthStateProvider(ProtectedSessionStorage storage) 
        {
            _sessionStorage = storage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionStorageResult = await _sessionStorage.GetAsync<UserSession>(SessionKey);
                if (!userSessionStorageResult.Success)
                {
                    return new AuthenticationState(_guest);
                }
                var userSession = userSessionStorageResult.Value;
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.Username),
                    new Claim(ClaimTypes.Role, userSession.Role),
                }, "CustomAuth"));

                return new AuthenticationState(claimsPrincipal);
            }
            catch //(InvalidOperationException)
            {
                //login failed, make it a guest instead
                return new AuthenticationState(_guest);
            }
        }

        public async Task UpdateAuthenticationState(UserSession userSession) 
        {
            ClaimsPrincipal claimsPrincipal;

            if (userSession is not null)
            {
                //user logged in
                await _sessionStorage.SetAsync(SessionKey, userSession);
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.Username),
                    new Claim(ClaimTypes.Role, userSession.Role),
                }));
            }
            else
            {
                //user logged out
                await _sessionStorage.DeleteAsync(SessionKey);
                claimsPrincipal = _guest;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}
