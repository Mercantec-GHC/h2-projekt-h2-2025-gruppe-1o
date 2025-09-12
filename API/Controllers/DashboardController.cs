using API.Data;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Vi fjerner Authorize herfra for at styre det på hver enkelt metode.
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
        [Authorize(Roles = "Manager")] // <-- KUN brugere med "Manager"-rollen kan kalde dette.
        public async Task<ActionResult<DailyStatsDto>> GetDailyStats()
        {
            var today = DateTime.UtcNow.Date;
            var totalRooms = await _context.Rooms.CountAsync();
            if (totalRooms == 0) return Ok(new DailyStatsDto());

            var arrivals = await _context.Bookings.CountAsync(b => b.CheckInDate.Date == today);
            var departures = await _context.Bookings.CountAsync(b => b.CheckOutDate.Date == today);

            var occupiedRooms = await _context.Bookings
                .CountAsync(b => b.CheckInDate.Date <= today && b.CheckOutDate.Date > today && b.Status == "CheckedIn");

            var occupancy = (double)occupiedRooms / totalRooms * 100;

            var revenue = await _context.Bookings
                .Where(b => b.CheckOutDate.Date == today && b.Status == "CheckedOut")
                .SumAsync(b => b.TotalPrice);

            var stats = new DailyStatsDto
            {
                ArrivalsCount = arrivals,
                DeparturesCount = departures,
                OccupancyPercentage = Math.Round(occupancy, 2),
                TodaysRevenue = revenue
            };

            return Ok(stats);
        }

        //
        // --- ▼▼▼ NY METODE TILFØJET HER ▼▼▼ ---
        //

        /// <summary>
        /// Henter dagens rengøringsplan. For flere medarbejdertyper.
        /// </summary>
        [HttpGet("cleaning-schedule")]
        [Authorize(Roles = "Manager,Receptionist,Staff")] // Roller adskilles med komma.
        public IActionResult GetCleaningSchedule()
        {
            // Alle medarbejdere med en af disse roller kan se planen.
            // Her ville du normalt hente data fra databasen.
            return Ok(new
            {
                Date = DateTime.UtcNow.Date,
                Schedule = "Rengøringsplan for i dag: Værelse 101, 203, 305..."
            });
        }
    }
}