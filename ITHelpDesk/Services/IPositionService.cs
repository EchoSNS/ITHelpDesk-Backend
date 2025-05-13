using ITHelpDesk.DTOs.Position;

namespace ITHelpDesk.Services
{
    public interface IPositionService
    {
        Task<List<PositionDto>> GetAllAsync(string? search);
        Task<PositionDto?> GetByIdAsync(int id);
        Task<PositionDto> AddAsync(CreatePositionDto dto);
        Task<PositionDto?> UpdateAsync(int id, UpdatePositionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
