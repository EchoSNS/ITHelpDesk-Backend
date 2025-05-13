using ITHelpDesk.Domain.Department;
using ITHelpDesk.Domain.Ticket;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Domain
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string MiddleName { get; set; }

        public int? PositionId { get; set; }
        public Position? Position { get; set; }

        public bool IsStaff { get; set; } = false; // Default to Staff
        public bool IsActive { get; set; } = true;

        // Add the Firebase Cloud Messaging (FCM) token
        public string? FcmToken { get; set; }

        public int? DepartmentId { get; set; }
        public Department.Department? Department { get; set; }

        public int? SubDepartmentId { get; set; }
        public SubDepartment? SubDepartment { get; set; }

        public virtual ICollection<Ticket.Ticket> SubmittedTickets { get; set; }
        public virtual ICollection<Ticket.Ticket> AssignedTickets { get; set; }
        public virtual ICollection<TicketComment> TicketComments { get; set; }
        public virtual ICollection<TicketView> TicketViews { get; set; }
        public virtual ICollection<Department.Department> ManagedDepartments { get; set; }
        public virtual ICollection<SubDepartment> ManagedSubDepartments { get; set; }
    }
}
