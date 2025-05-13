namespace ITHelpDesk.DTOs.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
