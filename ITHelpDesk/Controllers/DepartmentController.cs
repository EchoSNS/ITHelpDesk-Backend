using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ITHelpDesk.Data;
using ITHelpDesk.Domain.Department;
using ITHelpDesk.DTOs;
using ITHelpDesk.DTOs.Department;
using ITHelpDesk.Services;

[Route("api/admin/departments")]
[ApiController]
[Authorize(Roles = "Admin,IT")]
public class DepartmentController : ControllerBase
{
    private readonly HelpDeskDbContext _context;
    private readonly ILogger<DepartmentController> _logger;
    private readonly IDepartmentService _departmentService;

    public DepartmentController(
        HelpDeskDbContext context,
        ILogger<DepartmentController> logger,
        IDepartmentService departmentService
        )
    {
        _context = context;
        _logger = logger;
        _departmentService = departmentService;
    }

    // GET: api/admin/departments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments([FromQuery] DepartmentFilterDto filter)
    {
        try
        {
            var query = _context.Departments
                .Include(d => d.DepartmentManager)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(d =>
                    d.DepartmentName.ToLower().Contains(searchTerm) ||
                    (d.DepartmentDescription != null && d.DepartmentDescription.ToLower().Contains(searchTerm)) ||
                    (d.Remarks != null && d.Remarks.ToLower().Contains(searchTerm)) ||
                    (d.DepartmentManager != null && (
                        d.DepartmentManager.FirstName.ToLower().Contains(searchTerm) ||
                        d.DepartmentManager.LastName.ToLower().Contains(searchTerm) ||
                        (d.DepartmentManager.MiddleName != null && d.DepartmentManager.MiddleName.ToLower().Contains(searchTerm))
                    ))
                );
            }

            if (!string.IsNullOrWhiteSpace(filter?.ManagerId))
            {
                if (filter.ManagerId == "unassigned")
                {
                    query = query.Where(d => d.DepartmentManagerId == null);
                }
                else
                {
                    query = query.Where(d => d.DepartmentManagerId == filter.ManagerId);
                }
            }

            var departments = await query.ToListAsync();

            var departmentDtos = departments.Select(d => new DepartmentDto
            {
                Id = d.DepartmentId,
                Name = d.DepartmentName,
                Description = d.DepartmentDescription,
                ManagerId = d.DepartmentManagerId,
                ManagerEmail = d.DepartmentManager != null
                    ? d.DepartmentManager.Email : null,
                ManagerName = d.DepartmentManager != null
                    ? $"{d.DepartmentManager.FirstName} {(string.IsNullOrWhiteSpace(d.DepartmentManager.MiddleName) ? "" : d.DepartmentManager.MiddleName + " ")}{d.DepartmentManager.LastName}"
                    : null,
                Remarks = d.Remarks,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();

            return Ok(departmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving departments");
            return StatusCode(500, new { message = "An error occurred while retrieving departments" });
        }
    }

    // GET: api/admin/departments/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        try
        {
            var department = await _context.Departments
                .Include(d => d.DepartmentManager)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }

            var departmentDto = new DepartmentDto
            {
                Id = department.DepartmentId,
                Name = department.DepartmentName,
                Description = department.DepartmentDescription,
                ManagerId = department.DepartmentManagerId,
                ManagerName = department.DepartmentManager != null
                    ? $"{department.DepartmentManager.FirstName} {(string.IsNullOrWhiteSpace(department.DepartmentManager.MiddleName) ? "" : department.DepartmentManager.MiddleName + " ")}{department.DepartmentManager.LastName}"
                    : null,
                Remarks = department.Remarks,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };

            return Ok(departmentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving department with ID {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the department" });
        }
    }

    // POST: api/admin/departments (Create a new department)
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentDto createDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                return BadRequest(new { message = "Department name is required" });
            }

            // Check if department with the same name already exists
            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == createDto.Name.ToLower());

            if (existingDepartment != null)
            {
                return Conflict(new { message = "A department with this name already exists" });
            }

            var department = new Department
            {
                DepartmentName = createDto.Name.Trim(),
                DepartmentDescription = createDto.Description?.Trim(),
                Remarks = createDto.Remarks?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, new DepartmentDto
            {
                Id = department.DepartmentId,
                Name = department.DepartmentName,
                Description = department.DepartmentDescription,
                Remarks = department.Remarks,
                CreatedAt = department.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating department {DepartmentName}", createDto.Name);
            return StatusCode(500, new { message = "An error occurred while creating the department" });
        }
    }

    // PUT: api/admin/departments/{id} (Update department)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, CreateDepartmentDto updateDto)
    {
        try
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }

            if (string.IsNullOrWhiteSpace(updateDto.Name))
            {
                return BadRequest(new { message = "Department name is required" });
            }

            // Check if department with the same name already exists (excluding current one)
            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == updateDto.Name.ToLower() && d.DepartmentId != id);

            if (existingDepartment != null)
            {
                return Conflict(new { message = "A department with this name already exists" });
            }

            department.DepartmentName = updateDto.Name.Trim();
            department.DepartmentDescription = updateDto.Description?.Trim();
            department.Remarks = updateDto.Remarks?.Trim();
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the department" });
        }
    }

    // DELETE: api/admin/departments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        try
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }

            // Check if department has associated users
            var hasUsers = await _context.Users.AnyAsync(u => u.DepartmentId == id);
            if (hasUsers)
            {
                return BadRequest(new { message = "Cannot delete department that has associated users. Please reassign users first." });
            }

            var hasSubDept = await _context.SubDepartments.AnyAsync(u => u.SubDepartmentId == id);
            if (hasSubDept)
            {
                return BadRequest(new { message = "Cannot delete department that has associated sub departments. Please reassign sub departments first." });
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the department" });
        }
    }

    // POST: api/admin/departments/bulk-delete
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDeleteDepartments(BulkDeleteDto bulkDeleteDto)
    {
        try
        {
            if (bulkDeleteDto.DepartmentIds == null || !bulkDeleteDto.DepartmentIds.Any())
            {
                return BadRequest(new { message = "No department IDs provided" });
            }

            // Make sure we have a fresh database context
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // First, explicitly set the compatibility level for this connection to avoid CTE issues
                await _context.Database.ExecuteSqlRawAsync("SET NOCOUNT ON;");

                // Process each department individually to avoid complex SQL generation
                foreach (var departmentId in bulkDeleteDto.DepartmentIds)
                {
                    // 1. Get the department to check if it exists
                    var department = await _context.Departments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);

                    if (department == null) continue;

                    // 2. Handle users - clear department references
                    var departmentUsers = await _context.Users
                        .Where(u => u.DepartmentId == departmentId)
                        .ToListAsync();

                    foreach (var user in departmentUsers)
                    {
                        user.DepartmentId = null;
                    }

                    if (departmentUsers.Any())
                    {
                        await _context.SaveChangesAsync();
                    }

                    // 3. Clear department manager reference
                    var deptWithManager = await _context.Departments
                        .Where(d => d.DepartmentId == departmentId && d.DepartmentManagerId != null)
                        .FirstOrDefaultAsync();

                    if (deptWithManager != null)
                    {
                        deptWithManager.DepartmentManagerId = null;
                        await _context.SaveChangesAsync();
                    }

                    // 4. Get subdepartments for this department
                    var subdepartments = await _context.SubDepartments
                        .Where(sd => sd.DepartmentId == departmentId)
                        .ToListAsync();

                    foreach (var subdept in subdepartments)
                    {
                        // 5. Handle each subdepartment separately
                        int subdeptId = subdept.SubDepartmentId;

                        // 6. Update users with this subdepartment
                        var subdeptUsers = await _context.Users
                            .Where(u => u.SubDepartmentId == subdeptId)
                            .ToListAsync();

                        foreach (var user in subdeptUsers)
                        {
                            user.SubDepartmentId = null;
                        }

                        if (subdeptUsers.Any())
                        {
                            await _context.SaveChangesAsync();
                        }

                        // 7. Clear subdepartment manager reference
                        if (subdept.SubDepartmentManagerId != null)
                        {
                            subdept.SubDepartmentManagerId = null;
                            await _context.SaveChangesAsync();
                        }

                        // 8. Get positions for this subdepartment
                        var positions = await _context.Positions
                            .Where(p => p.SubDepartmentId == subdeptId)
                            .ToListAsync();

                        foreach (var position in positions)
                        {
                            // 9. Update users with this position
                            var positionUsers = await _context.Users
                                .Where(u => u.PositionId == position.PositionId)
                                .ToListAsync();

                            foreach (var user in positionUsers)
                            {
                                user.PositionId = null;
                            }

                            if (positionUsers.Any())
                            {
                                await _context.SaveChangesAsync();
                            }

                            // 10. Delete the position
                            _context.Positions.Remove(position);
                            await _context.SaveChangesAsync();
                        }

                        // 11. Now delete the subdepartment
                        _context.SubDepartments.Remove(subdept);
                        await _context.SaveChangesAsync();
                    }

                    // 12. Finally delete the department 
                    var deptToDelete = await _context.Departments.FindAsync(departmentId);
                    if (deptToDelete != null)
                    {
                        _context.Departments.Remove(deptToDelete);
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = $"Successfully deleted departments",
                    deletedCount = bulkDeleteDto.DepartmentIds.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while bulk deleting departments. Payload: {@bulkDeleteDto}", bulkDeleteDto);
            return StatusCode(500, new { message = "An error occurred while bulk deleting departments", details = ex.Message });
        }
    }

    [HttpPost("assign-manager")]
    public async Task<IActionResult> AssignManager([FromBody] AssignManagerDto dto)
    {
        var result = await _departmentService.AssignManagerAsync(dto.EntityId, dto.EmployeeId);
        if (!result)
            return BadRequest(new { message = "Failed to assign manager." });
        return Ok(new { message = "Manager assigned successfully." });
    }

    [HttpPost("remove-manager")]
    public async Task<IActionResult> RemoveManager([FromBody] RemoveManagerDto dto)
    {
        try
        {
            var department = await _context.Departments.FindAsync(dto.EntityId);
            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {dto.EntityId} not found" });
            }

            // Check if department has a manager assigned
            if (department.DepartmentManagerId == null)
            {
                return BadRequest(new { message = "Department doesn't have a manager assigned." });
            }

            // Remove the manager assignment
            department.DepartmentManagerId = null;
            department.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Manager removed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing manager from department {DepartmentId}", dto.EntityId);
            return StatusCode(500, new { message = "An error occurred while removing the manager" });
        }
    }
}