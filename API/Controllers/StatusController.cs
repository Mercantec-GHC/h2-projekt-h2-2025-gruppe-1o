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
            _logger.LogInformation("HealthCheck endpoint blev kaldt kl. {Timestamp}", DateTime.UtcNow);
            return Ok(new { status = "OK", message = "API'en er kørende!" });
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            _logger.LogInformation("Ping endpoint blev kaldt.");
            return Ok(new { status = "OK", message = "Pong 🏓" });
        }

        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            _logger.LogInformation("Kører bevidst test-fejl-endpoint.");
            throw new Exception("Dette er en bevidst test-fejl for at verificere den globale handler!");
        }

    }
}