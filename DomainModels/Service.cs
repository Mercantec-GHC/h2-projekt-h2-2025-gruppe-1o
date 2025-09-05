using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(20)]
        public string BillingType { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}