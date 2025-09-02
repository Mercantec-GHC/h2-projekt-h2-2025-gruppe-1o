using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetVersion()
        {
            // Henter automatisk versionen fra din API.dll fil
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
            return Ok(new { Version = version, Status = "API er online" });
        }
    }
}