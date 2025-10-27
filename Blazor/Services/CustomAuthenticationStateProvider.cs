using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using System.Buffers;

namespace Blazor.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        // Simple cache to avoid reparsing the same token repeatedly
        private static readonly object _cacheLock = new object();
        private static string? _cachedToken;
        private static Claim[]? _cachedClaims;

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
            if (string.IsNullOrEmpty(jwt))
                return Array.Empty<Claim>();

            // Return cached result for identical token
            lock (_cacheLock)
            {
                if (jwt == _cachedToken && _cachedClaims != null)
                {
                    // return a copy to avoid external mutation
                    return _cachedClaims.Select(c => new Claim(c.Type, c.Value)).ToList();
                }
            }

            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2)
                    return Array.Empty<Claim>();

                var payload = parts[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);

                var claims = new List<Claim>();

                var reader = new Utf8JsonReader(jsonBytes, isFinalBlock: true, state: default);
                while (reader.Read())
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    var propName = reader.GetString();
                    if (string.IsNullOrEmpty(propName))
                        continue;

                    // advance to the value
                    if (!reader.Read())
                        break;

                    switch (propName)
                    {
                        case "nameid":
                        {
                            var value = GetValueAsString(ref reader);
                            if (!string.IsNullOrEmpty(value)) claims.Add(new Claim(ClaimTypes.NameIdentifier, value));
                            break;
                        }

                        case "unique_name":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                var v = reader.GetString();
                                if (!string.IsNullOrEmpty(v)) claims.Add(new Claim(ClaimTypes.Name, v));
                            }
                            break;

                        case "email":
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                var v = reader.GetString();
                                if (!string.IsNullOrEmpty(v)) claims.Add(new Claim(ClaimTypes.Email, v));
                            }
                            break;

                        case "role":
                            if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                {
                                    if (reader.TokenType == JsonTokenType.String)
                                    {
                                        var role = reader.GetString();
                                        if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));
                                    }
                                    else
                                    {
                                        var role = GetValueAsString(ref reader);
                                        if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));
                                    }
                                }
                            }
                            else if (reader.TokenType == JsonTokenType.String)
                            {
                                var role = reader.GetString();
                                if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                            else
                            {
                                var roleRaw = GetValueAsString(ref reader);
                                if (!string.IsNullOrEmpty(roleRaw)) claims.Add(new Claim(ClaimTypes.Role, roleRaw.Trim('"')));
                            }
                            break;

                        default:
                            // skip values we don't care about
                            break;
                    }
                }

                // cache result
                lock (_cacheLock)
                {
                    _cachedToken = jwt;
                    _cachedClaims = claims.ToArray();
                }

                return claims;
            }
            catch
            {
                return Array.Empty<Claim>();
            }
        }

        private static string GetValueAsString(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString() ?? string.Empty;

            if (reader.HasValueSequence)
            {
                var seq = reader.ValueSequence;
                var length = (int)seq.Length;
                var buffer = new byte[length];
                seq.CopyTo(buffer);
                return Encoding.UTF8.GetString(buffer);
            }

            return Encoding.UTF8.GetString(reader.ValueSpan);
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            // switch pad length
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
                case 0: break;
                default:
                    // pad with = to be safe
                    base64 = base64.PadRight(base64.Length + (4 - (base64.Length % 4)), '=');
                    break;
            }

            // base64url -> base64
            base64 = base64.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(base64);
        }
    }
}