using DomainModels;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Blazor.Services
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public APIService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _jsRuntime = jsRuntime;
        }

        // ADD THIS METHOD - This ensures the auth header is set before each request
        private async Task EnsureAuthHeaderAsync()
        {
            // Always ensure the token is set before each API call
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // --- Booking & Room Metoder ---
        public async Task<List<RoomTypeGetDto>?> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int guestCount)
        {
            await EnsureAuthHeaderAsync(); // ADD THIS LINE
            var url = $"api/rooms/availability?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&numberOfGuests={guestCount}";
            try
            {
                return await _httpClient.GetFromJsonAsync<List<RoomTypeGetDto>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af ledige værelser: {ex.Message}");
                return new List<RoomTypeGetDto>();
            }
        }

        public async Task<RoomTypeDetailDto?> GetRoomTypeByIdAsync(int id)
        {
            await EnsureAuthHeaderAsync(); // ADD THIS LINE
            try
            {
                return await _httpClient.GetFromJsonAsync<RoomTypeDetailDto>($"api/rooms/types/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af værelsestype {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateBookingAsync(BookingCreateDto bookingDetails)
        {
            await EnsureAuthHeaderAsync(); // ADD THIS LINE
            var response = await _httpClient.PostAsJsonAsync("api/Bookings", bookingDetails);
            return response.IsSuccessStatusCode;
        }

        // --- Bruger-specifikke Metoder ---
        public async Task<List<BookingGetDto>?> GetMyBookingsAsync()
        {
            await EnsureAuthHeaderAsync(); // ADD THIS LINE
            try
            {
                return await _httpClient.GetFromJsonAsync<List<BookingGetDto>>("api/Bookings/my-bookings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bookings: {ex.Message}"); // Add logging
                return new List<BookingGetDto>();
            }
        }

        public async Task<UserDetailDto?> GetMyDetailsAsync()
        {
            await EnsureAuthHeaderAsync();

            try
            {
                var response = await _httpClient.GetAsync("api/Users/me");
                Console.WriteLine($"[API] Response Status: {response.StatusCode}");

                // Handle 204 No Content - the API accepts the request but returns no data
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Console.WriteLine("[API] Received 204 No Content - API endpoint may not be implemented correctly");
                    // Can't create mock data here without user context
                    return null;
                }

                if (response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API] Response content: {content}");

                    if (!string.IsNullOrEmpty(content))
                    {
                        return JsonSerializer.Deserialize<UserDetailDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> DebugClaimsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Users/debug-claims");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG CLAIMS] Status: {response.StatusCode}");
                Console.WriteLine($"[DEBUG CLAIMS] Response: {content}");
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG CLAIMS] Error: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateMyDetailsAsync(string userId, UserUpdateDto userDetails)
        {
            await EnsureAuthHeaderAsync(); // ADD THIS LINE
            var response = await _httpClient.PutAsJsonAsync($"api/Users/{userId}", userDetails);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(errorContent))
            {
                try
                {
                    var errorObject = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorObject.TryGetProperty("title", out var title))
                    {
                        return (false, title.GetString() ?? "Der opstod en ukendt fejl.");
                    }
                }
                catch
                {
                    return (false, errorContent);
                }
            }
            return (false, "Der opstod en ukendt fejl under opdatering.");
        }

        // --- Autentificerings-metoder ---
        public async Task<bool> RegisterAsync(RegisterDto registerModel)
        {
            // No auth needed for registration
            var response = await _httpClient.PostAsJsonAsync("api/Users/register", registerModel);
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> LoginAsync(LoginDto loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/login", loginModel);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult?.Token;

            if (string.IsNullOrEmpty(token)) return null;

            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "authToken", token);
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthentication(token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Capital B!

            return token;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto)
        {
            await EnsureAuthHeaderAsync(); // ADD THIS LINE
            var response = await _httpClient.PostAsJsonAsync("api/Users/change-password", dto);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent ?? "Der opstod en ukendt fejl.");
        }

        // --- Status Metoder ---
        public async Task<HealthCheckResponse?> GetHealthCheckAsync()
        {
            // Health checks typically don't need auth, but add if needed
            try
            {
                return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/Status/healthcheck");
            }
            catch { return new HealthCheckResponse { status = "Error", message = "Kunne ikke forbinde til API." }; }
        }

        public async Task<HealthCheckResponse?> GetDBHealthCheckAsync()
        {
            // Health checks typically don't need auth, but add if needed
            try
            {
                return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/Status/healthcheck");
            }
            catch { return new HealthCheckResponse { status = "Error", message = "Simuleret DB-check fejlede." }; }
        }


        public async Task<List<BookingSummaryDto>?> GetBookingsForAdminAsync(string? guestName = null, DateTime? date = null)
        {
            var queryParams = new Dictionary<string, string?>();
            if (!string.IsNullOrEmpty(guestName))
            {
                queryParams["guestName"] = guestName;
            }
            if (date.HasValue)
            {
                queryParams["date"] = date.Value.ToString("yyyy-MM-dd");
            }

            var url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("api/Bookings", queryParams);

            try
            {
                return await _httpClient.GetFromJsonAsync<List<BookingSummaryDto>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af admin-bookinger: {ex.Message}");
                return new List<BookingSummaryDto>();
            }
        }
    }



    public class LoginResult
    {
        public string? Token { get; set; }
    }
}