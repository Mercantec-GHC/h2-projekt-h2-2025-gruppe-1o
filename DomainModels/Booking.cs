using System;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Repræsenterer en booking af et værelse foretaget af en bruger.
    /// </summary>
    public class Booking : Common
    {
        /// <summary>
        /// Startdatoen for bookingen (check-in).
        /// </summary>
        [Required]
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// Slutdatoen for bookingen (check-ud).
        /// </summary>
        [Required]
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Den samlede, beregnede pris for opholdet.
        /// </summary>
        [Required]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Den aktuelle status for bookingen (f.eks. "Confirmed", "Cancelled").
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Confirmed";

        /// <summary>
        /// Foreign key, der refererer til den bruger, som har lavet bookingen.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Foreign key, der refererer til det værelse, der er blevet booket.
        /// </summary>
        public string RoomId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property til den bruger, der har booket.
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Navigation property til det værelse, der er booket.
        /// </summary>
        public virtual Room? Room { get; set; }
    }

    // --- DTOs for Booking ---

    /// <summary>
    /// Data Transfer Object for at vise detaljeret booking-information.
    /// </summary>
    public class BookingGetDto
    {
        /// <summary>
        /// Bookingens unikke ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Startdato for bookingen.
        /// </summary>
        public DateTime CheckInDate { get; set; }
        /// <summary>
        /// Slutdato for bookingen.
        /// </summary>
        public DateTime CheckOutDate { get; set; }
        /// <summary>
        /// Den samlede beregnede pris for opholdet.
        /// </summary>
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// Den nuværende status for bookingen (f.eks. "Confirmed").
        /// </summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Nummeret på det bookede værelse.
        /// </summary>
        public string RoomNumber { get; set; } = string.Empty;
        /// <summary>
        /// E-mailen på den bruger, der har foretaget bookingen.
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;
        /// <summary>
        /// Tidspunktet hvor bookingen blev oprettet.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for at oprette en ny booking.
    /// </summary>
    public class BookingCreateDto
    {
        /// <summary>
        /// ID'et på det værelse, der skal bookes.
        /// </summary>
        [Required(ErrorMessage = "Værelses-ID er påkrævet")]
        public string RoomId { get; set; } = string.Empty;

        /// <summary>
        /// Den ønskede check-in dato.
        /// </summary>
        [Required(ErrorMessage = "Check-in dato er påkrævet")]
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// Den ønskede check-ud dato.
        /// </summary>
        [Required(ErrorMessage = "Check-ud dato er påkrævet")]
        public DateTime CheckOutDate { get; set; }
    }
}