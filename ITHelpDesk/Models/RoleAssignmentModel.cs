using ITHelpDesk.Domain;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class RoleAssignmentModel
    {
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        public required string Role { get; set; }

    }
}