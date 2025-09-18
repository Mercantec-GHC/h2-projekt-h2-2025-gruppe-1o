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

        private async Task EnsureAuthHeaderAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // --- Booking & Room Metoder ---
        public async Task<List<RoomTypeGetDto>?> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int guestCount)
        {
            await EnsureAuthHeaderAsync();
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
            await EnsureAuthHeaderAsync();
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

        public async Task<List<RoomTypeGetDto>?> GetAllRoomTypesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<RoomTypeGetDto>>("api/rooms/types");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af alle værelsestyper: {ex.Message}");
                return new List<RoomTypeGetDto>();
            }
        }

        public async Task<bool> CreateBookingAsync(BookingCreateDto bookingDetails)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/Bookings", bookingDetails);
            return response.IsSuccessStatusCode;
        }

        // --- Mødelokale Metoder ---
        public async Task<List<MeetingRoomGetDto>?> GetMeetingRoomsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<MeetingRoomGetDto>>("api/meetingrooms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af mødelokaler: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TimeSlotDto>?> GetMeetingRoomAvailabilityAsync(int roomId, DateTime date)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<TimeSlotDto>>($"api/meetingrooms/availability/{roomId}?date={date:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af ledighed for mødelokale {roomId}: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> BookMeetingRoomAsync(MeetingRoomBookingCreateDto dto)
        {
            // DENNE ENE LINJE ER RETTET:
            var response = await _httpClient.PostAsJsonAsync("api/meetingrooms/book", dto);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return (true, "Booking gennemført!");
            }
            return (false, content ?? "Ukendt fejl");
        }

        // --- Bruger-specifikke Metoder ---
        public async Task<List<BookingGetDto>?> GetMyBookingsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<BookingGetDto>>("api/Bookings/my-bookings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bookings: {ex.Message}");
                return new List<BookingGetDto>();
            }
        }

        public async Task<UserDetailDto?> GetMyDetailsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Users/me");
                if (response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    var content = await response.Content.ReadAsStringAsync();
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

        public async Task<(bool Success, string ErrorMessage)> UpdateMyDetailsAsync(string userId, UserUpdateDto userDetails)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/Users/{userId}", userDetails);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent ?? "Der opstod en ukendt fejl under opdatering.");
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return token;
        }

        public async Task<StaffLoginResult?> StaffLoginAsync(StaffLoginDto loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/staff-login", loginModel);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<StaffLoginResult>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResult?.Token == null) return null;

            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "authToken", loginResult.Token);
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthentication(loginResult.Token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            return loginResult;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/Users/change-password", dto);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent ?? "Der opstod en ukendt fejl.");
        }

        // --- Andre Metoder ---
        public async Task<HealthCheckResponse?> GetHealthCheckAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/Status/healthcheck");
            }
            catch { return new HealthCheckResponse { status = "Error", message = "Kunne ikke forbinde til API." }; }
        }

        public async Task<List<BookingSummaryDto>?> GetBookingsForAdminAsync(string? guestName = null, DateTime? date = null)
        {
            await EnsureAuthHeaderAsync();
            var queryParams = new Dictionary<string, string?>();
            if (!string.IsNullOrEmpty(guestName)) queryParams["guestName"] = guestName;
            if (date.HasValue) queryParams["date"] = date.Value.ToString("yyyy-MM-dd");

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

        public async Task<DailyStatsDto?> GetDashboardStatsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<DailyStatsDto>("api/Dashboard/stats");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af dashboard-statistik: {ex.Message}");
                return new DailyStatsDto();
            }
        }
    }

    public class LoginResult
    {
        public string? Token { get; set; }
    }

    public class StaffLoginResult
    {
        public string? Token { get; set; }
        public StaffUser? User { get; set; }
    }

    public class StaffUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? Role { get; set; }
    }
}