using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Room entity til database
    /// </summary>
    public class Room : Common
    {
        [Required]
        [MaxLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // F.eks. "Single", "Double", "Suite"

        [Required]
        public decimal PricePerNight { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property til de bookinger, der er tilknyttet dette værelse
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    // --- DTOs for Room ---

    public class RoomGetDto
    {
        public string Id { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public string? Description { get; set; }
    }

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