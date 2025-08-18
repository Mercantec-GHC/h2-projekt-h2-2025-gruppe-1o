using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Repræsenterer en kategori af værelser (f.eks. "Single", "Double", "Suite").
    /// Gæster booker en RoomType, ikke et specifikt Room.
    /// </summary>
    public class RoomType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal BasePrice { get; set; }

        [Required]
        public int Capacity { get; set; }

        /// <summary>
        /// Navigation property til alle de fysiske værelser, der er af denne type.
        /// </summary>
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }

    /// <summary>
    /// DTO til at vise en værelsestype for en gæst under bookingflowet.
    /// </summary>
    public class RoomTypeGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Capacity { get; set; }
        public int AvailableCount { get; set; } // Antal ledige værelser af denne type
    }
}