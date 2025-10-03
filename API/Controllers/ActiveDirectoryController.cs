using ActiveDirectoryTesting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class ActiveDirectoryController : ControllerBase
    {
        private readonly ActiveDirectoryService _adService;
        private readonly ILogger<ActiveDirectoryController> _logger;

        public ActiveDirectoryController(ActiveDirectoryService adService, ILogger<ActiveDirectoryController> logger)
        {
            _adService = adService;
            _logger = logger;
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _adService.GetAllUsers();
                return Ok(users.OrderBy(u => u.DisplayName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under hentning af alle brugere fra Active Directory.");
                return StatusCode(500, new { message = "Der opstod en intern fejl ved kommunikation med Active Directory." });
            }
        }

        [HttpGet("groups")]
        public IActionResult GetGroups()
        {
            try
            {
                var groups = _adService.AdvancedGroupSearch(hasMembers: true);
                return Ok(groups.OrderBy(g => g.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under hentning af grupper fra Active Directory.");
                return StatusCode(500, new { message = "Der opstod en intern fejl ved kommunikation med Active Directory." });
            }
        }

        // --- Administrative Endpoints ---

        [HttpPost("users")]
        public IActionResult CreateUser([FromBody] CreateUserDto dto)
        {
            var (success, message) = _adService.CreateUser(dto.Username, dto.Password, dto.FirstName, dto.LastName, dto.Email);
            if (success)
            {
                return Ok(new { message });
            }
            return BadRequest(new { message });
        }

        [HttpPut("users/{username}/reset-password")]
        public IActionResult ResetPassword(string username, [FromBody] ResetPasswordDto dto)
        {
            var (success, message) = _adService.ResetUserPassword(username, dto.NewPassword);
            if (success)
            {
                return Ok(new { message });
            }
            return NotFound(new { message });
        }

        [HttpPut("users/{username}/status")]
        public IActionResult SetUserStatus(string username, [FromBody] SetUserStatusDto dto)
        {
            var (success, message) = _adService.SetUserStatus(username, dto.IsEnabled);
            if (success)
            {
                return Ok(new { message });
            }
            return NotFound(new { message });
        }

        [HttpPost("groups/{groupName}/members")]
        public IActionResult AddUserToGroup(string groupName, [FromBody] GroupMemberDto dto)
        {
            var (success, message) = _adService.AddUserToGroup(dto.Username, groupName);
            if (success)
            {
                return Ok(new { message });
            }
            return NotFound(new { message });
        }

        [HttpDelete("groups/{groupName}/members/{username}")]
        public IActionResult RemoveUserFromGroup(string groupName, string username)
        {
            var (success, message) = _adService.RemoveUserFromGroup(username, groupName);
            if (success)
            {
                return Ok(new { message });
            }
            return NotFound(new { message });
        }
    }

    // DTOs
    public class CreateUserDto
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    }
    public class ResetPasswordDto { [Required] public string NewPassword { get; set; } = string.Empty; }
    public class SetUserStatusDto { public bool IsEnabled { get; set; } }
    public class GroupMemberDto { [Required] public string Username { get; set; } = string.Empty; }
}