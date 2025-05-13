using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ITHelpDesk.Services;
using ITHelpDesk.Models;
using ITHelpDesk.Domain;
using Microsoft.EntityFrameworkCore;
using ITHelpDesk.Data;
using ITHelpDesk.DTOs.User_Management;

namespace ITHelpDesk.Controllers
{

    [Route("api/user-roles")]
    [ApiController]
    [Authorize]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly EmailService _emailService;
        private readonly HelpDeskDbContext _context;

        public UserRoleController(IUserRoleService userRoleService,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            EmailService emailService,
            HelpDeskDbContext context
            )
        {
            _userRoleService = userRoleService;
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _context = context;
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRole = currentRoles.FirstOrDefault();

                if (currentRole == model.Role)
                    return BadRequest(new { message = "User already has this role." });

                // Remove old role and assign new one
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                var result = await _userManager.AddToRoleAsync(user, model.Role);
                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to assign role." });

                Console.WriteLine($"Success: Role changed to {model.Role} for user {user.UserName}");

                // Only notify the affected user to log out
                return Ok(new { message = $"Role changed to {model.Role}", forceLogoutFor = user.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        [HttpPut("confirm-user/{userId}")]
        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> ConfirmStaff(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            user.IsStaff = !user.IsStaff;
            await _userManager.UpdateAsync(user);

            return Ok(new { isStaff = user.IsStaff });
        }


        [Authorize]
        [HttpGet("list-users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Include(u => u.Position)
                        .ThenInclude(p => p.SubDepartment)
                            .ThenInclude(sd => sd.Department)
                    .ToListAsync();

                var userList = new List<object>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user); 

                    userList.Add(new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.MiddleName,
                        user.Email,
                        DepartmentName = user.Position?.SubDepartment?.Department?.DepartmentName ?? "N/A",
                        SubDepartmentName = user.Position?.SubDepartment?.SubDepartmentName ?? "N/A",
                        Position = user.Position?.PositionName ?? "N/A",
                        Role = roles.FirstOrDefault() ?? "Staff",
                        user.IsStaff,
                    });
                }

                return Ok(userList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin,IT")]
        [HttpGet("get-auth-users")]
        public async Task<IActionResult> GetAuthUsers()
        {
            var users = _userManager.Users.ToList();
            var itAdminUsers = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin") || roles.Contains("IT"))
                {
                    itAdminUsers.Add(new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.MiddleName,
                        user.Email,
                        Role = roles.FirstOrDefault()
                    });
                }
            }

            return Ok(itAdminUsers);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Include(d => d.SubDepartments)
                .Select(d => new {
                    d.DepartmentId,
                    d.DepartmentName,
                    SubDepartments = d.SubDepartments.Select(sd => new { sd.SubDepartmentId, sd.SubDepartmentName })
                })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpGet("subdepartments")]
        public async Task<IActionResult> GetSubDepartments()
        {
            var subDepartments = await _context.SubDepartments
                .Include(sd => sd.Department)
                .Include(sd => sd.Positions)
                .Select(sd => new {
                    sd.SubDepartmentId,
                    sd.SubDepartmentName,
                    DepartmentId = sd.Department.DepartmentId,
                    Positions = sd.Positions.Select(p => new { p.PositionId, p.PositionName })
                })
                .ToListAsync();

            return Ok(subDepartments);
        }

        [HttpGet("positions")]
        public async Task<IActionResult> GetPositions()
        {
            var positions = await _context.Positions
                .Include(p => p.SubDepartment)
                .Select(p => new {
                    p.PositionId,
                    p.PositionName,
                    SubDepartmentId = p.SubDepartment.SubDepartmentId
                })
                .ToListAsync();

            return Ok(positions);
        }

        [HttpPut("{userId}/department")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment(string userId, [FromBody] DepartmentUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Add your business logic here
            user.DepartmentId = int.Parse(dto.Id);
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [HttpPut("{userId}/subdepartment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubDepartment(string userId, [FromBody] SubDepartmentUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Add business logic
            user.SubDepartmentId = int.Parse(dto.Id);
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [HttpPut("{userId}/position")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePosition(string userId, [FromBody] PositionUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.PositionId = dto.Id;
            await _userManager.UpdateAsync(user);

            return Ok();
        }
    }
}