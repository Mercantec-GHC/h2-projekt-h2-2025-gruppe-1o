using API.Data;
using API.Repositories;
using API.Services;
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
        private readonly MailService _mailService; // TILFØJET

        public BookingsController(IBookingRepository bookingRepository, ILogger<BookingsController> logger, AppDBContext context, MailService mailService) // TILFØJET
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
            _context = context;
            _mailService = mailService; // TILFØJET
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

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized("Bruger ikke fundet.");

            var roomType = await _context.RoomTypes.Include(rt => rt.Rooms).FirstOrDefaultAsync(rt => rt.Id == bookingDto.RoomTypeId);
            if (roomType == null) return BadRequest("Den valgte værelsestype findes ikke.");

            var nights = (checkOutDateUtc - checkInDateUtc).Days;
            if (nights <= 0) return BadRequest("Check-ud dato skal være efter check-in dato.");

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
                        case BillingType.PerPerson: servicesPrice += service.Price * bookingDto.GuestCount; break;
                        case BillingType.PerPersonPerNight: servicesPrice += service.Price * nights * bookingDto.GuestCount; break;
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

            var subject = $"Din booking hos Flyhigh Hotel er bekræftet (ID: {createdBooking.Id.Substring(0, 8).ToUpper()})";
            var body = $@"
                <h1>Tak for din booking, {user.FirstName}!</h1>
                <p>Vi glæder os til at byde dig velkommen på Flyhigh Hotel.</p>
                <h3>Booking Detaljer:</h3>
                <ul>
                    <li><strong>Værelsestype:</strong> {roomType.Name}</li>
                    <li><strong>Check-in:</strong> {createdBooking.CheckInDate:D}</li>
                    <li><strong>Check-ud:</strong> {createdBooking.CheckOutDate:D}</li>
                    <li><strong>Antal nætter:</strong> {nights}</li>
                    <li><strong>Totalpris:</strong> {createdBooking.TotalPrice:C}</li>
                </ul>
                <p>Med venlig hilsen,<br>Flyhigh Hotel</p>";

            bool emailSent = await _mailService.SendEmailAsync(user.Email, subject, body);
            if (!emailSent)
            {
                _logger.LogWarning("Booking {BookingId} blev oprettet, men bekræftelsesmailen kunne ikke sendes til {Email}.", createdBooking.Id, user.Email);
            }

            var resultDto = new BookingGetDto
            {
                Id = createdBooking.Id,
                CheckInDate = createdBooking.CheckInDate,
                CheckOutDate = createdBooking.CheckOutDate,
                TotalPrice = createdBooking.TotalPrice,
                Status = createdBooking.Status,
                RoomTypeName = roomType.Name,
                RoomNumber = availableRoom.RoomNumber,
                UserFullName = $"{user.FirstName} {user.LastName}",
                CreatedAt = createdBooking.CreatedAt
            };

            return CreatedAtAction(nameof(GetMyBookings), new { id = resultDto.Id }, resultDto);
        }


        [HttpPost("walk-in")]
        [Authorize(Roles = "Receptionist, Manager")]
        public async Task<ActionResult<BookingGetDto>> CreateWalkInBooking(WalkInBookingDto walkInDto)
        {
            // Start en databasetransaktion for at sikre, at enten alt eller intet bliver gemt
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Tjek om brugeren allerede eksisterer
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == walkInDto.Email);
                if (existingUser != null)
                {
                    return BadRequest("En bruger med denne email findes allerede i systemet.");
                }

                // 2. Opret den nye bruger
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null) throw new InvalidOperationException("Systemfejl: 'User'-rollen mangler.");

                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = walkInDto.FirstName,
                    LastName = walkInDto.LastName,
                    Email = walkInDto.Email,
                    PhoneNumber = walkInDto.PhoneNumber,
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")), // Generer et tilfældigt, sikkert password
                    RoleId = userRole.Id,
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Send velkomstmail
                var welcomeSubject = "Velkommen til Flyhigh Hotel";
                var welcomeBody = $"<h1>Hej {newUser.FirstName}!</h1><p>En konto er blevet oprettet til dig hos Flyhigh Hotel. Du kan nulstille din adgangskode via 'Glemt adgangskode' på login-siden.</p>";
                await _mailService.SendEmailAsync(newUser.Email, welcomeSubject, welcomeBody);

                // 3. Efterlign en normal bookingproces med den nye brugers ID
                // Vi opretter et ClaimsPrincipal manuelt for at kunne kalde den eksisterende CreateBooking-metode
                var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, newUser.Id) };
                var identity = new ClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);
                ControllerContext.HttpContext.User = principal; // Sæt den nye bruger som den "autentificerede" bruger for dette ene kald

                var bookingCreateDto = new BookingCreateDto
                {
                    RoomTypeId = walkInDto.RoomTypeId,
                    CheckInDate = walkInDto.CheckInDate,
                    CheckOutDate = walkInDto.CheckOutDate,
                    GuestCount = walkInDto.GuestCount,
                    ServiceIds = walkInDto.ServiceIds
                };

                var result = await CreateBooking(bookingCreateDto);

                // 4. Hvis alt gik godt, commit transaktionen
                await transaction.CommitAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Hvis noget fejler, rul alle ændringer tilbage
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Fejl under oprettelse af walk-in booking.");
                return StatusCode(500, "Der opstod en intern fejl under bookingprocessen.");
            }
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