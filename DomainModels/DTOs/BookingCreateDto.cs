using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class BookingCreateDto
    {
        [Required(ErrorMessage = "Værelsestype-ID er påkrævet")]
        public int RoomTypeId { get; set; }

        [Required(ErrorMessage = "Check-in dato er påkrævet")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-ud dato er påkrævet")]
        public DateTime CheckOutDate { get; set; }

        // Tilføj antal gæster
        [Required(ErrorMessage = "Antal gæster er påkrævet")]
        public int GuestCount { get; set; }

        // NYT: Tilføjet liste af valgte Service IDs
        public List<int> ServiceIds { get; set; } = new List<int>();
    }
}