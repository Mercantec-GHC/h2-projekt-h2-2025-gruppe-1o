using API.Data;
using API.Hubs;
using DomainModels;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Housekeeping, Manager")]
    public class HousekeepingController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly ILogger<HousekeepingController> _logger;

        public HousekeepingController(AppDBContext context, IHubContext<TicketHub> hubContext, ILogger<HousekeepingController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Henter en liste over alle værelser, der kræver rengøring.
        /// </summary>
        [HttpGet("rooms-to-clean")]
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

        /// <summary>
        /// Opdaterer et værelses status til 'Clean'.
        /// </summary>
        /// <param name="roomId">ID'et på det værelse, der er blevet rengjort.</param>
        [HttpPut("rooms/{roomId}/mark-as-clean")]
        public async Task<IActionResult> MarkRoomAsClean(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return NotFound($"Værelse med ID {roomId} blev ikke fundet.");
            }

            if (room.Status != "NeedsCleaning")
            {
                return BadRequest($"Værelse {room.RoomNumber} har ikke status 'NeedsCleaning' og kan ikke markeres som rengjort.");
            }

            room.Status = "Clean";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Værelse {RoomNumber} (ID: {RoomId}) er blevet markeret som 'Clean'.", room.RoomNumber, roomId);

            // Send SignalR notifikation til dashboards (f.eks. receptionist) om at status er ændret
            await _hubContext.Clients.All.SendAsync("RoomStatusChanged", roomId, room.Status);

            return NoContent();
        }
    }
}