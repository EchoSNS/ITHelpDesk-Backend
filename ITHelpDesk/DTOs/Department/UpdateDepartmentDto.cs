namespace ITHelpDesk.DTOs.Department
{
    public class UpdateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Remarks { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
