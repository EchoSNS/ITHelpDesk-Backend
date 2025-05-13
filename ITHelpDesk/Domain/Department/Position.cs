using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Domain.Department
{
    public class Position
    {
        [Key]
        public int PositionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; }

        public string? PositionDescription { get; set; }
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Key
        [ForeignKey("SubDepartment")]
        public int SubDepartmentId { get; set; }
        public virtual SubDepartment SubDepartment { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
