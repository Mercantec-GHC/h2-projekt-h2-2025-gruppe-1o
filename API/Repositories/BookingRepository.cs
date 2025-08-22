using API.Data;
using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDBContext _context;

        public BookingRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateAsync(Booking newBooking)
        {
            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();
            return newBooking;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync(string? userId = null, DateTime? fromDate = null)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.RoomType)
                .Include(b => b.Room)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(b => b.UserId == userId);
            }

            if (fromDate.HasValue)
            {
                var fromDateUtc = fromDate.Value.ToUniversalTime();
                query = query.Where(b => b.CheckOutDate.Date >= fromDateUtc.Date);
            }

            return await query.ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(string id)
        {
            return await _context.Bookings
                .Include(b => b.RoomType)
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}