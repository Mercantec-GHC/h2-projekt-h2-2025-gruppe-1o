
namespace Blazor.Models
{
    public class SearchQuery
    {
        public DateTime? CheckInDate { get; set; } = DateTime.Today;
        public DateTime? CheckOutDate { get; set; } = DateTime.Today.AddDays(1);
        public int GuestCount { get; set; } = 2;
    }
}