using DomainModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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

        // Metode til at hente ledige værelser
        public async Task<List<RoomTypeGetDto>?> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int guestCount)
        {
            var url = $"api/rooms/availability?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&numberOfGuests={guestCount}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<RoomTypeGetDto>>(url);
                return result ?? new List<RoomTypeGetDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af ledige værelser: {ex.Message}");
                return new List<RoomTypeGetDto>();
            }
        }

        // Metode til at registrere en ny bruger
        public async Task<bool> RegisterAsync(RegisterDto registerModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/register", registerModel);
            return response.IsSuccessStatusCode;
        }

        // Metode til at logge en bruger ind
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
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

            return token;
        }

        // Metode til at logge en bruger ud
        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // Sundhedstjek-metoder
        public async Task<HealthCheckResponse?> GetHealthCheckAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/status/healthcheck");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fejl ved HealthCheck: " + ex.Message);
                return new HealthCheckResponse { status = "Error", message = "Kunne ikke hente API-status (" + ex.Message + ")" };
            }
        }

        public async Task<HealthCheckResponse?> GetDBHealthCheckAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/status/dbhealthcheck");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fejl ved DBHealthCheck: " + ex.Message);
                return new HealthCheckResponse { status = "Error", message = "Kunne ikke hente database-status (" + ex.Message + ")" };
            }
        }

        // Metode til at hente den indloggede brugers bookinger
        public async Task<List<BookingGetDto>?> GetMyBookingsAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<BookingGetDto>>("api/Bookings/my-bookings");
                return result ?? new List<BookingGetDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af 'mine bookinger': {ex.Message}");
                return new List<BookingGetDto>(); // Returner en tom liste ved fejl
            }
        }
    }

    // Modeller der bruges af servicen
    public class LoginResult
    {
        public string? Token { get; set; }
    }
}