using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Repræsenterer en booking af en værelsestype foretaget af en bruger.
    /// </summary>
    public class Booking : Common
    {
        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Confirmed";

        public string UserId { get; set; } = string.Empty;
        public virtual User? User { get; set; }

        [Required]
        public int RoomTypeId { get; set; }
        public virtual RoomType? RoomType { get; set; }

        public int? RoomId { get; set; } // Nullable, sættes ved check-in
        public virtual Room? Room { get; set; }

        public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }

    /// <summary>
    /// Sammenkoblingstabel for mange-til-mange mellem Booking og Service.
    /// </summary>
    public class BookingService
    {
        public string BookingId { get; set; } = string.Empty;
        public virtual Booking? Booking { get; set; }

        public int ServiceId { get; set; }
        public virtual Service? Service { get; set; }
    }

    public class BookingGetDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty; // Vil være "Not Assigned" før check-in
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class BookingCreateDto
    {
        [Required(ErrorMessage = "Værelsestype-ID er påkrævet")]
        public int RoomTypeId { get; set; }

        [Required(ErrorMessage = "Check-in dato er påkrævet")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-ud dato er påkrævet")]
        public DateTime CheckOutDate { get; set; }

        public List<int> ServiceIds { get; set; } = new();
    }
}