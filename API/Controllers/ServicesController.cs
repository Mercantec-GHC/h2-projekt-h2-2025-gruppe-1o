using API.Data;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainModels.Enums;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public ServicesController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceGetDto>>> GetAvailableServices()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .Select(s => new ServiceGetDto // RETTELSE: Bruger nu den eksisterende ServiceGetDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Category = s.Category,
                    Price = s.Price,
                    BillingType = s.BillingType.ToString()
                })
                .ToListAsync();

            return Ok(services);
        }
    }
}