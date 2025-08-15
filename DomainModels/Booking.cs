using System;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Booking entity til database
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
        public string Status { get; set; } = "Confirmed"; // F.eks. "Confirmed", "Cancelled"

        // Foreign key til User
        public string UserId { get; set; } = string.Empty;

        // Foreign key til Room
        public string RoomId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property til den bruger, der har booket
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Navigation property til det værelse, der er booket
        /// </summary>
        public virtual Room? Room { get; set; }
    }

    // --- DTOs for Booking ---

    public class BookingGetDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty; // For at gøre det let for frontend
        public string UserEmail { get; set; } = string.Empty;  // For at gøre det let for frontend
        public DateTime CreatedAt { get; set; }
    }

    public class BookingCreateDto
    {
        [Required(ErrorMessage = "Værelses-ID er påkrævet")]
        public string RoomId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Check-in dato er påkrævet")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-ud dato er påkrævet")]
        public DateTime CheckOutDate { get; set; }
    }
}