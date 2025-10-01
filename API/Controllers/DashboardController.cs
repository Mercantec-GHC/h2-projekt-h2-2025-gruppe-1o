using API.Data;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDBContext _context;

        public DashboardController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter daglige statistikker. Er nu kun tilgængelig for Managere.
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<DailyStatsDto>> GetDailyStats()
        {
            var today = DateTime.UtcNow.Date;
            var totalRooms = await _context.Rooms.CountAsync();
            if (totalRooms == 0) return Ok(new DailyStatsDto());

            var arrivals = await _context.Bookings.CountAsync(b => b.CheckInDate.Date == today && b.Status != "Cancelled");
            var departures = await _context.Bookings.CountAsync(b => b.CheckOutDate.Date == today && b.Status != "Cancelled");

            // --- START: RETTELSE AF LOGIK ---
            // Belægning baseres nu på alle bookede (ikke-annullerede) værelser, der er aktive i dag.
            var occupiedRooms = await _context.Bookings
                .CountAsync(b => b.CheckInDate.Date <= today && b.CheckOutDate.Date > today && b.Status != "Cancelled");

            var occupancy = (double)occupiedRooms / totalRooms * 100;

            // Omsætning baseres nu på alle ikke-annullerede bookinger med check-ud i dag.
            var revenue = await _context.Bookings
                .Where(b => b.CheckOutDate.Date == today && b.Status != "Cancelled")
                .SumAsync(b => b.TotalPrice);
            // --- SLUT: RETTELSE AF LOGIK ---

            var stats = new DailyStatsDto
            {
                ArrivalsCount = arrivals,
                DeparturesCount = departures,
                OccupancyPercentage = Math.Round(occupancy, 2),
                TodaysRevenue = revenue
            };

            return Ok(stats);
        }

        // ----- NY METODE TIL RECEPTIONIST DASHBOARD -----
        /// <summary>
        /// Henter samlet data til receptionistens dashboard.
        /// </summary>
        [HttpGet("receptionist")]
        [Authorize(Roles = "Receptionist, Manager")]
        public async Task<ActionResult<ReceptionistDashboardDto>> GetReceptionistDashboardData()
        {
            var today = DateTime.UtcNow.Date;

            var arrivals = await _context.Bookings
                .Where(b => b.CheckInDate.Date == today && b.Status != "Cancelled")
                .Include(b => b.User)
                .Include(b => b.RoomType)
                .Select(b => new BookingSummaryDto
                {
                    Id = b.Id,
                    GuestFullName = $"{b.User.FirstName} {b.User.LastName}",
                    RoomTypeName = b.RoomType.Name,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status
                }).ToListAsync();

            var departures = await _context.Bookings
                .Where(b => b.CheckOutDate.Date == today && b.Status != "Cancelled")
                .Include(b => b.User)
                .Include(b => b.RoomType)
                .Select(b => new BookingSummaryDto
                {
                    Id = b.Id,
                    GuestFullName = $"{b.User.FirstName} {b.User.LastName}",
                    RoomTypeName = b.RoomType.Name,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status
                }).ToListAsync();

            var totalRoomsCount = await _context.Rooms.CountAsync();
            var occupiedRoomsCount = await _context.Bookings
                .CountAsync(b => b.CheckInDate.Date <= today && b.CheckOutDate.Date > today && b.Status != "Cancelled");

            var dashboardData = new ReceptionistDashboardDto
            {
                TodaysArrivals = arrivals,
                TodaysDepartures = departures,
                OccupiedRoomsCount = occupiedRoomsCount,
                AvailableRoomsCount = totalRoomsCount - occupiedRoomsCount
            };

            return Ok(dashboardData);
        }
        // ---------------------------------------------------

        /// <summary>
        /// Henter dagens rengøringsplan. For flere medarbejdertyper.
        /// </summary>
        [HttpGet("cleaning-schedule")]
        [Authorize(Roles = "Manager,Receptionist,Staff")]
        public IActionResult GetCleaningSchedule()
        {
            // Her ville du normalt hente data fra databasen.
            return Ok(new
            {
                Date = DateTime.UtcNow.Date,
                Schedule = "Rengøringsplan for i dag: Værelse 101, 203, 305..."
            });
        }
    }
}