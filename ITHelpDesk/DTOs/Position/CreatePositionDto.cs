using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.DTOs.Position
{
    public class CreatePositionDto
    {
        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; }

        public string? PositionDescription { get; set; }
        public string? Remarks { get; set; }

        [Required]
        public int SubDepartmentId { get; set; }
    }
}