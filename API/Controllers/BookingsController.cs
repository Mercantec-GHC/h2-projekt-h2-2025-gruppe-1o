using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    /// <summary>
    /// Håndterer oprettelse og visning af bookinger.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Kræver login for alle handlinger i denne controller
    public class BookingsController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initialiserer en ny instans af BookingsController.
        /// </summary>
        /// <param name="context">Database context for booking-systemet.</param>
        public BookingsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Opretter en ny booking for den indloggede bruger.
        /// </summary>
        /// <param name="bookingDto">Data for den nye booking, inklusiv værelsestype-ID og datoer.</param>
        /// <returns>Detaljer om den nyoprettede booking.</returns>
        /// <response code="201">Returnerer den nyoprettede booking.</response>
        /// <response code="400">Hvis input-data er ugyldigt.</response>
        /// <response code="401">Hvis brugeren ikke er logget ind.</response>
        /// <response code="409">Hvis der ikke er ledige værelser af den valgte type.</response>
        [HttpPost]
        public async Task<ActionResult<BookingGetDto>> CreateBooking(BookingCreateDto bookingDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bruger-ID ikke fundet i token.");
            }

            // Find den valgte værelsestype og inkluder dens relaterede værelser for at kunne tælle totalen
            var roomType = await _context.RoomTypes.Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.Id == bookingDto.RoomTypeId);

            if (roomType == null)
            {
                return BadRequest("Den valgte værelsestype findes ikke.");
            }

            // Dobbelttjek for tilgængelighed for at undgå race conditions
            var bookedCount = await _context.Bookings
                .CountAsync(b => b.RoomTypeId == bookingDto.RoomTypeId &&
                                 b.CheckInDate < bookingDto.CheckOutDate &&
                                 b.CheckOutDate > bookingDto.CheckInDate &&
                                 b.Status != "Cancelled");

            if (bookedCount >= roomType.Rooms.Count)
            {
                return Conflict("Der er desværre ingen ledige værelser af den valgte type i den angivne periode.");
            }

            var nights = (bookingDto.CheckOutDate - bookingDto.CheckInDate).Days;
            if (nights <= 0)
            {
                return BadRequest("Check-ud dato skal være efter check-in dato.");
            }

            var booking = new Booking
            {
                UserId = userId,
                RoomTypeId = bookingDto.RoomTypeId, // Korrekt: Bruger RoomTypeId
                RoomId = null, // Vigtigt: Sættes til null, da specifikt værelse først tildeles ved check-in
                CheckInDate = bookingDto.CheckInDate.ToUniversalTime(),
                CheckOutDate = bookingDto.CheckOutDate.ToUniversalTime(),
                TotalPrice = nights * roomType.BasePrice, // Grundpris fra værelsestypen
                Status = "Confirmed"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Opret den DTO, der skal returneres til klienten
            var resultDto = new BookingGetDto
            {
                Id = booking.Id,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                RoomTypeName = roomType.Name,
                RoomNumber = "Not Assigned", // Værelse er ikke tildelt endnu
                UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "",
                CreatedAt = booking.CreatedAt
            };

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, resultDto);
        }

        /// <summary>
        /// Henter en specifik booking ud fra dens ID.
        /// </summary>
        /// <param name="id">Det unikke ID for bookingen.</param>
        /// <returns>Detaljer om den specifikke booking.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingGetDto>> GetBooking(string id)
        {
            var booking = await _context.Bookings
                .Include(b => b.RoomType)
                .Include(b => b.Room) // Inkluder det specifikke værelse (kan være null)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Simpel adgangskontrol: Man må se sin egen booking, eller hvis man er personale
            if (booking.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
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
                RoomTypeName = booking.RoomType?.Name ?? "N/A",
                RoomNumber = booking.Room?.RoomNumber ?? "Not Assigned", // Viser "Not Assigned" hvis RoomId er null
                UserEmail = booking.User?.Email ?? "N/A",
                CreatedAt = booking.CreatedAt
            };
        }
    }
}