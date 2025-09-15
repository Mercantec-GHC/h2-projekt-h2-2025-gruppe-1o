using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace Blazor.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

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
                    return new AuthenticationState(_anonymous);
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

                keyValuePairs.TryGetValue("nameid", out object? nameId);
                if (nameId != null)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId.ToString()!));
                }

                keyValuePairs.TryGetValue("unique_name", out object? uniqueName);
                if (uniqueName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, uniqueName.ToString()!));
                }

                keyValuePairs.TryGetValue("email", out object? email);
                if (email != null)
                {
                    claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
                }

                // This is the most important part. We explicitly look for the "role" key.
                keyValuePairs.TryGetValue("role", out object? roleValue);
                if (roleValue != null)
                {
                    // This handles both single roles (a string) and multiple roles (an array)
                    if (roleValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in jsonElement.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleValue.ToString()!));
                    }
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