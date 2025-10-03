using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class WalkInBookingDto
    {
        // Brugeroplysninger
        [Required(ErrorMessage = "Fornavn er påkrævet")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Efternavn er påkrævet")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email er påkrævet")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefonnummer er påkrævet")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Booking-detaljer
        [Required]
        public int RoomTypeId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public int GuestCount { get; set; }

        public List<int> ServiceIds { get; set; } = new List<int>();
    }
}