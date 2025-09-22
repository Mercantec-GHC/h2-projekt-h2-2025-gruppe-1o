using System;
using DomainModels.Enums;

namespace DomainModels.DTOs
{
    public class TicketSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketCategory Category { get; set; }
        public string CreatedByName { get; set; } = "N/A";
        public string AssignedToName { get; set; } = "Ikke tildelt";
        public DateTime CreatedAt { get; set; }
    }
}