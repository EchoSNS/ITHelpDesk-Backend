using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.DTOs.Position
{
    public class UpdatePositionDto
    {
        public string PositionName { get; set; }
        public string? PositionDescription { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public int SubDepartmentId { get; set; }
    }
}
