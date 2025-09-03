using API.Data;
using API.Repositories;
using DomainModels;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingsController> _logger;
        private readonly IMemoryCache _cache;
        private readonly AppDBContext _context;

        public BookingsController(IBookingRepository bookingRepository, ILogger<BookingsController> logger, IMemoryCache cache, AppDBContext context)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
            _cache = cache;
            _context = context;
        }

        [HttpGet("my-bookings")]
        public async Task<ActionResult<IEnumerable<BookingGetDto>>> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bruger-ID ikke fundet i token.");
            }

            var bookings = await _bookingRepository.GetAllAsync(userId);
            var resultDto = bookings.Select(b => new BookingGetDto
            {
                Id = b.Id,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                RoomTypeName = b.RoomType.Name,
                RoomNumber = b.Room != null ? b.Room.RoomNumber : "Not Assigned",
                UserFullName = $"{b.User.FirstName} {b.User.LastName}",
                CreatedAt = b.CreatedAt
            }).ToList();

            return Ok(resultDto);
        }

        [HttpPost]
        public async Task<ActionResult<BookingGetDto>> CreateBooking(BookingCreateDto bookingDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Bruger-ID ikke fundet i token.");

            var roomType = await _context.RoomTypes.Include(rt => rt.Rooms).FirstOrDefaultAsync(rt => rt.Id == bookingDto.RoomTypeId);
            if (roomType == null) return BadRequest("Den valgte værelsestype findes ikke.");

            var nights = (bookingDto.CheckOutDate - bookingDto.CheckInDate).Days;
            if (nights <= 0) return BadRequest("Check-ud dato skal være efter check-in dato.");

            var bookedCount = (await _bookingRepository.GetAllAsync()).Count(b => b.RoomTypeId == bookingDto.RoomTypeId &&
                                     b.CheckInDate < bookingDto.CheckOutDate &&
                                     b.CheckOutDate > bookingDto.CheckInDate &&
                                     b.Status != "Cancelled");

            if (bookedCount >= roomType.Rooms.Count) return Conflict("Der er desværre ingen ledige værelser af den valgte type i den angivne periode.");

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

            var createdBooking = await _bookingRepository.CreateAsync(booking);
            var userFullName = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();
            if (string.IsNullOrEmpty(userFullName))
            {
                userFullName = User.FindFirstValue(ClaimTypes.Name) ?? "N/A";
            }


            var resultDto = new BookingGetDto
            {
                Id = createdBooking.Id,
                CheckInDate = createdBooking.CheckInDate,
                CheckOutDate = createdBooking.CheckOutDate,
                TotalPrice = createdBooking.TotalPrice,
                Status = createdBooking.Status,
                RoomTypeName = roomType.Name,
                RoomNumber = "Not Assigned",
                UserFullName = userFullName,
                CreatedAt = createdBooking.CreatedAt
            };

            // Assuming you have a GetBookingById method or similar
            return CreatedAtAction(nameof(GetMyBookings), new { id = resultDto.Id }, resultDto);
        }


        [HttpGet]
        [Authorize(Roles = "Receptionist, Manager")]
        public async Task<ActionResult<IEnumerable<BookingSummaryDto>>> GetAllBookings([FromQuery] string? guestName, [FromQuery] DateTime? date)
        {
            var bookings = await _bookingRepository.GetAllAsync(null, guestName, date);

            var resultDto = bookings.Select(b => new BookingSummaryDto
            {
                Id = b.Id,
                GuestFullName = $"{b.User.FirstName} {b.User.LastName}",
                RoomTypeName = b.RoomType.Name,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                Status = b.Status
            }).ToList();

            return Ok(resultDto);
        }
    }
}