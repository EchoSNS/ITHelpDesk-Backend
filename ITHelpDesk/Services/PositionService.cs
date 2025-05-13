using AutoMapper;
using ITHelpDesk.Domain.Department;
using ITHelpDesk.DTOs.Position;
using ITHelpDesk.Repositories;
using static ITHelpDesk.DTOs.Position.CreatePositionDto;

namespace ITHelpDesk.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _repository;
        private readonly IMapper _mapper;

        public PositionService(IPositionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<PositionDto>> GetAllAsync(string? search)
        {
            var positions = await _repository.GetAllAsync(search);
            return _mapper.Map<List<PositionDto>>(positions);
        }

        public async Task<PositionDto?> GetByIdAsync(int id)
        {
            var position = await _repository.GetByIdAsync(id);
            return position == null ? null : _mapper.Map<PositionDto>(position);
        }

        public async Task<PositionDto> AddAsync(CreatePositionDto dto)
        {
            var entity = _mapper.Map<Position>(dto);
            var added = await _repository.AddAsync(entity);
            return _mapper.Map<PositionDto>(added);
        }

        public async Task<PositionDto?> UpdateAsync(int id, UpdatePositionDto dto)
        {
            var entity = _mapper.Map<Position>(dto);
            entity.PositionId = id;
            var updated = await _repository.UpdateAsync(entity);
            return updated == null ? null : _mapper.Map<PositionDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id) =>
            await _repository.DeleteAsync(id);
    }
}
