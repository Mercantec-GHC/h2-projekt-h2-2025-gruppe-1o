using DomainModels;
using System.Net.Http.Json;

namespace Blazor.Services

{
    public partial class APIService(HttpClient httpClient) { 
        

    public async Task<List<RoomTypeGetDto>?> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int guestCount)
        {
            // Vi bygger URL'en med de korrekte query-parametre, som vores API forventer.
            // Datoer formateres til "yyyy-MM-dd" for at sikre, at de læses korrekt af API'et.
            var url = $"api/rooms/availability?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&numberOfGuests={guestCount}";

            try
            {
                // Vi kalder API'et og forventer en liste af RoomTypeGetDto-objekter retur.
                var result = await httpClient.GetFromJsonAsync<List<RoomTypeGetDto>>(url);
                return result ?? new List<RoomTypeGetDto>();
            }
            catch (Exception ex)
            {
                // Simpel fejlhåndtering. Vi logger fejlen til browserens konsol.
                Console.WriteLine($"Fejl ved hentning af ledige værelser: {ex.Message}");
                return new List<RoomTypeGetDto>(); // Returner en tom liste ved fejl.
            }
        }
    }
}