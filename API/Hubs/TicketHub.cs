using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace API.Hubs
{
    [Authorize]
    public class TicketHub : Hub
    {
        // Metode til at deltage i en specifik ticket-samtale
        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
        }

        // Metode til at forlade en ticket-samtale
        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
        }

        // Bemærk: Metoden til at sende selve beskeden håndterer vi via en normal API Controller
        // for at sikre, at beskeden gemmes i databasen, inden den sendes ud til klienterne.
        // Dette giver en mere robust løsning.
    }
}