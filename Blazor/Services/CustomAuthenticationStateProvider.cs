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
                // Debug: Print alle claims
                Console.WriteLine("JWT Claims found:");
                foreach (var kvp in keyValuePairs)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }

                // Standard JWT claim names (not ClaimTypes constants)
                keyValuePairs.TryGetValue("nameid", out var userId); // JWT subject claim (nameid)
                if (userId != null)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()!));
                    Console.WriteLine($"Added NameIdentifier claim: {userId}");
                }

                keyValuePairs.TryGetValue("unique_name", out var name);
                if (name != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
                }

                keyValuePairs.TryGetValue("email", out var email);
                if (email != null)
                {
                    claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
                }

                keyValuePairs.TryGetValue("role", out var role);
                if (role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()!));
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