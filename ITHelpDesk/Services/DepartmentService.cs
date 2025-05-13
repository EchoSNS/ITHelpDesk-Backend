using ITHelpDesk.Data;
using Google;

namespace ITHelpDesk.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HelpDeskDbContext _context;

        public DepartmentService(HelpDeskDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AssignManagerAsync(int departmentId, string employeeId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null) return false;

            department.DepartmentManagerId = employeeId;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
