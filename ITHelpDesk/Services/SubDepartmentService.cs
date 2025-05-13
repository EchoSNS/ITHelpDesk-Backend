using ITHelpDesk.Data;
using ITHelpDesk.Domain.Department;
using ITHelpDesk.DTOs;
using ITHelpDesk.DTOs.SubDepartment;
using ITHelpDesk.Repositories;
using Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ITHelpDesk.Services
{
    public class SubDepartmentService : ISubDepartmentService
    {
        private readonly HelpDeskDbContext _context;
        private readonly ILogger<SubDepartmentService> _logger;

        public SubDepartmentService(
            HelpDeskDbContext context,
            ILogger<SubDepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SubDepartmentDto>> GetAllSubDepartmentsAsync()
        {
            try
            {
                var subDepartments = await _context.SubDepartments
                    .Include(sd => sd.Department)
                    .Include(sd => sd.SubDepartmentManager)
                    .Select(sd => new SubDepartmentDto
                    {
                        SubDepartmentId = sd.SubDepartmentId,
                        SubDepartmentName = sd.SubDepartmentName,
                        SubDepartmentDescription = sd.SubDepartmentDescription,
                        DepartmentId = sd.DepartmentId,
                        DepartmentName = sd.Department.DepartmentName,
                        SubDepartmentManagerId = string.IsNullOrEmpty(sd.SubDepartmentManagerId) ? null : sd.SubDepartmentManagerId,
                        SubDepartmentManagerName = sd.SubDepartmentManager != null ?
                        (sd.SubDepartmentManager.FirstName + " " + 
                        (sd.SubDepartmentManager.MiddleName ?? "") + " "
                        + sd.SubDepartmentManager.LastName) : null,
                        SubDepartmentManagerEmail = sd.SubDepartmentManager.Email ?? null,
                        Remarks = sd.Remarks,
                        CreatedAt = sd.CreatedAt,
                        UpdatedAt = sd.UpdatedAt,
                    })
                    .ToListAsync();

                return subDepartments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sub-departments");
                throw;
            }
        }

        public async Task<IEnumerable<SubDepartmentDto>> GetFilteredSubDepartmentsAsync(SubDepartmentFilterDto filter)
        {
            try
            {
                var query = _context.SubDepartments
                    .Include(sd => sd.Department)
                    .Include(sd => sd.SubDepartmentManager)
                    .AsQueryable();

                // Apply filtering
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(sd =>
                        (sd.SubDepartmentManager.FirstName + " " +
                        (sd.SubDepartmentManager.MiddleName ?? "") + " "
                        + sd.SubDepartmentManager.LastName).ToLower().Contains(searchTerm) ||
                        sd.SubDepartmentDescription.ToLower().Contains(searchTerm) ||
                        sd.Department.DepartmentName.ToLower().Contains(searchTerm) ||
                        (sd.SubDepartmentManager != null && (sd.SubDepartmentManager.FirstName + " " +
                        (sd.SubDepartmentManager.MiddleName ?? "") + " "
                        + sd.SubDepartmentManager.LastName).ToLower().Contains(searchTerm)) ||
                        (sd.Remarks != null && sd.Remarks.ToLower().Contains(searchTerm))
                    );
                }

                if (filter.DepartmentId.HasValue)
                {
                    query = query.Where(sd => sd.DepartmentId == filter.DepartmentId.Value);
                }

                if (!filter.ManagerId.IsNullOrEmpty())
                {
                    query = query.Where(sd => (string.IsNullOrEmpty(sd.SubDepartmentManagerId) ? null : sd.SubDepartmentManagerId) == filter.ManagerId);
                }

                if (filter.UnassignedOnly == true)
                {
                    query = query.Where(sd => sd.SubDepartmentManagerId == null);
                }

                var subDepartments = await query
                    .Select(sd => new SubDepartmentDto
                    {
                        SubDepartmentId = sd.SubDepartmentId,
                        SubDepartmentName = sd.SubDepartmentName,
                        SubDepartmentDescription = sd.SubDepartmentDescription,
                        DepartmentId = sd.DepartmentId,
                        DepartmentName = sd.Department.DepartmentName,
                        SubDepartmentManagerId = string.IsNullOrEmpty(sd.SubDepartmentManagerId) ? null : sd.SubDepartmentManagerId,
                        SubDepartmentManagerName = sd.SubDepartmentManager != null ?
                        (sd.SubDepartmentManager.FirstName + " " +
                        (sd.SubDepartmentManager.MiddleName ?? "") + " "
                        + sd.SubDepartmentManager.LastName) : null,
                        SubDepartmentManagerEmail = sd.SubDepartmentManager != null ? sd.SubDepartmentManager.Email : null,
                        Remarks = sd.Remarks,
                        CreatedAt = sd.CreatedAt,
                        UpdatedAt = sd.UpdatedAt,
                    })
                    .ToListAsync();

                return subDepartments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered sub-departments");
                throw;
            }
        }

        public async Task<SubDepartmentDto> GetSubDepartmentByIdAsync(int id)
        {
            try
            {
                var subDepartment = await _context.SubDepartments
                    .Include(sd => sd.Department)
                    .Include(sd => sd.SubDepartmentManager)
                    .Where(sd => (string.IsNullOrEmpty(sd.SubDepartmentManagerId) ? (int?)null : int.Parse(sd.SubDepartmentManagerId)) == id)
                    .Select(sd => new SubDepartmentDto
                    {
                        SubDepartmentId = sd.SubDepartmentId,
                        SubDepartmentName = sd.SubDepartmentName,
                        SubDepartmentDescription = sd.SubDepartmentDescription,
                        DepartmentId = sd.DepartmentId,
                        DepartmentName = sd.Department.DepartmentName,
                        SubDepartmentManagerId = string.IsNullOrEmpty(sd.SubDepartmentManagerId) ? null : sd.SubDepartmentManagerId,
                        SubDepartmentManagerName = sd.SubDepartmentManager != null ?
                        (sd.SubDepartmentManager.FirstName + " " +
                        (sd.SubDepartmentManager.MiddleName ?? "") + " "
                        + sd.SubDepartmentManager.LastName) : null,
                        SubDepartmentManagerEmail = sd.SubDepartmentManager != null ? sd.SubDepartmentManager.Email : null,
                        Remarks = sd.Remarks,
                        CreatedAt = sd.CreatedAt,
                        UpdatedAt = sd.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (subDepartment == null)
                {
                    throw new KeyNotFoundException($"Sub-department with ID {id} not found.");
                }

                return subDepartment;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error getting sub-department by ID: {Id}", id);
                throw;
            }
        }

        public async Task<SubDepartmentDto> CreateSubDepartmentAsync(CreateSubDepartmentDto dto)
        {
            try
            {
                var department = await _context.Departments.FindAsync(dto.DepartmentId);
                if (department == null)
                {
                    throw new KeyNotFoundException($"Department with ID {dto.DepartmentId} not found.");
                }

                var subDepartment = new SubDepartment
                {
                    SubDepartmentName = dto.SubDepartmentName,
                    SubDepartmentDescription = dto.SubDepartmentDescription,
                    DepartmentId = dto.DepartmentId,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SubDepartments.Add(subDepartment);
                await _context.SaveChangesAsync();

                return new SubDepartmentDto
                {
                    SubDepartmentId = subDepartment.SubDepartmentId,
                    SubDepartmentName = subDepartment.SubDepartmentName,
                    SubDepartmentDescription = subDepartment.SubDepartmentDescription,
                    DepartmentId = subDepartment.DepartmentId,
                    DepartmentName = department.DepartmentName,
                    Remarks = subDepartment.Remarks
                };
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error creating sub-department");
                throw;
            }
        }

        public async Task<bool> UpdateSubDepartmentAsync(int id, UpdateSubDepartmentDto dto)
        {
            try
            {
                var subDepartment = await _context.SubDepartments.FindAsync(id);
                if (subDepartment == null)
                {
                    throw new KeyNotFoundException($"Sub-department with ID {id} not found.");
                }

                var department = await _context.Departments.FindAsync(dto.DepartmentId);
                if (department == null)
                {
                    throw new KeyNotFoundException($"Department with ID {dto.DepartmentId} not found.");
                }

                subDepartment.SubDepartmentName = dto.SubDepartmentName;
                subDepartment.SubDepartmentDescription = dto.SubDepartmentDescription;
                subDepartment.DepartmentId = dto.DepartmentId;
                subDepartment.Remarks = dto.Remarks;
                subDepartment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error updating sub-department with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSubDepartmentAsync(int id)
        {
            try
            {
                var subDepartment = await _context.SubDepartments.FindAsync(id);
                if (subDepartment == null)
                {
                    throw new KeyNotFoundException($"Sub-department with ID {id} not found.");
                }

                // Check if there are any dependencies before deleting
                var hasEmployees = await _context.Users
                    .AnyAsync(e => e.SubDepartmentId == id);

                if (hasEmployees)
                {
                    throw new InvalidOperationException($"Cannot delete sub-department with ID {id} because it has associated employees.");
                }

                _context.SubDepartments.Remove(subDepartment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error deleting sub-department with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> AssignManagerAsync(int subDepartmentId, string employeeId)
        {
            try
            {
                var subDepartment = await _context.SubDepartments.FindAsync(subDepartmentId);
                if (subDepartment == null)
                {
                    throw new KeyNotFoundException($"Sub-department with ID {subDepartmentId} not found.");
                }

                var employee = await _context.Users.FindAsync(employeeId);
                if (employee == null)
                {
                    throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
                }

                // Check if the employee is already managing another sub-department
                //var isAlreadyManager = await _context.SubDepartments
                //    .AnyAsync(sd => (string.IsNullOrEmpty(sd.SubDepartmentManagerId) ? null : sd.SubDepartmentManagerId) == employeeId && sd.SubDepartmentId != subDepartmentId);

                //if (isAlreadyManager)
                //{
                //    throw new InvalidOperationException($"Employee with ID {employeeId} is already managing another sub-department.");
                //}

                subDepartment.SubDepartmentManagerId = employeeId.ToString();
                subDepartment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error assigning manager with ID: {EmployeeId} to sub-department with ID: {SubDepartmentId}",
                    employeeId, subDepartmentId);
                throw;
            }
        }

        public async Task<IEnumerable<ManagerDto>> GetAvailableManagersAsync()
        {
            try
            {
                // Get employees who are eligible to be managers (based on your criteria)
                // For example, employees with a certain role or position
                var managers = await _context.Users
                    .Include(e => e.Department)
                    .Where(e => e.IsActive && e.IsStaff) // Adjust based on your criteria
                    .Select(e => new ManagerDto
                    {
                        Id = e.Id,
                        FullName = $"{e.FirstName} {e.MiddleName ?? ""} {e.LastName}",
                        Email = e.Email ?? "",
                        Department = e.Department.DepartmentName
                    })
                    .ToListAsync();

                return managers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available managers");
                throw;
            }
        }
    }
}
