using DomainModels;

namespace API.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(string id);
        Task<IEnumerable<Booking>> GetAllAsync(string? userId = null, DateTime? fromDate = null);
        Task<Booking> CreateAsync(Booking newBooking);
        // Vi kan tilføje UpdateAsync og DeleteAsync senere, hvis vi får brug for det.
    }
}