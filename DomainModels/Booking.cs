using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
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

        public int? RoomId { get; set; }
        public virtual Room? Room { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }


    public class BookingGetDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty; 
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