using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Blazor.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
                if (string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    return new AuthenticationState(_anonymous);
                }

                // DENNE LINJE ER AFGØRENDE: Den sætter token på ALLE fremtidige kald.
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwtAuth"));
                return new AuthenticationState(claimsPrincipal);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwtAuth"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out var userId);
                if (userId != null)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()!));
                }

                keyValuePairs.TryGetValue(ClaimTypes.Name, out var name);
                if (name != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
                }
            }
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}