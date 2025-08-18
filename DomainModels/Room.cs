using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Repræsenterer et FYSISK hotelværelse (f.eks. "Værelse 101"). 
    /// Dette er en instans af en RoomType.
    /// </summary>
    public class Room
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Det unikke nummer for værelset (f.eks. "101", "205B").
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Den aktuelle status for værelset ("Clean", "Dirty", "OutOfOrder").
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Clean";

        /// <summary>
        /// Foreign key, der refererer til den type, værelset er.
        /// </summary>
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Navigation property til værelsestypen.
        /// </summary>
        public virtual RoomType? RoomType { get; set; }

        /// <summary>
        /// Navigation property til de bookinger, der er tilknyttet dette specifikke værelse.
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    /// <summary>
    /// DTO til at vise information om et specifikt, fysisk værelse til personale.
    /// </summary>
    public class RoomGetDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
    }
}