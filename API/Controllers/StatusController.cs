using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;

        public StatusController(ILogger<StatusController> logger)
        {
            _logger = logger;
        }

        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            try
            {
                _logger.LogInformation("HealthCheck endpoint blev kaldt kl. {Timestamp}", DateTime.UtcNow);
                return Ok(new { status = "OK", message = "API'en er kørende!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl i HealthCheck endpoint.");
                return StatusCode(500, "Health check fejlede.");
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            try
            {
                _logger.LogInformation("Ping endpoint blev kaldt.");
                return Ok(new { status = "OK", message = "Pong 🏓" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl i Ping endpoint.");
                return StatusCode(500, "Ping fejlede.");
            }
        }

        // DBHealthCheck er fjernet, da den kastede en unødvendig exception og
        // rigtige health checks bør konfigureres i Program.cs
    }
}