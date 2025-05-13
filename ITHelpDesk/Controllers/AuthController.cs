using ITHelpDesk.Domain;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ITHelpDesk.Services;
using Microsoft.AspNetCore.Authorization;

namespace ITHelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly EmailService _emailService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ITokenService tokenService,
            EmailService emailService)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        // User Registration
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = new List<string> { "Passwords do not match." }
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = new List<string> { "Email is already in use." }
                });
            }
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Id = Guid.NewGuid().ToString() // Explicitly set ID
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign "Staff" role by default
                await _userManager.AddToRoleAsync(user, "Staff");

                // Notify Admins and IT
                await _emailService.SendNewUserNotificationToAdmins(user);

                // Notify the user that their account is under review
                await _emailService.SendAccountUnderReviewNotification(user.Email);

                return Ok(new { Message = "User registered successfully" });
            }

            var errorMessages = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Registration failed", errors = errorMessages });
        }

        // User Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Response.ContentType = "application/json";
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "No registered account for this email." });

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials." });

            if (!user.IsStaff)
                return Unauthorized(new { message = "Your account is under review. Please wait for Admin/IT approval." });

            // Get the single role of the user
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Staff"; // Default to "Staff" if no role exists

            // Generate JWT with roles
            var token = _tokenService.GenerateJwtToken(user, role);
            return Ok(new { Token = token });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserSession"); // Clear session
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
                return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password updated successfully" });
        }

    }
}