using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(AppDBContext context, ILogger<RoomsController> logger)
        {
            _context = context;
            _logger = logger;
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