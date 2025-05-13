using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.DTOs.SubDepartment
{
    public class CreateSubDepartmentDto
    {
        public string SubDepartmentName { get; set; } = string.Empty;
        public string? SubDepartmentDescription { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int DepartmentId { get; set; }
    }
}
