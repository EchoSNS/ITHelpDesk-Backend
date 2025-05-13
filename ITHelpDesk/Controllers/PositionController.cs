using ITHelpDesk.DTOs.Position;
using ITHelpDesk.Services;
using Microsoft.AspNetCore.Mvc;
using static ITHelpDesk.DTOs.Position.CreatePositionDto;

namespace ITHelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _service;

        public PositionsController(IPositionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _service.GetAllAsync(search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePositionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.PositionId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePositionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
