using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DomainModels.Enums;

namespace DomainModels
{
    public class Ticket : Common
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [Required]
        public TicketCategory Category { get; set; }

        // Oprettet af (bruger/gæst)
        public string? CreatedByUserId { get; set; }
        public virtual User? CreatedByUser { get; set; }

        [MaxLength(100)]
        public string? GuestName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? GuestEmail { get; set; }

        // Tildelt medarbejder
        public string? AssignedToUserId { get; set; }
        public virtual User? AssignedToUser { get; set; }

        // Relation til selve samtalen
        public virtual ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    }
}