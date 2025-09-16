using DomainModels.Enums; // 1. Tilføj using-statement til din nye enum
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModels
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // NYT: Tilføjet Description
        [Required]
        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")] // God praksis at specificere præcision for decimaler
        public decimal Price { get; set; }

        // RETTET: Ændret fra string til den sikre BillingType enum
        [Required]
        public BillingType BillingType { get; set; }

        // NYT: Tilføjet Category
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        // NYT: Tilføjet IsActive med en standardværdi
        [Required]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}