using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class UpdatePasswordModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your new password.")]
        [Compare("NewPassword", ErrorMessage = "The confirmation password does not match.")]
        public required string ConfirmNewPassword { get; set; }
    }
}
