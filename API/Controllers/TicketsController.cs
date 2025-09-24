using API.Data;
using API.Hubs;
using API.Repositories;
using DomainModels;
using DomainModels.DTOs;
using DomainModels.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public TicketsController(ITicketRepository ticketRepository, IHubContext<TicketHub> ticketHubContext, AppDBContext context)
        {
            _ticketRepository = ticketRepository;
            _ticketHubContext = ticketHubContext;
            _context = context;
        }

        private IQueryable<Ticket> GetRoleBasedQuery(ClaimsPrincipal user)
        {
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            IQueryable<Ticket> query = _context.Tickets
                                               .Include(t => t.CreatedByUser)
                                               .Include(t => t.AssignedToUser);

            switch (userRole)
            {
                case "Manager":
                    return query.Where(t => t.Category == TicketCategory.Manager || t.Category == TicketCategory.General);
                case "Receptionist":
                    return query.Where(t => t.Category == TicketCategory.Reception);
                case "Housekeeping":
                    return query.Where(t => t.Category == TicketCategory.Housekeeping);
                default:
                    return Enumerable.Empty<Ticket>().AsQueryable();
            }
        }

        [HttpGet("my-role/open")]
        [Authorize(Roles = "Manager,Receptionist,Housekeeping")]
        public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetOpenRoleBasedTickets()
        {
            var query = GetRoleBasedQuery(User).Where(t => t.Status != TicketStatus.Closed);
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
        [Authorize(Roles = "Manager,Receptionist,Housekeeping")]
        public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetClosedRoleBasedTickets()
        {
            var query = GetRoleBasedQuery(User).Where(t => t.Status == TicketStatus.Closed);
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
            if (userId == null)
            {
                return Unauthorized();
            }

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
            if (ticket == null)
            {
                return NotFound();
            }
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
            var userName = User.FindFirstValue(ClaimTypes.Name);

            if (userId == null && (string.IsNullOrWhiteSpace(ticketDto.GuestName) || string.IsNullOrWhiteSpace(ticketDto.GuestEmail)))
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
                GuestName = userId == null ? ticketDto.GuestName : null,
                GuestEmail = userId == null ? ticketDto.GuestEmail : null,
                Status = TicketStatus.Open
            };

            var createdTicket = await _ticketRepository.CreateTicketAsync(newTicket);

            var resultDto = new TicketSummaryDto
            {
                Id = createdTicket.Id,
                Title = createdTicket.Title,
                Status = createdTicket.Status,
                Category = createdTicket.Category,
                CreatedByName = userName ?? createdTicket.GuestName ?? "N/A",
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

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound("Ticket ikke fundet.");

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized("Bruger ikke fundet.");

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
            bool isStaff = senderRole == "Manager" || senderRole == "Receptionist" || senderRole == "Housekeeping";

            if (isStaff)
            {
                ticket.Status = TicketStatus.PendingCustomerReply;
            }
            else
            {
                ticket.Status = TicketStatus.PendingSupportReply;
            }

            await _context.SaveChangesAsync();

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
            await _ticketHubContext.Clients.Group($"Ticket_{id}").SendAsync("TicketStatusChanged", id, ticket.Status);

            return Ok(resultDto);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Manager,Receptionist,Housekeeping,User")] // User tilføjet for at de kan svare
        public async Task<IActionResult> UpdateTicketStatus(string id, [FromBody] TicketStatusUpdateDto statusUpdateDto)
        {
            var ticket = await _context.Tickets.Include(t => t.CreatedByUser).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            bool isStaff = userRole == "Manager" || userRole == "Receptionist" || userRole == "Housekeeping";
            bool isOwner = ticket.CreatedByUserId == userId;

            // Logik for at sætte sagen til lukning
            if (isStaff && statusUpdateDto.NewStatus == TicketStatus.Closed)
            {
                ticket.Status = TicketStatus.PendingClosure;
            }
            // Logik for kundens svar på lukning
            else if (isOwner && ticket.Status == TicketStatus.PendingClosure)
            {
                ticket.Status = statusUpdateDto.NewStatus; // Bliver enten Closed eller PendingSupportReply
            }
            // Almindelig statusopdatering af medarbejder
            else if (isStaff)
            {
                ticket.Status = statusUpdateDto.NewStatus;
            }
            else
            {
                return Forbid(); // Almindelige brugere kan ikke bare ændre status
            }

            await _context.SaveChangesAsync();

            await _ticketHubContext.Clients.Group($"Ticket_{id}").SendAsync("TicketStatusChanged", id, ticket.Status);

            return NoContent();
        }
    }
}