namespace ITHelpDesk.DTOs.Department
{
    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
