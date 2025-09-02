using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Gør den offentligt tilgængelig for nem test
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetVersion()
        {
            // Dette er vores test-markør
            return Ok(new { Version = "1.0.1", Besked = "Deployment virker!" });
        }
    }
}