using System.Collections.Generic;

namespace DomainModels.DTOs
{
    public class ReceptionistDashboardDto
    {
        public List<BookingSummaryDto> TodaysArrivals { get; set; } = new List<BookingSummaryDto>();
        public List<BookingSummaryDto> TodaysDepartures { get; set; } = new List<BookingSummaryDto>();
        public int OccupiedRoomsCount { get; set; }
        public int AvailableRoomsCount { get; set; }
    }
}