using ITHelpDesk.Data;
using ITHelpDesk.Domain.Ticket;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly HelpDeskDbContext _context;

        public TicketRepository(HelpDeskDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);

            //load the related Submitter data
            await _context.Entry(ticket)
                .Reference(t => t.Submitter)
                .LoadAsync();

            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
        {
            var existingTicket = await _context.Tickets.FindAsync(ticket.Id);
            if (existingTicket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Entry(existingTicket).CurrentValues.SetValues(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            /*return await _context.Tickets
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == ticketId);*/

            return await _context.Tickets.FindAsync(ticketId);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(
            TicketStatus? status = null,
            TicketPriority? priority = null,
            string assignedToId = null,
            string submitterId = null)
        {
            var query = _context.Tickets
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrEmpty(assignedToId))
                query = query.Where(t => t.AssignedToId == assignedToId);

            if (!string.IsNullOrEmpty(submitterId))
                query = query.Where(t => t.SubmitterId == submitterId);

            return await query.ToListAsync();
        }

        public async Task AddCommentAsync(TicketComment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TicketComment>> GetTicketCommentsAsync(int ticketId)
        {
            return await _context.TicketComments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AssignTicketAsync(int ticketId, string userId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.AssignedToId = userId;
            ticket.Status = TicketStatus.Assigned;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}