using API.Data; // Kan måske fjernes, hvis ingen andre dependencies bruger den
using API.Repositories; // Tilføj denne using-statement
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Kan måske fjernes
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace API.Controllers
{
    /// <summary>
    /// Håndterer alle operationer relateret til bookinger, såsom oprettelse, hentning og administration.
    /// Kræver som udgangspunkt at brugeren er logget ind.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {

        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingsController> _logger;
        private readonly IMemoryCache _cache;
        private readonly AppDBContext _context; // Bevaret midlertidigt for 'my-bookings' logik

        /// <summary>
        /// Initialiserer en ny instans af BookingsController med de nødvendige dependencies.
        /// </summary>
        public BookingsController(IBookingRepository bookingRepository, ILogger<BookingsController> logger, IMemoryCache cache, AppDBContext context)
        {
            _bookingRepository = bookingRepository; // Tilføjet
            _logger = logger;
            _cache = cache;
            _context = context; // Bevaret midlertidigt
        }

        /// <summary>
        /// Henter en liste over alle bookinger. Kræver 'Receptionist' eller 'Manager' rolle.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Receptionist,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<BookingGetDto>>> GetBookings(
        [FromQuery] string? userId,
        [FromQuery] DateTime? fromDate)
        {
            _logger.LogInformation("Henter bookingliste med filtre via repository: UserId={UserId}, FromDate={FromDate}", userId, fromDate);

            // SIMPLIFICERET: Alt EF Core logik er nu i repository'et
            var bookings = await _bookingRepository.GetAllAsync(userId, fromDate);

            var result = bookings.Select(b => new BookingGetDto
            {
                Id = b.Id,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                RoomTypeName = b.RoomType.Name,
                RoomNumber = b.Room != null ? b.Room.RoomNumber : "Not Assigned",
                UserEmail = b.User.Email,
                CreatedAt = b.CreatedAt
            });

            _logger.LogInformation("Returnerede {Count} bookinger efter filtrering.", result.Count());
            return Ok(result);
        }

        /// <summary>
        /// Henter alle bookinger for den aktuelt indloggede bruger.
        /// </summary>
        [HttpGet("my-bookings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<BookingGetDto>>> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bruger-ID ikke fundet i token.");
            }

            var cacheKey = $"my_bookings_{userId}";
            if (_cache.TryGetValue(cacheKey, out List<BookingGetDto> cachedBookings))
            {
                return Ok(cachedBookings);
            }

            // SIMPLIFICERET: Bruger repository til at hente data
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
                UserEmail = b.User.Email,
                CreatedAt = b.CreatedAt
            }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
            _cache.Set(cacheKey, resultDto, cacheOptions);

            return Ok(resultDto);
        }

        /// <summary>
        /// Opretter en ny booking for den indloggede bruger.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingGetDto>> CreateBooking(BookingCreateDto bookingDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Bruger-ID ikke fundet i token.");

            // Validering og forretningslogik forbliver i controlleren
            var roomType = await _context.RoomTypes.Include(rt => rt.Rooms).FirstOrDefaultAsync(rt => rt.Id == bookingDto.RoomTypeId);
            if (roomType == null) return BadRequest("Den valgte værelsestype findes ikke.");

            var nights = (bookingDto.CheckOutDate - bookingDto.CheckInDate).Days;
            if (nights <= 0) return BadRequest("Check-ud dato skal være efter check-in dato.");

            // Tjek for ledighed (denne logik kan også flyttes til en service senere)
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

            //Bruger repository til at oprette
            var createdBooking = await _bookingRepository.CreateAsync(booking);

            var cacheKey = $"my_bookings_{userId}";
            _cache.Remove(cacheKey);

            var resultDto = new BookingGetDto
            {
                Id = createdBooking.Id,
                CheckInDate = createdBooking.CheckInDate,
                CheckOutDate = createdBooking.CheckOutDate,
                TotalPrice = createdBooking.TotalPrice,
                Status = createdBooking.Status,
                RoomTypeName = roomType.Name,
                RoomNumber = "Not Assigned",
                UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "",
                CreatedAt = createdBooking.CreatedAt
            };

            return CreatedAtAction(nameof(GetBooking), new { id = resultDto.Id }, resultDto);
        }

        /// <summary>
        /// Henter en specifik booking baseret på dens unikke ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingGetDto>> GetBooking(string id)
        {
            _logger.LogInformation("Henter booking med ID {BookingId} via repository", id);

            //Bruger repository til at hente
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                return Forbid();
            }

            var resultDto = new BookingGetDto
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

            return Ok(resultDto);
        }
    }
}