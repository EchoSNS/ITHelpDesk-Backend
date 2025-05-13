using ITHelpDesk.Domain.Department;

namespace ITHelpDesk.Repositories
{
    public interface ISubDepartmentRepository
    {
        Task<IEnumerable<SubDepartment>> GetAllAsync();
        Task<SubDepartment?> GetByIdAsync(int id);
        Task AddAsync(SubDepartment subDepartment);
        Task UpdateAsync(SubDepartment subDepartment);
        Task DeleteAsync(int id);
    }
}
