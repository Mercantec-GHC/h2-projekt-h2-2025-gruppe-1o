using System.ComponentModel.DataAnnotations;
using DomainModels.Enums;

namespace DomainModels.DTOs
{
    public class TicketCreateDto
    {
        [Required(ErrorMessage = "Titel er påkrævet.")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Beskrivelse er påkrævet.")]
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Kategori er påkrævet.")]
        public TicketCategory Category { get; set; }
        public string? GuestName { get; set; }
        [EmailAddress]
        public string? GuestEmail { get; set; }
    }
}