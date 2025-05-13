using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Domain.Department
{
    public class SubDepartment
    {
        [Key]
        public int SubDepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubDepartmentName { get; set; }

        public string? SubDepartmentDescription { get; set; }
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? SubDepartmentManagerId { get; set; }  // FK to ApplicationUser.Id
        public ApplicationUser? SubDepartmentManager { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Key
        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public virtual ICollection<Position> Positions { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
