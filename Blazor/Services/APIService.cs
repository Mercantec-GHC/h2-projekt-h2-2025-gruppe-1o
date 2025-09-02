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

        // --- Booking & Room Metoder ---
        public async Task<List<RoomTypeGetDto>?> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int guestCount)
        {
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
            var response = await _httpClient.PostAsJsonAsync("api/Bookings", bookingDetails);
            return response.IsSuccessStatusCode;
        }

        // --- Bruger-specifikke Metoder ---
        public async Task<List<BookingGetDto>?> GetMyBookingsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<BookingGetDto>>("api/Bookings/my-bookings");
            }
            catch { return new List<BookingGetDto>(); }
        }

        public async Task<UserDetailDto?> GetMyDetailsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDetailDto>("api/Users/me");
            }
            catch { return null; }
        }

        public async Task<bool> UpdateMyDetailsAsync(string userId, UserUpdateDto userDetails)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Users/{userId}", userDetails);
            return response.IsSuccessStatusCode;
        }

        // --- Autentificerings-metoder ---
        public async Task<bool> RegisterAsync(RegisterDto registerModel)
        {
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return token;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // --- Status Metoder ---
        public async Task<HealthCheckResponse?> GetHealthCheckAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/Status/healthcheck");
            }
            catch { return new HealthCheckResponse { status = "Error", message = "Kunne ikke forbinde til API." }; }
        }

        public async Task<HealthCheckResponse?> GetDBHealthCheckAsync()
        {
            // NOTE: Lige nu kalder vi det samme endpoint. I fremtiden kunne dette pege på et dedikeret DB-healthcheck endpoint.
            try
            {
                return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/Status/healthcheck");
            }
            catch { return new HealthCheckResponse { status = "Error", message = "Simuleret DB-check fejlede." }; }
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/change-password", dto);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            // Forsøg at deserialisere en standard fejlbesked fra API'et
            var errorObject = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent);
            var errorMessage = errorObject.TryGetProperty("title", out var title) ? title.GetString() : "Der opstod en ukendt fejl.";

            return (false, errorMessage ?? "Der opstod en ukendt fejl.");
        }


    }

    public class LoginResult
    {
        public string? Token { get; set; }
    }
}