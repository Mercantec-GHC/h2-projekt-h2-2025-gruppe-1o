using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RoomsController> _logger;
        private readonly IMemoryCache _cache;

        public RoomsController(AppDBContext context, ILogger<RoomsController> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetRoomTypes([FromQuery] string? sortBy, [FromQuery] bool desc = false)
        {
            // Trin 4: Opret en dynamisk cache-nøgle
            var cacheKey = $"allRoomTypes_sortBy={sortBy ?? "default"}_desc={desc}";
            _logger.LogInformation("Forsøger at hente værelsestyper fra cache med nøglen '{CacheKey}'", cacheKey);

            // Prøv at hente fra cache først
            if (_cache.TryGetValue(cacheKey, out List<RoomType> cachedRoomTypes))
            {
                _logger.LogInformation("Cache hit! Returnerer {Count} værelsestyper fra cachen for nøglen '{CacheKey}'.", cachedRoomTypes.Count, cacheKey);
                return Ok(cachedRoomTypes);
            }

            // Trin 2: Start en IQueryable forespørgsel
            _logger.LogInformation("Cache miss for '{CacheKey}'. Henter værelsestyper fra databasen.", cacheKey);
            var query = _context.RoomTypes.AsQueryable();

            // Trin 3: Tilføj sorteringslogik
            switch (sortBy?.ToLower())
            {
                case "price":
                    query = desc ? query.OrderByDescending(rt => rt.BasePrice) : query.OrderBy(rt => rt.BasePrice);
                    break;
                case "capacity":
                    query = desc ? query.OrderByDescending(rt => rt.Capacity) : query.OrderBy(rt => rt.Capacity);
                    break;
                default:
                    query = query.OrderBy(rt => rt.Name); // Standard sortering
                    break;
            }

            // Eksekver den færdigbyggede query mod databasen
            var roomTypesFromDb = await query.ToListAsync();

            // Gem de friske data i cachen til næste gang
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

            _cache.Set(cacheKey, roomTypesFromDb, cacheOptions);
            _logger.LogInformation("Gemt {Count} værelsestyper i cachen for nøglen '{CacheKey}'.", roomTypesFromDb.Count, cacheKey);

            return Ok(roomTypesFromDb);
        }



        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<RoomTypeGetDto>>> GetAvailableRoomTypes(
            [FromQuery] DateTime checkInDate,
            [FromQuery] DateTime checkOutDate,
            [FromQuery] int numberOfGuests)
        {
            _logger.LogInformation("Søger efter ledige værelser fra {CheckIn} til {CheckOut} for {GuestCount} gæster.", checkInDate, checkOutDate, numberOfGuests);

            if (checkOutDate <= checkInDate)
            {
                _logger.LogWarning("Søgning efter ledighed afvist: Check-ud dato er før eller samme dag som check-in.");
                return BadRequest("Check-ud dato skal være efter check-in dato.");
            }

            var bookedCounts = await _context.Bookings
                .Where(b => b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate && b.Status != "Cancelled")
                .GroupBy(b => b.RoomTypeId)
                .Select(g => new { RoomTypeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoomTypeId, x => x.Count);

            var roomTypeInfos = await _context.RoomTypes
                .Where(rt => rt.Capacity >= numberOfGuests)
                .Select(rt => new
                {
                    rt.Id,
                    rt.Name,
                    rt.Description,
                    rt.BasePrice,
                    rt.Capacity,
                    TotalCount = rt.Rooms.Count()
                })
                .ToListAsync();

            var result = roomTypeInfos
                .Select(rt => new RoomTypeGetDto
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Description = rt.Description,
                    BasePrice = rt.BasePrice,
                    Capacity = rt.Capacity,
                    AvailableCount = rt.TotalCount - bookedCounts.GetValueOrDefault(rt.Id, 0)
                })
                .Where(dto => dto.AvailableCount > 0)
                .ToList();

            _logger.LogInformation("Søgning efter ledighed returnerede {ResultCount} værelsestyper.", result.Count);
            return Ok(result);
        }
    }
}