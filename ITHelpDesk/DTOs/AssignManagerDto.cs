using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.DTOs
{
    public class AssignManagerDto
    {
        public int EntityId { get; set; } // DepartmentId or SubDepartmentId
        public string EmployeeId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty; // "department" or "subdepartment"
    }
}
