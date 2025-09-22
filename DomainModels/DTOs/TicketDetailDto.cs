using System;
using System.Collections.Generic;
using DomainModels.Enums;

namespace DomainModels.DTOs
{
    public class TicketDetailDto : TicketSummaryDto
    {
        public string? Description { get; set; }
        public List<TicketMessageDto> Messages { get; set; } = new List<TicketMessageDto>();
    }

    public class TicketMessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsInternalNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}