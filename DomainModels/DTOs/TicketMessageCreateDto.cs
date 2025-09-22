using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class TicketMessageCreateDto
    {
        [Required(ErrorMessage = "Beskedens indhold må ikke være tomt.")]
        public string Content { get; set; } = string.Empty;

        public bool IsInternalNote { get; set; } = false;
    }
}