using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Controllers
{
    /// <summary>
    /// Håndterer offentligt tilgængelige informationer om værelsestyper og deres ledighed.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RoomsController> _logger;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initialiserer en ny instans af RoomsController.
        /// </summary>
        /// <param name="context">Database context for at interagere med databasen.</param>
        /// <param name="logger">Logger til at logge information og fejl.</param>
        /// <param name="cache">Memory cache til at forbedre performance ved gentagne kald.</param>
        public RoomsController(AppDBContext context, ILogger<RoomsController> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Henter en liste over alle tilgængelige værelsestyper på hotellet.
        /// </summary>
        /// <remarks>
        /// Resultatet af dette kald caches for at forbedre ydeevnen.
        /// Gyldige værdier for 'sortBy' er 'price' og 'capacity'.
        /// Hvis 'sortBy' ikke angives, sorteres der som standard efter navn.
        /// </remarks>
        /// <param name="sortBy">Valgfrit. Feltet, der skal sorteres efter ('price' eller 'capacity').</param>
        /// <param name="desc">Valgfrit. Angiver om sorteringen skal være i faldende rækkefølge. Standard er 'false'.</param>
        /// <returns>En liste af værelsestyper.</returns>
        /// <response code="200">Returnerer en liste af værelsestyper, eventuelt sorteret.</response>
        [HttpGet("types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoomTypes([FromQuery] string? sortBy, [FromQuery] bool desc = false)
        {
            var cacheKey = $"allRoomTypes_sortBy={sortBy ?? "default"}_desc={desc}";
            _logger.LogInformation("Forsøger at hente værelsestyper fra cache med nøglen '{CacheKey}'", cacheKey);

            if (_cache.TryGetValue(cacheKey, out List<RoomType> cachedRoomTypes))
            {
                _logger.LogInformation("Cache hit! Returnerer {Count} værelsestyper fra cachen for nøglen '{CacheKey}'.", cachedRoomTypes.Count, cacheKey);
                return Ok(cachedRoomTypes);
            }

            _logger.LogInformation("Cache miss for '{CacheKey}'. Henter værelsestyper fra databasen.", cacheKey);
            var query = _context.RoomTypes.AsQueryable();

            switch (sortBy?.ToLower())
            {
                case "price":
                    query = desc ? query.OrderByDescending(rt => rt.BasePrice) : query.OrderBy(rt => rt.BasePrice);
                    break;
                case "capacity":
                    query = desc ? query.OrderByDescending(rt => rt.Capacity) : query.OrderBy(rt => rt.Capacity);
                    break;
                default:
                    query = query.OrderBy(rt => rt.Name);
                    break;
            }

            var roomTypesFromDb = await query.ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

            _cache.Set(cacheKey, roomTypesFromDb, cacheOptions);
            _logger.LogInformation("Gemt {Count} værelsestyper i cachen for nøglen '{CacheKey}'.", roomTypesFromDb.Count, cacheKey);

            return Ok(roomTypesFromDb);
        }

        /// <summary>
        /// Finder ledige værelsestyper baseret på ankomst, afrejse og antal gæster.
        /// </summary>
        /// <remarks>
        /// Dette er det primære søge-endpoint for gæster. Det returnerer en liste af værelsestyper,
        /// hvor der er mindst ét ledigt værelse i den angivne periode, og som har kapacitet til det angivne antal gæster.
        /// </remarks>
        /// <param name="checkInDate">Den ønskede ankomstdato.</param>
        /// <param name="checkOutDate">Den ønskede afrejsedato.</param>
        /// <param name="numberOfGuests">Antallet af gæster, der skal overnatte.</param>
        /// <returns>En liste af ledige værelsestyper med antallet af ledige værelser for hver type.</returns>
        /// <response code="200">Returnerer en liste af ledige værelsestyper.</response>
        /// <response code="400">Hvis check-ud datoen er før eller samme dag som check-in datoen.</response>
        [HttpGet("availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<RoomTypeGetDto>>> GetAvailableRoomTypes(
            [FromQuery] DateTime checkInDate,
            [FromQuery] DateTime checkOutDate,
            [FromQuery] int numberOfGuests)
        {
            _logger.LogInformation("Søger efter ledige værelser fra {CheckIn} til {CheckOut} for {GuestCount} gæster.", checkInDate, checkOutDate, numberOfGuests);

            if (checkOutDate.Date <= checkInDate.Date)
            {
                _logger.LogWarning("Søgning efter ledighed afvist: Check-ud dato er før eller samme dag som check-in.");
                return BadRequest("Check-ud dato skal være efter check-in dato.");
            }

            // Find alle bookede værelsestyper i den givne periode ved kun at sammenligne dato-delen
            var bookedRoomTypeIds = await _context.Bookings
                .Where(b => b.Status != "Cancelled" &&
                            b.CheckInDate.Date < checkOutDate.Date &&
                            b.CheckOutDate.Date > checkInDate.Date)
                .GroupBy(b => b.RoomTypeId)
                .Select(g => new {
                    RoomTypeId = g.Key,
                    BookedCount = g.Count()
                })
                .ToListAsync();

            // Find alle værelsestyper der har kapacitet nok og deres totale antal værelser
            var potentialRoomTypes = await _context.RoomTypes
                .Include(rt => rt.Rooms) // Inkluder Rooms for at kunne tælle totalen
                .Where(rt => rt.Capacity >= numberOfGuests)
                .Select(rt => new {
                    rt.Id,
                    rt.Name,
                    rt.Description,
                    rt.BasePrice,
                    rt.Capacity,
                    TotalRoomCount = rt.Rooms.Count
                })
                .ToListAsync();

            // Sammenlign de to lister for at finde de ledige værelser
            var availableRoomTypes = potentialRoomTypes
                .Select(rt => {
                    var bookingInfo = bookedRoomTypeIds.FirstOrDefault(b => b.RoomTypeId == rt.Id);
                    var bookedCount = bookingInfo?.BookedCount ?? 0;
                    return new RoomTypeGetDto
                    {
                        Id = rt.Id,
                        Name = rt.Name,
                        Description = rt.Description,
                        BasePrice = rt.BasePrice,
                        Capacity = rt.Capacity,
                        AvailableCount = rt.TotalRoomCount - bookedCount
                    };
                })
                .Where(dto => dto.AvailableCount > 0)
                .ToList();

            _logger.LogInformation("Søgning efter ledighed returnerede {ResultCount} værelsestyper.", availableRoomTypes.Count);
            return Ok(availableRoomTypes);
        }
    }
}