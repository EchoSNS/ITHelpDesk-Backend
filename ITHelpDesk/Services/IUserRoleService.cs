namespace ITHelpDesk.Services
{
    public interface IUserRoleService
    {
        Task<bool> AssignRoleAsync(string userId, string role);
    }
}
