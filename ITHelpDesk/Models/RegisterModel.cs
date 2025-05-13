using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "First Name is required")]
        [MaxLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        public required string LastName { get; set; }

        [MaxLength(100, ErrorMessage = "Middle Name cannot exceed 100 characters.")]
        public required string MiddleName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "PhoneNumber is required")]
        public required string PhoneNumber { get; set; }
    }
}
