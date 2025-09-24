using API.Data;
using API.Hubs;
using API.Repositories;
using API.Services;
using DomainModels;
using DomainModels.DTOs;
using DomainModels.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IHubContext<TicketHub> _ticketHubContext;
        private readonly AppDBContext _context;
        private readonly MailService _mailService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ITicketRepository ticketRepository,
            IHubContext<TicketHub> ticketHubContext,
            AppDBContext context,
            MailService mailService,
            ILogger<TicketsController> logger)
        {
            _ticketRepository = ticketRepository;
            _ticketHubContext = ticketHubContext;
            _context = context;
            _mailService = mailService;
            _logger = logger;
        }

        private IQueryable<Ticket> GetRoleBasedQuery(ClaimsPrincipal user)
        {
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            IQueryable<Ticket> query = _context.Tickets
                                               .Include(t => t.CreatedByUser)
                                               .Include(t => t.AssignedToUser);

            return userRole switch
            {
                "Manager" => query, // Manager kan se alt
                "Receptionist" => query.Where(t => t.Category == TicketCategory.Reception || t.Category == TicketCategory.General),
                "Housekeeping" => query.Where(t => t.Category == TicketCategory.Housekeeping),
                _ => Enumerable.Empty<Ticket>().AsQueryable(),
            };
        }

        [HttpGet("my-role/open")]
        [Authorize(Roles = "Manager,Receptionist,Housekeeping")]
        public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetOpenRoleBasedTickets()
        {
            var query = GetRoleBasedQuery(User).Where(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.PendingClosure);
            var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            var ticketDtos = tickets.Select(t => new TicketSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Category = t.Category,
                CreatedByName = t.CreatedByUser?.FirstName ?? t.GuestName ?? "N/A",
                AssignedToName = t.AssignedToUser?.FirstName ?? "Ikke tildelt",
                CreatedAt = t.CreatedAt
            });
            return Ok(ticketDtos);
        }

        [HttpGet("my-role/closed")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetClosedRoleBasedTickets()
        {
            var sixtyDaysAgo = DateTime.UtcNow.AddDays(-60);
            var query = GetRoleBasedQuery(User).Where(t => t.Status == TicketStatus.Closed && t.UpdatedAt > sixtyDaysAgo);

            var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            var ticketDtos = tickets.Select(t => new TicketSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Category = t.Category,
                CreatedByName = t.CreatedByUser?.FirstName ?? t.GuestName ?? "N/A",
                AssignedToName = t.AssignedToUser?.FirstName ?? "Ikke tildelt",
                CreatedAt = t.CreatedAt
            });
            return Ok(ticketDtos);
        }

        [HttpGet("my-tickets")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetMyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var tickets = await _context.Tickets
                .Where(t => t.CreatedByUserId == userId)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketSummaryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    Category = t.Category,
                    CreatedByName = "Mig",
                    AssignedToName = t.AssignedToUser != null ? t.AssignedToUser.FirstName : "Ikke tildelt",
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
            return Ok(tickets);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TicketDetailDto>> GetTicketById(string id)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound();

            var ticketDto = new TicketDetailDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Category = ticket.Category,
                CreatedByName = ticket.CreatedByUser?.FirstName ?? ticket.GuestName ?? "N/A",
                AssignedToName = ticket.AssignedToUser?.FirstName ?? "Ikke tildelt",
                CreatedAt = ticket.CreatedAt,
                Messages = ticket.Messages.Select(m => new TicketMessageDto
                {
                    Id = m.Id,
                    SenderName = m.Sender.FirstName,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    IsInternalNote = m.IsInternalNote,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };
            return Ok(ticketDto);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<TicketSummaryDto>> CreateTicket(TicketCreateDto ticketDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userId != null ? await _context.Users.FindAsync(userId) : null;

            if (user == null && (string.IsNullOrWhiteSpace(ticketDto.GuestName) || string.IsNullOrWhiteSpace(ticketDto.GuestEmail)))
            {
                return BadRequest("Navn og email er påkrævet for gæster.");
            }

            var newTicket = new Ticket
            {
                Id = Guid.NewGuid().ToString(),
                Title = ticketDto.Title,
                Description = ticketDto.Description,
                Category = ticketDto.Category,
                CreatedByUserId = userId,
                GuestName = user == null ? ticketDto.GuestName : null,
                GuestEmail = user == null ? ticketDto.GuestEmail : null,
                Status = TicketStatus.Open
            };

            var createdTicket = await _ticketRepository.CreateTicketAsync(newTicket);

            var recipientEmail = user?.Email ?? createdTicket.GuestEmail;
            var recipientName = user?.FirstName ?? createdTicket.GuestName;

            if (!string.IsNullOrEmpty(recipientEmail))
            {
                var subject = $"Vi har modtaget din henvendelse (Sag: #{createdTicket.Id.Substring(0, 6).ToUpper()})";
                var body = $@"
                    <h1>Hej {recipientName},</h1>
                    <p>Tak for din henvendelse. Vi har oprettet en sag med følgende oplysninger:</p>
                    <ul>
                        <li><strong>Sagsnummer:</strong> #{createdTicket.Id.Substring(0, 6).ToUpper()}</li>
                        <li><strong>Emne:</strong> {createdTicket.Title}</li>
                    </ul>
                    <p>Vi vender tilbage til dig hurtigst muligt.</p>
                    <p>Med venlig hilsen,<br>Flyhigh Hotel Support</p>";

                bool emailSent = await _mailService.SendEmailAsync(recipientEmail, subject, body);
                if (!emailSent)
                {
                    _logger.LogWarning("Sag {TicketId} blev oprettet, men bekræftelsesmail kunne ikke sendes til {Email}.", createdTicket.Id, recipientEmail);
                }
            }

            var resultDto = new TicketSummaryDto
            {
                Id = createdTicket.Id,
                Title = createdTicket.Title,
                Status = createdTicket.Status,
                Category = createdTicket.Category,
                CreatedByName = user?.FirstName ?? createdTicket.GuestName ?? "N/A",
                CreatedAt = createdTicket.CreatedAt
            };

            await _ticketHubContext.Clients.All.SendAsync("NewTicketCreated", resultDto);

            return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, resultDto);
        }

        [HttpPost("{id}/messages")]
        [Authorize]
        public async Task<ActionResult<TicketMessageDto>> PostMessage(string id, TicketMessageCreateDto messageDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized("Bruger ikke fundet.");

            var ticket = await _context.Tickets.Include(t => t.CreatedByUser).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound("Ticket ikke fundet.");

            var message = new TicketMessage
            {
                Id = Guid.NewGuid().ToString(),
                TicketId = id,
                SenderId = userId,
                Content = messageDto.Content,
                IsInternalNote = messageDto.IsInternalNote
            };
            _context.TicketMessages.Add(message);

            var senderRole = user.Role?.Name;
            bool isStaff = senderRole is "Manager" or "Receptionist" or "Housekeeping";

            if (isStaff)
            {
                ticket.Status = TicketStatus.PendingCustomerReply;
            }
            else
            {
                ticket.Status = TicketStatus.PendingSupportReply;
            }

            await _context.SaveChangesAsync();
            await _ticketHubContext.Clients.Group($"Ticket_{id}").SendAsync("TicketStatusChanged", id, ticket.Status);

            if (isStaff && !message.IsInternalNote)
            {
                var recipientEmail = ticket.CreatedByUser?.Email ?? ticket.GuestEmail;
                var recipientName = ticket.CreatedByUser?.FirstName ?? ticket.GuestName;

                if (!string.IsNullOrEmpty(recipientEmail))
                {
                    var subject = $"Nyt svar på din sag #{ticket.Id.Substring(0, 6).ToUpper()}";
                    var body = $@"
                        <h1>Hej {recipientName},</h1>
                        <p>Der er kommet et nyt svar på din sag '{ticket.Title}'.</p>
                        <hr>
                        <p><strong>{user.FirstName} skriver:</strong></p>
                        <p><em>{message.Content}</em></p>
                        <hr>
                        <p>Du kan se hele samtalen i vores support-sektion.</p>
                        <p>Med venlig hilsen,<br>Flyhigh Hotel Support</p>";

                    bool emailSent = await _mailService.SendEmailAsync(recipientEmail, subject, body);
                    if (!emailSent)
                    {
                        _logger.LogWarning("Svar tilføjet til sag {TicketId}, men notifikationsmail kunne ikke sendes til {Email}.", ticket.Id, recipientEmail);
                    }
                }
            }

            var resultDto = new TicketMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = user.FirstName,
                Content = message.Content,
                IsInternalNote = message.IsInternalNote,
                CreatedAt = message.CreatedAt
            };

            await _ticketHubContext.Clients.Group($"Ticket_{id}").SendAsync("ReceiveMessage", resultDto);

            return Ok(resultDto);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Manager,Receptionist,Housekeeping,User")]
        public async Task<IActionResult> UpdateTicketStatus(string id, [FromBody] TicketStatusUpdateDto statusUpdateDto)
        {
            var ticket = await _context.Tickets.Include(t => t.CreatedByUser).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            bool isStaff = userRole is "Manager" or "Receptionist" or "Housekeeping";
            bool isOwner = ticket.CreatedByUserId == userId;

            if (isStaff && statusUpdateDto.NewStatus == TicketStatus.Closed)
            {
                ticket.Status = TicketStatus.PendingClosure;
            }
            else if (isOwner && ticket.Status == TicketStatus.PendingClosure && (statusUpdateDto.NewStatus == TicketStatus.Closed || statusUpdateDto.NewStatus == TicketStatus.PendingSupportReply))
            {
                ticket.Status = statusUpdateDto.NewStatus;
            }
            else if (isStaff)
            {
                ticket.Status = statusUpdateDto.NewStatus;
            }
            else
            {
                return Forbid();
            }

            await _context.SaveChangesAsync();
            await _ticketHubContext.Clients.Group($"Ticket_{id}").SendAsync("TicketStatusChanged", id, ticket.Status);

            return NoContent();
        }
    }
}