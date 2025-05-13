using ITHelpDesk.Domain.Ticket;

namespace ITHelpDesk.Repositories
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task DeleteTicketAsync(int id);
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket> UpdateTicketAsync(Ticket ticket);
        Task<Ticket> GetTicketByIdAsync(int ticketId);
        Task<IEnumerable<Ticket>> GetTicketsAsync(
            TicketStatus? status = null,
            TicketPriority? priority = null,
            string assignedToId = null,
            string submitterId = null);
        Task AddCommentAsync(TicketComment comment);
        Task<IEnumerable<TicketComment>> GetTicketCommentsAsync(int ticketId);

        Task<bool> AssignTicketAsync(int ticketId, string userId);
        Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status);
    }
}
