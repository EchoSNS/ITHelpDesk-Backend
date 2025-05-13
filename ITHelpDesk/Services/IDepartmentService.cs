namespace ITHelpDesk.Services
{
    public interface IDepartmentService
    {
        Task<bool> AssignManagerAsync(int departmentId, string employeeId);
    }
}
