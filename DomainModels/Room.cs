using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Repræsenterer et fysisk hotelværelse, der kan bookes.
    /// </summary>
    public class Room : Common
    {
        /// <summary>
        /// Det unikke nummer for værelset (f.eks. "101", "205B").
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Typen af værelse (f.eks. "Single", "Double", "Suite").
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Prisen for at leje værelset for én nat.
        /// </summary>
        [Required]
        public decimal PricePerNight { get; set; }

        /// <summary>
        /// En valgfri, mere detaljeret beskrivelse af værelset.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property til de bookinger, der er tilknyttet dette værelse.
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    /// <summary>
    /// DTO til at vise information om et værelse.
    /// </summary>
    public class RoomGetDto
    {
        public string Id { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO til at oprette et nyt værelse.
    /// </summary>
    public class RoomCreateDto
    {
        [Required(ErrorMessage = "Værelsesnummer er påkrævet")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Værelsestype er påkrævet")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pris pr. nat er påkrævet")]
        [Range(1, 100000, ErrorMessage = "Prisen skal være en gyldig værdi")]
        public decimal PricePerNight { get; set; }

        public string? Description { get; set; }
    }
}