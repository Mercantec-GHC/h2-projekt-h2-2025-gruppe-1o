using API.Data;
using API.Hubs;
using DomainModels;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RoomsController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IHubContext<TicketHub> _hubContext;

        public RoomsController(AppDBContext context, ILogger<RoomsController> logger, IMemoryCache cache, IHubContext<TicketHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _hubContext = hubContext;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetRoomTypes([FromQuery] string? sortBy, [FromQuery] bool desc = false)
        {
            var cacheKey = $"allRoomTypes_sortBy={sortBy ?? "default"}_desc={desc}";
            if (_cache.TryGetValue(cacheKey, out List<RoomType> cachedRoomTypes))
            {
                return Ok(cachedRoomTypes);
            }

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
            var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(cacheKey, roomTypesFromDb, cacheOptions);
            return Ok(roomTypesFromDb);
        }

        [HttpGet("types/{id}")]
        public async Task<ActionResult<RoomTypeDetailDto>> GetRoomTypeById(int id)
        {
            var roomType = await _context.RoomTypes.Include(rt => rt.Services).FirstOrDefaultAsync(rt => rt.Id == id);
            if (roomType == null) return NotFound();
            var dto = new RoomTypeDetailDto
            {
                Id = roomType.Id,
                Name = roomType.Name,
                ShortDescription = roomType.ShortDescription,
                LongDescription = roomType.LongDescription,
                BasePrice = roomType.BasePrice,
                Capacity = roomType.Capacity,
                Services = roomType.Services.Select(s => new ServiceGetDto { /* Mapping */ }).ToList()
            };
            return Ok(dto);
        }

        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<RoomTypeGetDto>>> GetAvailableRoomTypes([FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate, [FromQuery] int numberOfGuests)
        {
            var checkIn = DateOnly.FromDateTime(checkInDate);
            var checkOut = DateOnly.FromDateTime(checkOutDate);
            if (checkOut <= checkIn) return BadRequest("Check-ud dato skal være efter check-in dato.");
            var bookedCounts = await _context.Bookings
                .Where(b => b.Status != "Cancelled" && DateOnly.FromDateTime(b.CheckInDate) < checkOut && DateOnly.FromDateTime(b.CheckOutDate) > checkIn)
                .GroupBy(b => b.RoomTypeId)
                .Select(g => new { RoomTypeId = g.Key, BookedCount = g.Count() })
                .ToDictionaryAsync(x => x.RoomTypeId, x => x.BookedCount);
            var roomTypes = await _context.RoomTypes.Include(rt => rt.Rooms).Where(rt => rt.Capacity >= numberOfGuests).ToListAsync();
            var availableRoomTypes = roomTypes.Select(rt => new RoomTypeGetDto
            {
                Id = rt.Id,
                Name = rt.Name,
                Description = rt.ShortDescription,
                BasePrice = rt.BasePrice,
                Capacity = rt.Capacity,
                AvailableCount = rt.Rooms.Count - bookedCounts.GetValueOrDefault(rt.Id, 0)
            }).Where(dto => dto.AvailableCount > 0).ToList();
            return Ok(availableRoomTypes);
        }

        [HttpPut("{roomId}/request-cleaning")]
        [Authorize(Roles = "Receptionist, Manager")]
        public async Task<IActionResult> RequestRoomCleaning(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return NotFound($"Værelse med ID {roomId} blev ikke fundet.");
            room.Status = "NeedsCleaning";
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("RoomStatusChanged", roomId, room.Status);
            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist, Manager")]
        public async Task<ActionResult<IEnumerable<RoomGetDto>>> GetAllRooms()
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Select(r => new RoomGetDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    Status = r.Status,
                    RoomTypeName = r.RoomType.Name
                })
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }

        [HttpGet("needs-cleaning")]
        [Authorize(Roles = "Housekeeping, Manager")]
        public async Task<ActionResult<IEnumerable<RoomGetDto>>> GetRoomsNeedingCleaning()
        {
            return await _context.Rooms
                .Where(r => r.Status == "NeedsCleaning")
                .Include(r => r.RoomType)
                .Select(r => new RoomGetDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    Status = r.Status,
                    RoomTypeName = r.RoomType.Name
                })
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }
    }
}