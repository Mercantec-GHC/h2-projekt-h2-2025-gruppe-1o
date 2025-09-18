using API.Data;
using DomainModels;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingRoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public MeetingRoomsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/meetingrooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeetingRoomGetDto>>> GetMeetingRooms()
        {
            return await _context.MeetingRooms
                .Select(mr => new MeetingRoomGetDto
                {
                    Id = mr.Id,
                    Name = mr.Name,
                    Capacity = mr.Capacity,
                    HourlyRate = mr.HourlyRate,
                    Description = mr.Description,
                    ImageUrl = mr.ImageUrl
                })
                .ToListAsync();
        }

        // GET: api/meetingrooms/availability/1?date=2025-10-20
        [HttpGet("availability/{id}")]
        public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetAvailability(int id, [FromQuery] DateTime date)
        {
            // RETTELSE: Konverter den indkommende dato til UTC
            var dateUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            var startOfDay = dateUtc;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.MeetingRoomBookings
                .Where(b => b.MeetingRoomId == id && b.StartTime >= startOfDay && b.StartTime < endOfDay)
                .Select(b => new TimeSlotDto { StartTime = b.StartTime, EndTime = b.EndTime })
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }

        // POST: api/meetingrooms/book
        [HttpPost("book")]
        public async Task<IActionResult> BookMeetingRoom(MeetingRoomBookingCreateDto bookingDto)
        {
            // RETTELSE: Konverter også her datoerne til UTC for at sikre konsistens
            var startTimeUtc = DateTime.SpecifyKind(bookingDto.StartTime, DateTimeKind.Utc);
            var endTimeUtc = DateTime.SpecifyKind(bookingDto.EndTime, DateTimeKind.Utc);

            if (startTimeUtc >= endTimeUtc || startTimeUtc < DateTime.UtcNow)
            {
                return BadRequest("Ugyldig tidsperiode.");
            }

            var meetingRoom = await _context.MeetingRooms.FindAsync(bookingDto.MeetingRoomId);
            if (meetingRoom == null)
            {
                return NotFound("Mødelokalet findes ikke.");
            }

            var isOverlapping = await _context.MeetingRoomBookings
                .AnyAsync(b => b.MeetingRoomId == bookingDto.MeetingRoomId &&
                               b.StartTime < endTimeUtc &&
                               b.EndTime > startTimeUtc);

            if (isOverlapping)
            {
                return Conflict("Den valgte tid er allerede booket.");
            }

            var durationHours = (endTimeUtc - startTimeUtc).TotalHours;
            var totalPrice = (decimal)durationHours * meetingRoom.HourlyRate;

            var newBooking = new MeetingRoomBooking
            {
                MeetingRoomId = bookingDto.MeetingRoomId,
                StartTime = startTimeUtc,
                EndTime = endTimeUtc,
                BookedByName = bookingDto.BookedByName,
                BookedByEmail = bookingDto.BookedByEmail,
                TotalPrice = totalPrice
            };

            _context.MeetingRoomBookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mødelokalet er nu booket.", bookingId = newBooking.Id });
        }
    }
}