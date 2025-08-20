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
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(AppDBContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<BookingGetDto>> CreateBooking(BookingCreateDto bookingDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Bruger {UserId} forsøger at oprette booking for RoomTypeId {RoomTypeId} fra {CheckIn} til {CheckOut}",
                userId, bookingDto.RoomTypeId, bookingDto.CheckInDate, bookingDto.CheckOutDate);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateBooking afvist: Bruger-ID ikke fundet i token.");
                return Unauthorized("Bruger-ID ikke fundet i token.");
            }

            var roomType = await _context.RoomTypes.Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.Id == bookingDto.RoomTypeId);

            if (roomType == null)
            {
                _logger.LogWarning("Booking for bruger {UserId} fejlede: Værelsestype med ID {RoomTypeId} findes ikke.", userId, bookingDto.RoomTypeId);
                return BadRequest("Den valgte værelsestype findes ikke.");
            }

            var bookedCount = await _context.Bookings
                .CountAsync(b => b.RoomTypeId == bookingDto.RoomTypeId &&
                                    b.CheckInDate < bookingDto.CheckOutDate &&
                                    b.CheckOutDate > bookingDto.CheckInDate &&
                                    b.Status != "Cancelled");

            if (bookedCount >= roomType.Rooms.Count)
            {
                _logger.LogWarning("Booking for bruger {UserId} fejlede: Ingen ledige værelser af type {RoomTypeId} i den valgte periode.", userId, bookingDto.RoomTypeId);
                return Conflict("Der er desværre ingen ledige værelser af den valgte type i den angivne periode.");
            }

            var nights = (bookingDto.CheckOutDate - bookingDto.CheckInDate).Days;
            if (nights <= 0)
            {
                _logger.LogWarning("Booking for bruger {UserId} fejlede: Check-ud dato er ikke efter check-in dato.", userId);
                return BadRequest("Check-ud dato skal være efter check-in dato.");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RoomTypeId = bookingDto.RoomTypeId,
                RoomId = null,
                CheckInDate = bookingDto.CheckInDate.ToUniversalTime(),
                CheckOutDate = bookingDto.CheckOutDate.ToUniversalTime(),
                TotalPrice = nights * roomType.BasePrice,
                Status = "Confirmed"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var resultDto = new BookingGetDto
            {
                Id = booking.Id,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                RoomTypeName = roomType.Name,
                RoomNumber = "Not Assigned",
                UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "",
                CreatedAt = booking.CreatedAt
            };

            _logger.LogInformation("Booking {BookingId} oprettet succesfuldt for bruger {UserId}.", booking.Id, userId);
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, resultDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingGetDto>> GetBooking(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Bruger {UserId} anmoder om booking med ID {BookingId}", currentUserId, id);

            var booking = await _context.Bookings
                .Include(b => b.RoomType)
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                _logger.LogWarning("Booking med ID {BookingId} blev ikke fundet (anmodet af {UserId}).", id, currentUserId);
                return NotFound();
            }

            if (booking.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                _logger.LogWarning("Uautoriseret forsøg: Bruger {RequestingUserId} forsøgte at tilgå booking {BookingId}, som tilhører bruger {OwnerId}.", currentUserId, id, booking.UserId);
                return Forbid();
            }

            _logger.LogInformation("Booking {BookingId} hentet succesfuldt for bruger {UserId}", id, currentUserId);
            return new BookingGetDto
            {
                Id = booking.Id,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                RoomTypeName = booking.RoomType?.Name ?? "N/A",
                RoomNumber = booking.Room?.RoomNumber ?? "Not Assigned",
                UserEmail = booking.User?.Email ?? "N/A",
                CreatedAt = booking.CreatedAt
            };
        }
    }
}