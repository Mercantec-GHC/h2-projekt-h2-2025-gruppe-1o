using DomainModels;

namespace API.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(string id);
        Task<IEnumerable<Booking>> GetAllAsync(string? userId = null, string? guestName = null, DateTime? date = null);
        Task<Booking> CreateAsync(Booking newBooking);
    }
}