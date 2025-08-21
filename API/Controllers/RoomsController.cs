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

        [HttpGet("types")] // Nyt endpoint til at hente alle værelsestyper
        public async Task<IActionResult> GetRoomTypes()
        {
            // Definer en unik nøgle for de data, du vil cache
            const string cacheKey = "allRoomTypes";
            _logger.LogInformation("Forsøger at hente værelsestyper fra cache med nøglen '{CacheKey}'", cacheKey);

            // Prøv at hente fra cache først
            if (_cache.TryGetValue(cacheKey, out List<RoomType> cachedRoomTypes))
            {
                _logger.LogInformation("Cache hit! Returnerer {Count} værelsestyper fra cachen.", cachedRoomTypes.Count);
                return Ok(cachedRoomTypes);
            }

            // Hvis data ikke var i cachen (cache miss), så hent fra databasen
            _logger.LogInformation("Cache miss. Henter værelsestyper fra databasen.");
            var roomTypesFromDb = await _context.RoomTypes.ToListAsync();

            // Gem de friske data i cachen til næste gang
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Udøber efter 5 min inaktivitet
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60)); // Udløber senest om 60 min

            _cache.Set(cacheKey, roomTypesFromDb, cacheOptions);
            _logger.LogInformation("Gemt {Count} værelsestyper i cachen.", roomTypesFromDb.Count);

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