using ITHelpDesk.Domain;

namespace ITHelpDesk.Services
{
    public interface ITokenService
    {
        public string GenerateJwtToken(ApplicationUser user, string role, bool roleChanged = false);
    }
}