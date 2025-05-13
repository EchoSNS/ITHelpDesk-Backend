using ITHelpDesk.Domain;
using Microsoft.AspNetCore.Identity;

namespace ITHelpDesk.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRoleService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            await _userManager.AddToRoleAsync(user, role);
            return true;
        }
    }
}
