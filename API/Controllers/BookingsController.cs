using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDBContext _context;
        private readonly ILogger<BookingsController> _logger;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initialiserer en ny instans af BookingsController med de nødvendige dependencies.
        /// </summary>
        /// <param name="context">Database context for at interagere med databasen.</param>
        /// <param name="logger">Logger til at logge information og fejl.</param>
        /// <param name="cache">Memory cache til at forbedre performance ved gentagne kald.</param>
        public BookingsController(AppDBContext context, ILogger<BookingsController> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Henter en liste over alle bookinger. Kræver 'Receptionist' eller 'Manager' rolle.
        /// </summary>
        /// <remarks>
        /// Giver mulighed for at filtrere bookinger baseret på bruger-ID og en startdato.
        /// </remarks>
        /// <param name="userId">Valgfrit. Filtrer bookinger til kun at inkludere denne specifikke bruger.</param>
        /// <param name="fromDate">Valgfrit. Returnerer bookinger, der er aktive på eller efter denne dato (baseret på CheckOutDate).</param>
        /// <returns>En liste over bookinger, der matcher de angivne filtre.</returns>
        /// <response code="200">Returnerer en liste af bookinger.</response>
        /// <response code="401">Hvis brugeren ikke er autentificeret.</response>
        /// <response code="403">Hvis brugeren ikke har den påkrævede rolle (Receptionist eller Manager).</response>
        [HttpGet]
        [Authorize(Roles = "Receptionist,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<BookingGetDto>>> GetBookings(
        [FromQuery] string? userId,
        [FromQuery] DateTime? fromDate)
        {
            _logger.LogInformation("Henter bookingliste med filtre: UserId={UserId}, FromDate={FromDate}", userId, fromDate);

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

            var result = await query.Select(b => new BookingGetDto
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
            }).ToListAsync();

            _logger.LogInformation("Returnerede {Count} bookinger efter filtrering.", result.Count);
            return Ok(result);
        }

        /// <summary>
        /// Henter alle bookinger for den aktuelt indloggede bruger.
        /// </summary>
        /// <remarks>
        /// Resultatet af dette kald caches i 10 sekunder for at forbedre ydeevnen ved gentagne kald.
        /// </remarks>
        /// <returns>En liste over den indloggede brugers bookinger.</returns>
        /// <response code="200">Returnerer en liste af brugerens bookinger.</response>
        /// <response code="401">Hvis brugeren ikke er autentificeret eller token er ugyldigt.</response>
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
            _logger.LogInformation("Bruger {UserId} forsøger at hente 'my-bookings' fra cache med nøglen '{CacheKey}'", userId, cacheKey);

            if (_cache.TryGetValue(cacheKey, out List<BookingGetDto> cachedBookings))
            {
                _logger.LogInformation("Cache hit for {CacheKey}! Returnerer {Count} bookinger fra cachen.", cacheKey, cachedBookings.Count);
                return Ok(cachedBookings);
            }

            _logger.LogInformation("Cache miss for {CacheKey}. Henter bookinger fra databasen for bruger {UserId}.", cacheKey, userId);
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.RoomType)
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingGetDto
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
                })
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(10));

            _cache.Set(cacheKey, bookings, cacheOptions);
            _logger.LogInformation("Gemt {Count} bookinger i cachen for {CacheKey}.", bookings.Count, cacheKey);

            return Ok(bookings);
        }

        /// <summary>
        /// Opretter en ny booking for den indloggede bruger.
        /// </summary>
        /// <param name="bookingDto">Data for den nye booking, der skal oprettes.</param>
        /// <returns>Den nyoprettede booking.</returns>
        /// <response code="201">Returnerer den nyoprettede booking og en 'Location' header til ressourcen.</response>
        /// <response code="400">Hvis input-data er ugyldigt (f.eks. ugyldig dato eller værelsestype).</response>
        /// <response code="401">Hvis brugeren ikke er autentificeret.</response>
        /// <response code="409">Hvis der ikke er ledige værelser af den valgte type i den angivne periode.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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

            var cacheKey = $"my_bookings_{userId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Cache for {CacheKey} blev ryddet pga. ny booking.", cacheKey);

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

        /// <summary>
        /// Henter en specifik booking baseret på dens unikke ID.
        /// </summary>
        /// <remarks>
        /// En almindelig bruger kan kun hente sine egne bookinger. 
        /// En 'Receptionist' eller 'Admin' kan hente enhver booking.
        /// </remarks>
        /// <param name="id">ID for den booking, der skal hentes.</param>
        /// <returns>Den fundne booking.</returns>
        /// <response code="200">Returnerer den specifikke booking.</response>
        /// <response code="401">Hvis brugeren ikke er autentificeret.</response>
        /// <response code="403">Hvis brugeren forsøger at tilgå en booking, de ikke ejer, og ikke har en admin-rolle.</response>
        /// <response code="404">Hvis en booking med det angivne ID ikke blev fundet.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            // Sikkerhedstjek: Brugeren skal enten eje bookingen eller være receptionist/admin
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