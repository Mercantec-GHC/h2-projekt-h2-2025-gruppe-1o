using API.Data;
using API.Services;
using DomainModels;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<UsersController> _logger;
        private readonly LoginAttemptService _loginAttemptService;
        private readonly ActiveDirectoryTesting.ActiveDirectoryService _adService; // TILFØJ DENNE

        public UsersController(
            AppDBContext context,
            JwtService jwtService,
            ILogger<UsersController> logger,
            LoginAttemptService loginAttemptService,
            ActiveDirectoryTesting.ActiveDirectoryService adService) // TILFØJ DENNE
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _loginAttemptService = loginAttemptService;
            _adService = adService; // TILFØJ DENNE
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDetailDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserUpdateDto userDto)
        {
            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null) return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userToUpdate.Id != currentUserId && !User.IsInRole("Admin")) return Forbid();

            userToUpdate.FirstName = userDto.FirstName;
            userToUpdate.LastName = userDto.LastName;
            userToUpdate.Email = userDto.Email;
            userToUpdate.PhoneNumber = userDto.PhoneNumber ?? string.Empty;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("En bruger med denne email findes allerede.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null) throw new InvalidOperationException("Systemkonfigurationsfejl: Standard brugerrolle 'User' mangler.");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                HashedPassword = hashedPassword,
                RoleId = userRole.Id,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bruger oprettet!", userId = user.Id });
        }

        [AllowAnonymous]
        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin(StaffLoginDto dto)
        {
            // 1. Valider mod Active Directory med det direkte brugernavn
            var isValidAdUser = _adService.ValidateUserCredentials(dto.Username, dto.Password);
            if (!isValidAdUser)
            {
                return Unauthorized("Forkert medarbejder-login eller adgangskode.");
            }

            // 2. Hent brugeroplysninger (inkl. telefonnummer) og grupper fra AD
            var adUser = _adService.GetUserWithGroups(dto.Username);
            if (adUser == null || string.IsNullOrWhiteSpace(adUser.Email))
            {
                return StatusCode(500, "Brugeren mangler en email i Active Directory og kan ikke logges ind.");
            }

            // 3. Find eller opret bruger i lokal database
            var localUser = await _context.Users
                                          .Include(u => u.Role)
                                          .FirstOrDefaultAsync(u => u.Email.ToLower() == adUser.Email.ToLower());

            if (localUser == null)
            {
                // Brugeren oprettes
                localUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = adUser.Email,
                    FirstName = adUser.FirstName,
                    LastName = adUser.LastName,
                    PhoneNumber = adUser.Phone, // <-- TILFØJ HER for nye brugere
                    HashedPassword = "EXTERNALLY_MANAGED"
                };
                _context.Users.Add(localUser);
            }
            else
            {
                // Brugeren opdateres
                localUser.FirstName = adUser.FirstName;
                localUser.LastName = adUser.LastName;
                localUser.PhoneNumber = adUser.Phone; // <-- TILFØJ HER for eksisterende brugere
            }

            // 4. Synkroniser roller
            var roleNameFromAd = adUser.Groups.FirstOrDefault(g => g == "Admin" || g == "Manager" || g == "Receptionist") ?? "Staff";
            var localRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleNameFromAd);
            if (localRole == null)
            {
                localRole = new Role { Id = Guid.NewGuid().ToString(), Name = roleNameFromAd };
                _context.Roles.Add(localRole);
            }
            localUser.Role = localRole;

            localUser.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 5. Generer JWT Token
            var token = _jwtService.GenerateToken(localUser);

            return Ok(new
            {
                token,
                user = new { id = localUser.Id, email = localUser.Email, firstName = localUser.FirstName, role = localUser.Role.Name }
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Bruger ikke fundet.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.HashedPassword))
            {
                return BadRequest("Den nuværende adgangskode er ikke korrekt.");
            }

            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.HashedPassword = newHashedPassword;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Adgangskode blev opdateret succesfuldt." });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // TRIN 1: Forsøg at finde brugeren via email, som normalt.
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            bool isAuthenticated = false;

            if (user != null)
            {
                // Brugeren blev fundet i databasen. Nu tjekker vi, hvordan de skal valideres.
                if (user.HashedPassword == "EXTERNALLY_MANAGED" || user.HashedPassword == "EXTERNAL_MANAGED")
                {
                    // Dette scenarie er svært, for vi har kun brugerens DB-email, ikke nødvendigvis deres AD-login.
                    // SÅ vi springer videre og lader TRIN 2 håndtere AD-validering.
                }
                else
                {
                    // Dette er en almindelig bruger med et lokalt password.
                    isAuthenticated = BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword);
                    if (isAuthenticated)
                    {
                        // Hvis password er korrekt, log ind.
                        return await CompleteLogin(user);
                    }
                }
            }

            // TRIN 2: Hvis vi når hertil, betyder det enten:
            // a) Brugeren blev ikke fundet via email (fordi de loggede ind med AD-navn).
            // b) Brugeren blev fundet, men er markeret som ekstern.
            // Vi prøver derfor at validere direkte mod Active Directory.

            // Vi antager, at det, der blev skrevet i email-feltet, er AD-brugernavnet.
            var adUsername = dto.Email;

            try
            {
                if (_adService.ValidateUserCredentials(adUsername, dto.Password))
                {
                    // AD-login var en succes! Nu henter vi brugerens detaljer fra AD.
                    var adUser = _adService.GetUserWithGroups(adUsername);
                    if (adUser != null && !string.IsNullOrEmpty(adUser.Email))
                    {
                        // Nu bruger vi den KORREKTE email fra AD til at finde brugeren i VORES database.
                        var localUserFromAd = await _context.Users.Include(u => u.Role)
                                                   .FirstOrDefaultAsync(u => u.Email.ToLower() == adUser.Email.ToLower());

                        if (localUserFromAd != null)
                        {
                            // Vi fandt den matchende lokale bruger! Log dem ind.
                            return await CompleteLogin(localUserFromAd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hvis der sker en fejl under AD-kaldet (f.eks. server nede), log det.
                _logger.LogError(ex, "Fejl under forsøg på AD-validering i det primære login-flow.");
                // Fald tilbage til generisk fejl for ikke at lække systeminformation.
                return Unauthorized("Forkert email eller adgangskode");
            }


            // Hvis ingen af metoderne virkede, afvis.
            return Unauthorized("Forkert email eller adgangskode");
        }

        // Hjælpe-metode for at undgå at gentage kode
        private async Task<IActionResult> CompleteLogin(User user)
        {
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new { id = user.Id, email = user.Email, firstName = user.FirstName, lastName = user.LastName, role = user.Role?.Name ?? "User" }
            });
        }
        [AllowAnonymous]
        [HttpPost("test-ad")] // Ændret fra HttpGet til HttpPost
        public IActionResult TestAdConnection([FromBody] TestAdCredentialsDto dto)
        {
            _logger.LogInformation($"--- Starter AD Forbindelsestest for bruger: {dto.Username} ---");
            try
            {
                // Vi bruger de oplysninger, du sender med, til at validere.
                _logger.LogInformation($"Tester validering med '{dto.Username}'...");
                bool isValid = _adService.ValidateUserCredentials(dto.Username, dto.Password);

                if (isValid)
                {
                    _logger.LogInformation($"AD-validering for '{dto.Username}' var en succes!");
                    return Ok(new { status = "Succes", message = $"Bruger '{dto.Username}' blev valideret succesfuldt mod Active Directory." });
                }
                else
                {
                    _logger.LogError($"AD-validering for '{dto.Username}' fejlede (forkert brugernavn/kodeord?).");
                    // VIGTIGT: Returner 400 Bad Request, ikke 500, hvis det bare er forkerte credentials.
                    return BadRequest(new { status = "Fejl", message = "Forbindelsen til AD var OK, men brugeroplysningerne var forkerte." });
                }
            }
            catch (Exception ex)
            {
                // Fanger fatale fejl som f.eks. netværksproblemer.
                _logger.LogError(ex, "FATAL FEJL under AD-forbindelsestest.");
                return StatusCode(500, new
                {
                    status = "Fatal Fejl",
                    message = "Der skete en fejl under forsøget på at forbinde til Active Directory.",
                    error = new
                    {
                        type = ex.GetType().ToString(),
                        errorMessage = ex.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        }
    }
}
