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
            var url = $"api/rooms/availability?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&numberOfGuests={guestCount}";
            return await _httpClient.GetFromJsonAsync<List<RoomTypeGetDto>>(url);
        }

        public async Task<RoomTypeDetailDto?> GetRoomTypeByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RoomTypeDetailDto>($"api/rooms/types/{id}");
        }

        public async Task<List<RoomTypeGetDto>?> GetAllRoomTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<RoomTypeGetDto>>("api/rooms/types");
        }

        public async Task<bool> CreateBookingAsync(BookingCreateDto bookingDetails)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/Bookings", bookingDetails);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<RoomGetDto>> GetAllRoomsAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<RoomGetDto>>("api/rooms") ?? new List<RoomGetDto>();
        }

        public async Task<bool> RequestRoomCleaningAsync(int roomId)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsync($"api/rooms/{roomId}/request-cleaning", null);
            return response.IsSuccessStatusCode;
        }

        // --- Mødelokale Metoder ---
        public async Task<List<MeetingRoomGetDto>?> GetMeetingRoomsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<MeetingRoomGetDto>>("api/meetingrooms");
        }

        public async Task<List<TimeSlotDto>?> GetMeetingRoomAvailabilityAsync(int roomId, DateTime date)
        {
            return await _httpClient.GetFromJsonAsync<List<TimeSlotDto>>($"api/meetingrooms/availability/{roomId}?date={date:yyyy-MM-dd}");
        }

        public async Task<(bool Success, string Message)> BookMeetingRoomAsync(MeetingRoomBookingCreateDto dto)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/meetingrooms/book", dto);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return (true, "Booking gennemført!");
            return (false, content ?? "Ukendt fejl");
        }

        // --- Bruger-specifikke Metoder ---
        public async Task<List<BookingGetDto>?> GetMyBookingsAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<BookingGetDto>>("api/Bookings/my-bookings");
        }

        public async Task<UserDetailDto?> GetMyDetailsAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<UserDetailDto>("api/Users/me");
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateMyDetailsAsync(string userId, UserUpdateDto userDetails)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/Users/{userId}", userDetails);
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent ?? "Ukendt fejl.");
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
            var loginResult = await response.Content.ReadFromJsonAsync<LoginResult>();
            if (string.IsNullOrEmpty(loginResult?.Token)) return null;

            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "authToken", loginResult.Token);
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthentication(loginResult.Token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);
            return loginResult.Token;
        }

        public async Task<StaffLoginResult?> StaffLoginAsync(StaffLoginDto loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/staff-login", loginModel);
            if (!response.IsSuccessStatusCode) return null;

            var loginResult = await response.Content.ReadFromJsonAsync<StaffLoginResult>();
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
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent ?? "Ukendt fejl.");
        }

        // --- Dashboard & Admin Metoder ---
        public async Task<HealthCheckResponse?> GetHealthCheckAsync()
        {
            return await _httpClient.GetFromJsonAsync<HealthCheckResponse>("api/Status/healthcheck");
        }

        public async Task<List<BookingSummaryDto>?> GetBookingsForAdminAsync(string? guestName = null, DateTime? date = null)
        {
            await EnsureAuthHeaderAsync();
            var queryParams = new Dictionary<string, string?>();
            if (!string.IsNullOrEmpty(guestName)) queryParams["guestName"] = guestName;
            if (date.HasValue) queryParams["date"] = date.Value.ToString("yyyy-MM-dd");
            var url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("api/Bookings", queryParams);
            return await _httpClient.GetFromJsonAsync<List<BookingSummaryDto>>(url);
        }

        public async Task<DailyStatsDto?> GetDashboardStatsAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<DailyStatsDto>("api/Dashboard/stats");
        }

        public async Task<ReceptionistDashboardDto?> GetReceptionistDashboardDataAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ReceptionistDashboardDto>("api/Dashboard/receptionist");
        }

        public async Task<List<RoomTypeCardDto>?> GetRoomTypeAvailabilitySummaryAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<RoomTypeCardDto>>("api/rooms/types/availability-summary");
        }

        public async Task<(bool Success, string Message, BookingGetDto? Booking)> CreateWalkInBookingAsync(WalkInBookingDto bookingDetails)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/bookings/walk-in", bookingDetails);

            if (response.IsSuccessStatusCode)
            {
                var createdBooking = await response.Content.ReadFromJsonAsync<BookingGetDto>();
                return (true, "Walk-in booking oprettet succesfuldt.", createdBooking);
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return (false, errorMessage ?? "Der opstod en ukendt fejl under oprettelse af booking.", null);
        }

        // --- Housekeeping Metoder ---
        public async Task<List<RoomGetDto>?> GetRoomsNeedingCleaningAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<RoomGetDto>>("api/housekeeping/rooms-to-clean");
        }

        public async Task<bool> MarkRoomAsCleanAsync(int roomId)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsync($"api/housekeeping/rooms/{roomId}/mark-as-clean", null);
            return response.IsSuccessStatusCode;
        }

        // --- TICKET METODER ---
        public async Task<string?> GetAuthTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
        }

        public async Task<TicketSummaryDto?> CreateTicketAsync(TicketCreateDto ticket)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/tickets", ticket);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TicketSummaryDto>() : null;
        }

        public async Task<List<TicketSummaryDto>?> GetOpenTicketsForMyRoleAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<TicketSummaryDto>>("api/tickets/my-role/open");
        }

        public async Task<List<TicketSummaryDto>?> GetClosedTicketsForMyRoleAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<TicketSummaryDto>>("api/tickets/my-role/closed");
        }

        public async Task<List<TicketSummaryDto>?> GetMyTicketsAsync()
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<TicketSummaryDto>>("api/tickets/my-tickets");
        }

        public async Task<TicketDetailDto?> GetTicketByIdAsync(string id)
        {
            await EnsureAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<TicketDetailDto>($"api/tickets/{id}");
        }

        public async Task<TicketMessageDto?> PostMessageAsync(string ticketId, TicketMessageCreateDto message)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/tickets/{ticketId}/messages", message);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TicketMessageDto>() : null;
        }

        public async Task<bool> UpdateTicketStatusAsync(string ticketId, TicketStatusUpdateDto statusUpdate)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/tickets/{ticketId}/status", statusUpdate);
            return response.IsSuccessStatusCode;
        }

        // --- Active Directory Læse-Metoder ---
        public async Task<List<ADUserDto>?> GetAdUsersAsync()
        {
            try
            {
                await EnsureAuthHeaderAsync();
                return await _httpClient.GetFromJsonAsync<List<ADUserDto>>("api/activedirectory/users");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl under hentning af AD-brugere: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ADGroupDto>?> GetAdGroupsAsync()
        {
            try
            {
                await EnsureAuthHeaderAsync();
                return await _httpClient.GetFromJsonAsync<List<ADGroupDto>>("api/activedirectory/groups");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl under hentning af AD-grupper: {ex.Message}");
                return null;
            }
        }

        // --- Active Directory Admin Metoder ---
        public async Task<(bool Success, string Message)> CreateAdUserAsync(CreateUserDto userDto)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/activedirectory/users", userDto);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return (response.IsSuccessStatusCode, content?.Message ?? "Ukendt svar fra server.");
        }

        public async Task<(bool Success, string Message)> ResetAdUserPasswordAsync(string username, ResetPasswordDto passwordDto)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/activedirectory/users/{username}/reset-password", passwordDto);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return (response.IsSuccessStatusCode, content?.Message ?? "Ukendt svar fra server.");
        }

        public async Task<(bool Success, string Message)> SetAdUserStatusAsync(string username, SetUserStatusDto statusDto)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/activedirectory/users/{username}/status", statusDto);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return (response.IsSuccessStatusCode, content?.Message ?? "Ukendt svar fra server.");
        }

        public async Task<(bool Success, string Message)> AddUserToGroupAsync(string groupName, GroupMemberDto memberDto)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/activedirectory/groups/{groupName}/members", memberDto);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return (response.IsSuccessStatusCode, content?.Message ?? "Ukendt svar fra server.");
        }

        public async Task<(bool Success, string Message)> RemoveUserFromGroupAsync(string groupName, string username)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/activedirectory/groups/{groupName}/members/{username}");
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return (response.IsSuccessStatusCode, content?.Message ?? "Ukendt svar fra server.");
        }
    }

    // DTO-klasser til Active Directory Læsning
    public class ADUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<string> Groups { get; set; } = new List<string>();
        // Tilføjet for at kunne vise status i UI
        public bool IsEnabled { get; set; }
    }
    public class ADGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Members { get; set; } = new List<string>();
    }

    // DTO-klasser til Active Directory Administration
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class ResetPasswordDto { public string NewPassword { get; set; } = string.Empty; }
    public class SetUserStatusDto { public bool IsEnabled { get; set; } }
    public class GroupMemberDto { public string Username { get; set; } = string.Empty; }

    // Interne hjælpe-klasser
    internal class ApiResponse { public string? Message { get; set; } }
    public class LoginResult { public string? Token { get; set; } }
    public class StaffLoginResult { public string? Token { get; set; } public StaffUser? User { get; set; } }
    public class StaffUser { public string? Id { get; set; } public string? Email { get; set; } public string? FirstName { get; set; } public string? Role { get; set; } }
}