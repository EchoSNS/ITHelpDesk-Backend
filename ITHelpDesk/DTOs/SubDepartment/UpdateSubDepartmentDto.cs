using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.DTOs.SubDepartment
{
    public class UpdateSubDepartmentDto
    {
        public string SubDepartmentName { get; set; } = string.Empty;
        public string? SubDepartmentDescription { get; set; }
        public string? Remarks { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int DepartmentId { get; set; }
    }
}
