using ITHelpDesk.Data;
using ITHelpDesk.Domain.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelpDesk.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "Admin,IT")]
    public class DashboardController : ControllerBase
    {
        private readonly HelpDeskDbContext _context;

        public DashboardController(HelpDeskDbContext context)
        {
            _context = context;
        }

        private IQueryable<Ticket> ApplyDateFilter(IQueryable<Ticket> query, string filter)
        {
            var now = DateTime.UtcNow;

            return filter.ToLower() switch
            {
                "this-year" => query.Where(t => t.UpdatedAt != null && t.UpdatedAt.Value.Year == now.Year),
                "this-month" => query.Where(t => t.UpdatedAt != null && t.UpdatedAt.Value.Year == now.Year && t.UpdatedAt.Value.Month == now.Month),
                "this-week" => query.Where(t => t.UpdatedAt >= now.AddDays(-(int)now.DayOfWeek)),
                _ => query
            };
        }

        [HttpGet("resolved-reports")]
        public async Task<IActionResult> GetTicketSummary([FromQuery] string filter = "all-time")
        {
            var query = ApplyDateFilter(_context.Tickets, filter);

            var statusCounts = await query
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Calculate resolved and total tickets
            int resolvedCount = statusCounts
                .Where(s => s.Status == TicketStatus.Resolved.ToString() || s.Status == TicketStatus.Closed.ToString())
                .Sum(s => s.Count);
            int totalCount = statusCounts.Sum(s => s.Count);

            // Format the response as an array with the structure expected by the frontend
            var result = new[]
            {
                new
                {
                    category = "Tickets",
                    resolved = resolvedCount,
                    total = totalCount
                }
            };

            return Ok(result);
        }

        // Get ticket counts by status
        [HttpGet("ticket-status")]
        public async Task<IActionResult> GetTicketStatus([FromQuery] string filter = "all-time")
        {

            var query = ApplyDateFilter(_context.Tickets, filter);
            var statusCounts = await query
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return Ok(statusCounts);
        }

        //  Get the number of tickets for each category
        [HttpGet("top-concerns")]
        public async Task<IActionResult> GetTopConcerns([FromQuery] string filter = "all-time")
        {
            var query = ApplyDateFilter(_context.Tickets, filter);

            var topConcerns = await query
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            return Ok(topConcerns);
        }

        // Get top resolvers based on number of resolved tickets
        [HttpGet("top-resolvers")]
        public async Task<IActionResult> GetTopResolvers([FromQuery] string filter = "all-time")
        {
            var query = _context.Tickets
                .Where(t => t.Status == TicketStatus.Resolved && t.AssignedTo != null);

            query = ApplyDateFilter(query, filter);

            var result = await query
                .GroupBy(t => t.AssignedTo)
                .Select(g => new
                {
                    FullName = $"{g.Key.FirstName} {g.Key.MiddleName} {g.Key.LastName}".Trim(),
                    ResolvedCount = g.Count()
                })
                .OrderByDescending(g => g.ResolvedCount)
                .ToListAsync();

            return Ok(result);
        }

        // Get top creators based on number of submitted tickets
        [HttpGet("top-creators")]
        public async Task<IActionResult> GetTopCreators([FromQuery] string filter = "all-time")
        {
            var query = _context.Tickets
                .Where(t => t.Submitter != null);

            query = ApplyDateFilter(query, filter);

            var result = await query
                .GroupBy(t => t.Submitter)
                .Select(g => new
                {
                    FullName = $"{g.Key.FirstName} {g.Key.MiddleName} {g.Key.LastName}".Trim(),
                    CreatedCount = g.Count()
                })
                .OrderByDescending(g => g.CreatedCount)
                .ToListAsync();

            return Ok(result);
        }

        // Get ticket stats (Total, Open, High-severity, Critical)
        [HttpGet("stats")]
        public async Task<IActionResult> GetTicketStats()
        {
            var stats = await _context.Tickets
                .GroupBy(t => true)
                .Select(g => new
                {
                    TotalTickets = g.Count(),
                    OpenTickets = g.Count(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved),
                    HighSeverityTickets = g.Count(t => t.Priority == TicketPriority.High),
                    CriticalSeverityTickets = g.Count(t => t.Priority == TicketPriority.Critical)
                })
                .FirstOrDefaultAsync();

            return Ok(stats);
        }

        // Get Ticket Types for the Pie Chart
        [HttpGet("ticket-types")]
        public async Task<IActionResult> GetTicketTypes()
        {
            var ticketTypes = await _context.Tickets
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Type = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            return Ok(ticketTypes);
        }
        // Get resolution rate statistics
        [HttpGet("resolution-rate")]
        public async Task<IActionResult> GetResolutionRate([FromQuery] string filter = "all-time")
        {
            var query = ApplyDateFilter(_context.Tickets, filter);

            var totalTickets = await query.CountAsync();
            var resolvedTickets = await query.Where(t =>
                t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)
                .CountAsync();

            var resolutionRateData = new
            {
                TotalTickets = totalTickets,
                ResolvedTickets = resolvedTickets,
                ResolutionRate = totalTickets > 0 ? (resolvedTickets * 100.0 / totalTickets) : 0
            };

            return Ok(resolutionRateData);
        }

        // Get tickets by department
        [HttpGet("department-stats")]
        public async Task<IActionResult> GetDepartmentStats([FromQuery] string filter = "all-time")
        {
            var query = ApplyDateFilter(_context.Tickets, filter);

            var departmentStats = await query
                .GroupBy(t => t.Submitter.Position.SubDepartment.Department) // Assuming DepartmentName is a string
                .Select(g => new
                {
                    Department = g.Key,
                    TicketCount = g.Count(),
                    OpenTickets = g.Count(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved),
                    HighPriorityTickets = g.Count(t => t.Priority == TicketPriority.High || t.Priority == TicketPriority.Critical)
                })
                .OrderByDescending(g => g.TicketCount)
                .ToListAsync();

            return Ok(departmentStats);
        }

        // Get average severity data
        [HttpGet("average-severity")]
        public async Task<IActionResult> GetAverageSeverity([FromQuery] string filter = "all-time")
        {
            var query = ApplyDateFilter(_context.Tickets, filter);

            // Assuming Priority is an enum with values like Low=1, Medium=2, High=3, Critical=4
            var result = await query
                .GroupBy(t => true)
                .Select(g => new
                {
                    AverageSeverity = g.Average(t => (int)t.Priority)
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }

        [HttpGet("monthly-comparison")]
        public async Task<IActionResult> GetMonthlyComparison()
        {
            var currentYear = DateTime.UtcNow.Year;
            var lastYear = currentYear - 1;

            // Get current year data
            var currentYearData = await _context.Tickets
                .Where(t => t.CreatedAt.Year == currentYear)
                .GroupBy(t => t.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(m => m.Month)
                .ToListAsync();

            // Get last year data
            var lastYearData = await _context.Tickets
                .Where(t => t.CreatedAt.Year == lastYear)
                .GroupBy(t => t.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(m => m.Month)
                .ToListAsync();

            // Fill in missing months with zeros
            var currentYearCounts = new int[12];
            var lastYearCounts = new int[12];

            foreach (var item in currentYearData)
            {
                // Adjust for 0-based array vs 1-based month
                currentYearCounts[item.Month - 1] = item.Count;
            }

            foreach (var item in lastYearData)
            {
                // Adjust for 0-based array vs 1-based month
                lastYearCounts[item.Month - 1] = item.Count;
            }

            var result = new
            {
                CurrentYear = currentYearCounts,
                LastYear = lastYearCounts
            };

            return Ok(result);
        }

        // Search tickets with filtering functionality
        [HttpGet("search")]
        public async Task<IActionResult> SearchTickets(
        [FromQuery] string searchTerm = "",
        [FromQuery] string status = "",
        [FromQuery] string priority = "",
        [FromQuery] string department = "",
        [FromQuery] string assignedTo = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = _context.Tickets.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    t.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TicketStatus>(status, out var statusEnum))
                {
                    query = query.Where(t => t.Status == statusEnum);
                }
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                if (Enum.TryParse<TicketPriority>(priority, out var priorityEnum))
                {
                    query = query.Where(t => t.Priority == priorityEnum);
                }
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(t => t.Submitter.Position.SubDepartment.Department.DepartmentName == department);
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                query = query.Where(t =>
                    t.AssignedTo.FirstName.Contains(assignedTo) ||
                    t.AssignedTo.LastName.Contains(assignedTo));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Status,
                    t.Priority,
                    SubmitterName = $"{t.Submitter.FirstName} {t.Submitter.LastName}",
                    Department = t.Submitter.Position.SubDepartment.Department,
                    AssignedTo = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : null,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            var result = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Tickets = tickets
            };

            return Ok(result);
        }

        // Get list of departments for dropdown filters
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Tickets
                .Select(t => t.Submitter.Position.SubDepartment.Department)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            return Ok(departments);
        }
    }
}
