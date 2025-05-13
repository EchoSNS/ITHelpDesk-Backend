using ITHelpDesk.Domain.Department;

namespace ITHelpDesk.Repositories
{
    public interface IPositionRepository
    {
        Task<List<Position>> GetAllAsync(string? search);
        Task<Position?> GetByIdAsync(int id);
        Task<Position> AddAsync(Position position);
        Task<Position?> UpdateAsync(Position position);
        Task<bool> DeleteAsync(int id);
    }
}
