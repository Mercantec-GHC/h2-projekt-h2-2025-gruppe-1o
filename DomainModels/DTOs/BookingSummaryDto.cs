namespace DomainModels.DTOs
{
    public class BookingSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string GuestFullName { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}