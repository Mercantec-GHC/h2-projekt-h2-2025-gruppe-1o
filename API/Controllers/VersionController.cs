using API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class VersionController : ControllerBase
    {
        private readonly AppDBContext _context;

        public VersionController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
            return Ok(new { Version = version, Status = "API er online" });
        }

        // NY TEST-METODE
        [HttpGet("testdb")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                return Ok(new { Status = "Succes", Message = $"Der er forbindelse til databasen. Antal brugere fundet: {userCount}" });
            }
            catch (Exception ex)
            {
                // Returner den specifikke fejl, hvis forbindelsen fejler
                return StatusCode(500, new { Status = "Fejl", Message = "Kunne ikke forbinde til databasen.", Error = ex.Message, InnerError = ex.InnerException?.Message });
            }
        }
    }
}