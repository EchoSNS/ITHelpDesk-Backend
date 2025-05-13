namespace ITHelpDesk.DTOs.Position
{
    public class PositionDto
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; }
        public string? PositionDescription { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public int SubDepartmentId { get; set; }
        public string? SubDepartmentName { get; set; }
    }

}
