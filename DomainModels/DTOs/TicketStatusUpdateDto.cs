using DomainModels.Enums;
using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class TicketStatusUpdateDto
    {
        [Required]
        public TicketStatus NewStatus { get; set; }
    }
}