using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ITHelpDesk.Domain;
using ITHelpDesk.Repositories;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ITHelpDesk.Data;
using ITHelpDesk.Helpers;
using ITHelpDesk.Services;
using ITHelpDesk.Domain.Ticket;
using ITHelpDesk.DTOs;

namespace ITHelpDesk.Controllers
{
    [Route("api/Tickets")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly HelpDeskDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITicketRepository _ticketRepository;
        //private readonly ISmsService _smsService;

        private readonly EmailService _emailService;

        public TicketsController(
                HelpDeskDbContext context,
                UserManager<ApplicationUser> userManager,
                ITicketRepository ticketRepository,
                EmailService emailService,
                //ISmsService smsService,
                ILogger<TicketsController> logger)
        {
            _context = context;
            _ticketRepository = ticketRepository;
            _userManager = userManager;
            _emailService = emailService;
            //_smsService = smsService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] Ticket ticket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (ticket == null)
                return BadRequest("Invalid request payload. Ensure all required fields are provided.");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User is not authenticated.");

            ticket.SubmitterId = userId;
            ticket.Submitter = null;
            ticket.Status = TicketStatus.New; //Default Status for new ticket = New

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTicket = await _ticketRepository.CreateTicketAsync(ticket);

            // Fetch & Notify IT & Admin Users via SMS & FCM
            var itAdminUsers = await _userManager.Users
            .Where(u => _context.UserRoles.Any(r =>
                r.UserId == u.Id && _context.Roles
                    .Where(role => role.Name == "IT" || role.Name == "Admin")
                    .Select(role => role.Id)
                    .Contains(r.RoleId)
            ))
            .ToListAsync();

            foreach (var user in itAdminUsers)
            {
                await _emailService.SendEmailAsync(new List<string> { user.Email },
                    "New Ticket Created",
                    $"A new ticket '{ticket.Title}' has been created by {ticket.Submitter.FirstName} {ticket.Submitter.LastName}. Check the Help Desk system.");
            }

            //var itAdminPhoneNumbers = itAdminUsers.Select(u => u.PhoneNumber).ToList();

            //await _smsService.NotifyTicketCreationAsync(
            //    createdTicket.Id.ToString(),
            //    createdTicket.Title,
            //    (ticket.Submitter.FirstName + " " + ticket.Submitter.MiddleName + " " + ticket.Submitter.LastName),
            //    itAdminPhoneNumbers
            //);

            return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, createdTicket);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var ticket = await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            // Automatically mark ticket as viewed when accessing details
            // First, check if it's already viewed by this user
            var existingView = await _context.TicketViews
                .FirstOrDefaultAsync(tv => tv.TicketId == id && tv.UserId == userId);

            if (existingView == null)
            {
                var ticketView = new TicketView
                {
                    TicketId = id,
                    UserId = userId,
                    ViewedAt = DateTime.UtcNow
                };

                _context.TicketViews.Add(ticketView);

                // Update the ticket entity (need to get a tracked instance)
                var trackedTicket = await _context.Tickets.FindAsync(id);
                if (trackedTicket != null)
                {
                    trackedTicket.IsViewed = true;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(ticket);
        }

        // For filtered tickets
        [HttpGet("filtered")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets(
            [FromQuery] string? search,  // Search by title or description
            [FromQuery] TicketStatus? status,
            [FromQuery] TicketPriority? priority,
            [FromQuery] string? category,
            [FromQuery] string? assignedTo,
            [FromQuery] string? submitter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "createdAt", // Sorting field
            [FromQuery] string? sortOrder = "desc" // Sorting order: "asc" or "desc"
        )
        {
            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            // Search by Title or Description
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));
            }

            // Filter by Status
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            // Filter by Priority
            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            // Filter by Category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            // Filter by Assigned User
            if (!string.IsNullOrEmpty(assignedTo))
            {
                query = query.Where(t => t.AssignedToId == assignedTo);
            }
            // Filter by Submitter
            if (!string.IsNullOrEmpty(submitter))
            {
                query = query.Where(t => t.SubmitterId == submitter);
            }

            // Sorting
            switch (sortBy?.ToLower())
            {
                case "title":
                    query = sortOrder == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title);
                    break;
                case "priority":
                    query = sortOrder == "asc" ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority);
                    break;
                case "status":
                    query = sortOrder == "asc" ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status);
                    break;
                case "submitter":
                    query = sortOrder == "asc" ? query.OrderBy(t => t.SubmitterId) : query.OrderByDescending(t => t.SubmitterId);
                    break;
                case "assignedto":
                    query = sortOrder == "asc" ? query.OrderBy(t => t.AssignedTo) : query.OrderByDescending(t => t.AssignedToId);
                    break;
                default:
                    query = sortOrder == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt);
                    break;
            }

            // Pagination
            int totalRecords = await query.CountAsync();
            var tickets = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { totalRecords, tickets });
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetTickets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortColumn = "CreatedAt",
            [FromQuery] string sortDirection = "desc")
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var query = _context.Tickets
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            // Sorting logic
            query = sortDirection.ToLower() == "asc"
                ? query.OrderByDynamic(sortColumn, false)
                : query.OrderByDynamic(sortColumn, true);

            var totalRecords = await query.CountAsync();

            var tickets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                tickets,
                totalRecords,
                currentPage = page,
                pageSize
            });
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetAllTickets()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            await _ticketRepository.DeleteTicketAsync(id);
            return NoContent();
        }

        private async Task<bool> TicketExists(int id)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            return ticket != null;
        }

        [HttpPost("{ticketId}/comments")]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] TicketComment comment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("User not authenticated.");

            var ticket = await _context.Tickets
                .Include(t => t.Submitter)
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return NotFound();

            comment.TicketId = ticketId;
            comment.UserId = userId;
            comment.CreatedAt = DateTime.UtcNow;

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            // Collect all unique email addresses
            var recipients = new HashSet<string>();

            if (!string.IsNullOrEmpty(ticket.Submitter?.Email))
                recipients.Add(ticket.Submitter.Email);

            if (!string.IsNullOrEmpty(ticket.AssignedTo?.Email))
                recipients.Add(ticket.AssignedTo.Email);

            foreach (var previousComment in ticket.Comments)
            {
                if (!string.IsNullOrEmpty(previousComment.User?.Email))
                    recipients.Add(previousComment.User.Email);
            }

            // Send email notification
            foreach (var email in recipients)
            {
                await _emailService.SendEmailAsync(
                    new List<string> { email },
                    "New Comment on Ticket",
                    $"A new comment was added to ticket '{ticket.Title}'. Check the HelpDesk system for details."
                );
            }

            return Ok(comment);
        }

        [HttpGet("{ticketId}/comments")]
        public async Task<IActionResult> GetTicketComments(int ticketId, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var query = _context.TicketComments
            .Where(c => c.TicketId == ticketId)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt);

            var totalComments = await query.CountAsync();
            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!comments.Any())
            {
                return NotFound(new { message = "No comments found for this ticket." });
            }

            return Ok(new
            {
                comments,
                totalComments,
                currentPage = page,
                totalPages = (int)Math.Ceiling((double)totalComments / pageSize)
            });
        }

        [HttpPost("{ticketId}/assign/{userId}")]
        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> AssignTicket(int ticketId, string userId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Submitter)  // ✅ Load Submitter to access FcmToken
                .FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.AssignedToId = userId;
            ticket.Status = TicketStatus.Assigned;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(ticket.Submitter?.Email))
            {
                await _emailService.SendEmailAsync(new List<string> { ticket.Submitter.Email },
                    "Ticket Status Assigned",
                    $"Your ticket '{ticket.Title}' is now assigned to {ticket.AssignedTo?.FirstName} {ticket.AssignedTo?.LastName}. Check the HelpDesk system.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Email failed to send. email: {ticket.Submitter?.Email}");
            }

            // Send SMS notifications
            //await _smsService.NotifyTicketAssignmentAsync(
            //    ticket.Id.ToString(),
            //    ticket.Title,
            //    ticket.Submitter.PhoneNumber,
            //    (ticket.AssignedTo.FirstName + " " + ticket.AssignedTo.MiddleName + " " + ticket.AssignedTo.LastName),
            //    ticket.AssignedTo.PhoneNumber
            //);

            return Ok(new { message = "Ticket assigned successfully." });
        }

        [HttpPut("{ticketId}/status/{status}")]
        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] UpdateTicketStatusDto model)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Submitter)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound("Ticket not found.");

            // Validate the status
            if (!Enum.TryParse(model.Status, out TicketStatus newStatus))
                return BadRequest("Invalid status value.");

            var oldStatus = ticket.Status.ToString();
            // Update the ticket fields
            ticket.Status = newStatus;
            if (newStatus == TicketStatus.Closed)
                ticket.ClosedAt = DateTime.UtcNow;


            ticket.ResolutionNotes = model.ResolutionNote; // Save resolution note
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();


            if (!string.IsNullOrEmpty(ticket.Submitter?.Email))
            {
                await _emailService.SendEmailAsync(new List<string> { ticket.Submitter.Email },
                    "Ticket Status Updated",
                    $"Your ticket '{ticket.Title}' is now {newStatus}. Check the HelpDesk system.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Email update failed to send.");
            }

            // SMS service call for notification

            //await _smsService.NotifyTicketStatusChangeAsync(
            //        ticket.Id.ToString(),
            //        ticket.Title,
            //        ticket.Submitter.PhoneNumber,
            //        oldStatus,
            //        ticket.Status.ToString()
            //    );

            return Ok(new { message = "Ticket status updated successfully." });
        }

        [Authorize(Roles = "Admin,IT")]
        [HttpPost("{ticketId}/mark-viewed")]
        public async Task<IActionResult> MarkTicketAsViewed(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingView = await _context.TicketViews
                .FirstOrDefaultAsync(tv => tv.TicketId == ticketId && tv.UserId == userId);

            if (existingView == null)
            {
                var ticketView = new TicketView
                {
                    TicketId = ticketId,
                    UserId = userId,
                    ViewedAt = DateTime.UtcNow
                };

                _context.TicketViews.Add(ticketView);
                ticket.IsViewed = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin,IT")]
        [HttpGet("ticket-counts")]
        public async Task<IActionResult> GetTicketCounts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Keep everything in UTC for consistent comparison
            var utcNow = DateTime.UtcNow;
            var tenMinutesAgo = utcNow.AddMinutes(-10);

            // Count new tickets created within the last 10 minutes
            var newCount = await _context.Tickets
                .CountAsync(t => t.CreatedAt >= tenMinutesAgo && t.CreatedAt <= utcNow);

            // Count unread tickets (same logic as before)
            var unreadCount = await _context.Tickets
                .Where(t => t.AssignedTo == null &&
                            !t.TicketViews.Any(tv => tv.UserId == userId) &&
                            t.Status == TicketStatus.New)
                .CountAsync();

            return Ok(new { newCount, unreadCount });
        }
    }
}