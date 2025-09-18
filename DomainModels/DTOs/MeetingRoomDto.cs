using System;
using System.Collections.Generic;

namespace DomainModels.DTOs
{
    // Bruges til at sende en liste af lokaler til frontend
    public class MeetingRoomGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal HourlyRate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    // Bruges til at modtage en ny booking fra frontend
    public class MeetingRoomBookingCreateDto
    {
        public int MeetingRoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string BookedByName { get; set; } = string.Empty;
        public string BookedByEmail { get; set; } = string.Empty;
    }

    // Bruges til at sende info om bookede tider til frontend
    public class TimeSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}