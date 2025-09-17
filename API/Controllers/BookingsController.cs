using API.Data;
using API.Repositories;
using DomainModels;
using DomainModels.DTOs;
using DomainModels.Enums;
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
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingsController> _logger;
        private readonly AppDBContext _context;

        public BookingsController(IBookingRepository bookingRepository, ILogger<BookingsController> logger, AppDBContext context)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
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
            var checkInDateUtc = DateTime.SpecifyKind(bookingDto.CheckInDate, DateTimeKind.Utc);
            var checkOutDateUtc = DateTime.SpecifyKind(bookingDto.CheckOutDate, DateTimeKind.Utc);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Bruger-ID ikke fundet i token.");

            var roomType = await _context.RoomTypes.Include(rt => rt.Rooms).FirstOrDefaultAsync(rt => rt.Id == bookingDto.RoomTypeId);
            if (roomType == null) return BadRequest("Den valgte værelsestype findes ikke.");

            var nights = (checkOutDateUtc - checkInDateUtc).Days;
            if (nights <= 0) return BadRequest("Check-ud dato skal være efter check-in dato.");

            var allRoomIdsForType = roomType.Rooms.Select(r => r.Id).ToList();
            var conflictingBookings = await _context.Bookings
                .Where(b => b.RoomTypeId == bookingDto.RoomTypeId &&
                            b.Status != "Cancelled" &&
                            b.CheckInDate < checkOutDateUtc &&
                            b.CheckOutDate > checkInDateUtc)
                .ToListAsync();
            var occupiedRoomIds = conflictingBookings.Where(b => b.RoomId.HasValue).Select(b => b.RoomId.Value).ToList();
            var availableRoom = roomType.Rooms.FirstOrDefault(r => !occupiedRoomIds.Contains(r.Id));

            if (availableRoom == null)
            {
                return Conflict("Der er desværre ingen specifikke ledige værelser af den valgte type i den angivne periode.");
            }

            decimal servicesPrice = 0;
            var selectedServices = new List<Service>();

            if (bookingDto.ServiceIds != null && bookingDto.ServiceIds.Any())
            {
                selectedServices = await _context.Services
                    .Where(s => bookingDto.ServiceIds.Contains(s.Id))
                    .ToListAsync();

                foreach (var service in selectedServices)
                {
                    switch (service.BillingType)
                    {
                        case BillingType.PerBooking: servicesPrice += service.Price; break;
                        case BillingType.PerNight: servicesPrice += service.Price * nights; break;
                        case BillingType.PerPerson: servicesPrice += service.Price; break; // Antager 1 person
                        case BillingType.PerPersonPerNight: servicesPrice += service.Price * nights; break; // Antager 1 person
                    }
                }
            }

            var totalPrice = (nights * roomType.BasePrice) + servicesPrice;

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RoomTypeId = bookingDto.RoomTypeId,
                RoomId = availableRoom.Id,
                CheckInDate = checkInDateUtc,
                CheckOutDate = checkOutDateUtc,
                TotalPrice = totalPrice,
                Status = "Confirmed",
                Services = selectedServices
            };

            var createdBooking = await _bookingRepository.CreateAsync(booking);

            var user = await _context.Users.FindAsync(userId);
            var userFullName = user != null ? $"{user.FirstName} {user.LastName}" : "N/A";

            var resultDto = new BookingGetDto
            {
                Id = createdBooking.Id,
                CheckInDate = createdBooking.CheckInDate,
                CheckOutDate = createdBooking.CheckOutDate,
                TotalPrice = createdBooking.TotalPrice,
                Status = createdBooking.Status,
                RoomTypeName = roomType.Name,
                RoomNumber = availableRoom.RoomNumber,
                UserFullName = userFullName,
                CreatedAt = createdBooking.CreatedAt
            };

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