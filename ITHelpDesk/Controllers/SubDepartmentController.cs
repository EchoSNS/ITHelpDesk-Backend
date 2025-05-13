using ITHelpDesk.Domain.Department;
using ITHelpDesk.DTOs.SubDepartment;
using ITHelpDesk.Services;
using Microsoft.AspNetCore.Mvc;
using ITHelpDesk.Data;
using ITHelpDesk.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ITHelpDesk.Controllers
{
    [Route("api/admin/subdepartments")]
    [ApiController]
    [Authorize(Roles = "Admin,IT")]
    public class SubDepartmentController : ControllerBase
    {
        private readonly ISubDepartmentService _subDepartmentService;
        private readonly ILogger<SubDepartmentController> _logger;

        public SubDepartmentController(ISubDepartmentService subDepartmentService, ILogger<SubDepartmentController> logger)
        {
            _subDepartmentService = subDepartmentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubDepartmentDto>>> GetAllSubDepartments()
        {
            try
            {
                var subDepartments = await _subDepartmentService.GetAllSubDepartmentsAsync();
                return Ok(subDepartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sub-departments");
                return StatusCode(500, "An error occurred while retrieving sub-departments.");
            }
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<SubDepartmentDto>>> GetFilteredSubDepartments([FromQuery] SubDepartmentFilterDto filter)
        {
            try
            {
                var subDepartments = await _subDepartmentService.GetFilteredSubDepartmentsAsync(filter);
                return Ok(subDepartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered sub-departments");
                return StatusCode(500, "An error occurred while retrieving filtered sub-departments.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubDepartmentDto>> GetSubDepartmentById(int id)
        {
            try
            {
                var subDepartment = await _subDepartmentService.GetSubDepartmentByIdAsync(id);
                return Ok(subDepartment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sub-department by ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the sub-department.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<SubDepartmentDto>> CreateSubDepartment([FromBody] CreateSubDepartmentDto dto)
        {
            try
            {
                var createdSubDepartment = await _subDepartmentService.CreateSubDepartmentAsync(dto);
                return CreatedAtAction(nameof(GetSubDepartmentById), new { id = createdSubDepartment.SubDepartmentId }, createdSubDepartment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sub-department");
                return StatusCode(500, "An error occurred while creating the sub-department.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubDepartment(int id, [FromBody] UpdateSubDepartmentDto dto)
        {
            try
            {
                await _subDepartmentService.UpdateSubDepartmentAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sub-department with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the sub-department.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubDepartment(int id)
        {
            try
            {
                await _subDepartmentService.DeleteSubDepartmentAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sub-department with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the sub-department.");
            }
        }

        [HttpPost("assign-manager")]
        public async Task<IActionResult> AssignManager([FromBody] AssignManagerDto dto)
        {
            var result = await _subDepartmentService.AssignManagerAsync(dto.EntityId, dto.EmployeeId);
            if (!result)
                return BadRequest(new { message = "Failed to assign manager." });
            return Ok(new { message = "Manager assigned successfully." });
        }

        [HttpGet("available-managers")]
        public async Task<ActionResult<IEnumerable<ManagerDto>>> GetAvailableManagers()
        {
            try
            {
                var managers = await _subDepartmentService.GetAvailableManagersAsync();
                return Ok(managers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available managers");
                return StatusCode(500, "An error occurred while retrieving available managers.");
            }
        }
    }
}
