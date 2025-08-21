using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // Tilføjet for IMemoryCache
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
        private readonly IMemoryCache _cache; // Tilføjet cache felt

        // Opdateret constructor til at modtage IMemoryCache
        public BookingsController(AppDBContext context, ILogger<BookingsController> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache; // Tildel cache
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist,Manager")]
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

            // Tilføj filter for bruger-ID, hvis det er angivet
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(b => b.UserId == userId);
            }

            // Tilføj filter for startdato, hvis det er angivet
            if (fromDate.HasValue)
            {
                // Find bookinger, der er aktive på eller efter den angivne dato
                query = query.Where(b => b.CheckOutDate.Date >= fromDate.Value.Date);
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

        // NYT ENDPOINT MED CACHING
        [HttpGet("my-bookings")]
        public async Task<ActionResult<IEnumerable<BookingGetDto>>> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bruger-ID ikke fundet i token.");
            }

            // Definer en unik cache-nøgle for denne specifikke bruger
            var cacheKey = $"my_bookings_{userId}";
            _logger.LogInformation("Bruger {UserId} forsøger at hente 'my-bookings' fra cache med nøglen '{CacheKey}'", userId, cacheKey);

            // Forsøg at hente fra cache først
            if (_cache.TryGetValue(cacheKey, out List<BookingGetDto> cachedBookings))
            {
                _logger.LogInformation("Cache hit for {CacheKey}! Returnerer {Count} bookinger fra cachen.", cacheKey, cachedBookings.Count);
                return Ok(cachedBookings);
            }

            _logger.LogInformation("Cache miss for {CacheKey}. Henter bookinger fra databasen for bruger {UserId}.", cacheKey, userId);
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.RoomType) // Eager loading for at få RoomTypeName
                .Include(b => b.Room)     // Eager loading for at få RoomNumber
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
                    UserEmail = b.User.Email, // User er allerede tilgængelig via relationen
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            // Gem de friske data i cachen med en kort levetid
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(10)); // Kort cache levetid

            _cache.Set(cacheKey, bookings, cacheOptions);
            _logger.LogInformation("Gemt {Count} bookinger i cachen for {CacheKey}.", bookings.Count, cacheKey);

            return Ok(bookings);
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

            // CACHE INVALIDATION: Ryd cachen for brugerens bookinger
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