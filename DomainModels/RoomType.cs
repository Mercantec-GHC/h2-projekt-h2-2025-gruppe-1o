using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DomainModels.DTOs;

namespace DomainModels
{
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

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        // DENNE LINJE MANGLER I DIN FIL:
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }

    public class RoomTypeGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Capacity { get; set; }
        public int AvailableCount { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class RoomTypeDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Capacity { get; set; }
        public List<ServiceGetDto> Services { get; set; } = new();
    }
}