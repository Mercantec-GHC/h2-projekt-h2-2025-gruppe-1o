using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Kræver login for alle handlinger i denne controller
    public class BookingsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public BookingsController(AppDBContext context)
        {
            _context = context;
        }

        // POST: api/Bookings
        /// <summary>
        /// Opretter en ny booking for den indloggede bruger.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookingGetDto>> CreateBooking(BookingCreateDto bookingDto)
        {
            // Hent bruger-ID fra JWT token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Bruger-ID ikke fundet i token.");
            }

            var room = await _context.Rooms.FindAsync(bookingDto.RoomId);
            if (room == null)
            {
                return BadRequest("Det valgte værelse findes ikke.");
            }

            // Simpel validering for at undgå overlappende bookinger
            var isRoomBooked = await _context.Bookings
                .AnyAsync(b => b.RoomId == bookingDto.RoomId &&
                               b.CheckInDate < bookingDto.CheckOutDate &&
                               b.CheckOutDate > bookingDto.CheckInDate);

            if (isRoomBooked)
            {
                return Conflict("Værelset er allerede booket i den valgte periode.");
            }

            var nights = (bookingDto.CheckOutDate - bookingDto.CheckInDate).Days;
            if (nights <= 0)
            {
                return BadRequest("Check-ud dato skal være efter check-in dato.");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RoomId = bookingDto.RoomId,
                CheckInDate = bookingDto.CheckInDate.ToUniversalTime(),
                CheckOutDate = bookingDto.CheckOutDate.ToUniversalTime(),
                TotalPrice = nights * room.PricePerNight,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Returner en fuld DTO med alle detaljer
            var resultDto = new BookingGetDto
            {
                Id = booking.Id,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                RoomNumber = room.RoomNumber,
                UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "",
                CreatedAt = booking.CreatedAt
            };

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, resultDto);
        }

        // GET: api/Bookings/5
        /// <summary>
        /// Henter en specifik booking.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingGetDto>> GetBooking(string id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Tjek om brugeren ejer bookingen eller er admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return new BookingGetDto
            {
                Id = booking.Id,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                RoomNumber = booking.Room?.RoomNumber ?? "N/A",
                UserEmail = booking.User?.Email ?? "N/A",
                CreatedAt = booking.CreatedAt
            };
        }
    }
}