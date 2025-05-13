using ITHelpDesk.Data;
using ITHelpDesk.Domain.Department;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Repositories
{
    public class SubDepartmentRepository : ISubDepartmentRepository
    {
        private readonly HelpDeskDbContext _context;

        public SubDepartmentRepository(HelpDeskDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubDepartment>> GetAllAsync()
        {
            return await _context.SubDepartments.Include(sd => sd.Department).ToListAsync();
        }

        public async Task<SubDepartment?> GetByIdAsync(int id)
        {
            return await _context.SubDepartments.Include(sd => sd.Department)
                                                .FirstOrDefaultAsync(sd => sd.SubDepartmentId == id);
        }

        public async Task AddAsync(SubDepartment subDepartment)
        {
            await _context.SubDepartments.AddAsync(subDepartment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SubDepartment subDepartment)
        {
            _context.SubDepartments.Update(subDepartment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var subDepartment = await _context.SubDepartments.FindAsync(id);
            if (subDepartment != null)
            {
                _context.SubDepartments.Remove(subDepartment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
