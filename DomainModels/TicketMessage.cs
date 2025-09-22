using System;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    public class TicketMessage : Common
    {
        [Required]
        public string TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }

        [Required]
        public string SenderId { get; set; }
        public virtual User Sender { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsInternalNote { get; set; } = false;
    }
}