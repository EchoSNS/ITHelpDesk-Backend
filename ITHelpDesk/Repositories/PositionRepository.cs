using ITHelpDesk.Data;
using ITHelpDesk.Domain.Department;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Repositories
{
    public class PositionRepository : IPositionRepository
    {
        private readonly HelpDeskDbContext _context;

        public PositionRepository(HelpDeskDbContext context)
        {
            _context = context;
        }

        public async Task<List<Position>> GetAllAsync(string? search)
        {
            return await _context.Positions
                .Include(p => p.SubDepartment)
                .Where(p => string.IsNullOrEmpty(search) || p.PositionName.Contains(search))
                .ToListAsync();
        }

        public async Task<Position?> GetByIdAsync(int id) =>
            await _context.Positions.FindAsync(id);

        public async Task<Position> AddAsync(Position position)
        {
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<Position?> UpdateAsync(Position position)
        {
            var existing = await _context.Positions.FindAsync(position.PositionId);
            if (existing == null) return null;

            existing.PositionName = position.PositionName;
            existing.PositionDescription = position.PositionDescription;
            existing.Remarks = position.Remarks;
            existing.SubDepartmentId = position.SubDepartmentId;
            existing.IsActive = position.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null) return false;

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
