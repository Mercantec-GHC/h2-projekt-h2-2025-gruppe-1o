using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Håndterer logik relateret til hotelværelser, inklusiv søgning efter ledighed.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public RoomsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Søger efter og returnerer ledige værelsestyper i en given periode for et bestemt antal gæster.
        /// </summary>
        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<RoomTypeGetDto>>> GetAvailableRoomTypes(
            [FromQuery] DateTime checkInDate,
            [FromQuery] DateTime checkOutDate,
            [FromQuery] int numberOfGuests)
        {
            if (checkOutDate <= checkInDate)
            {
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

            return Ok(result);
        }
    }
}