using System.Collections.Generic;
using System.Threading.Tasks;
using DomainModels;

namespace API.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket?> GetTicketByIdAsync(string ticketId);
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
    }
}