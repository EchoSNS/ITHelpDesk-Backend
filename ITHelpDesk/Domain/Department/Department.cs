using ITHelpDesk.Domain.Ticket;
using ITHelpDesk.Domain;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Domain.Department
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        public string? DepartmentDescription { get; set; }

        public string? DepartmentManagerId { get; set; }
        public ApplicationUser? DepartmentManager { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Relationships
        public ICollection<SubDepartment> SubDepartments { get; set; } = new List<SubDepartment>();
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
