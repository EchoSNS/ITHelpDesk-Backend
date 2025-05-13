using ITHelpDesk.DTOs.SubDepartment;
using ITHelpDesk.DTOs;

namespace ITHelpDesk.Services
{
    public interface ISubDepartmentService
    {
        Task<IEnumerable<SubDepartmentDto>> GetAllSubDepartmentsAsync();
        Task<IEnumerable<SubDepartmentDto>> GetFilteredSubDepartmentsAsync(SubDepartmentFilterDto filter);
        Task<SubDepartmentDto> GetSubDepartmentByIdAsync(int id);
        Task<SubDepartmentDto> CreateSubDepartmentAsync(CreateSubDepartmentDto subDepartment);
        Task<bool> UpdateSubDepartmentAsync(int id, UpdateSubDepartmentDto subDepartment);
        Task<bool> DeleteSubDepartmentAsync(int id);
        Task<bool> AssignManagerAsync(int subDepartmentId, string employeeId);
        Task<IEnumerable<ManagerDto>> GetAvailableManagersAsync();
    }
}
