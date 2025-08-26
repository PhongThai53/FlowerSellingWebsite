using AutoMapper;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using ProjectGreenLens.Services.Interfaces;

namespace ProjectGreenLens.Services.Implementations
{
    public class BaseService<TEntity, TCreateDTO, TUpdateDTO, TResponseDTO>
    : IBaseService<TCreateDTO, TUpdateDTO, TResponseDTO>
    where TEntity : BaseEntity
    {
        private readonly IBaseRepository<TEntity> _repository;
        private readonly IMapper _mapper;

        public BaseService(IBaseRepository<TEntity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TResponseDTO>> getAllAsync()
        {
            var entities = await _repository.getAllAsync();
            return _mapper.Map<IEnumerable<TResponseDTO>>(entities);
        }

        public async Task<TResponseDTO> getByIdAsync(int id)
        {
            var entity = await _repository.getByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            return _mapper.Map<TResponseDTO>(entity);
        }

        public async Task<TResponseDTO> createAsync(TCreateDTO createDto)
        {
            var entity = _mapper.Map<TEntity>(createDto);
            var created = await _repository.createAsync(entity);
            return _mapper.Map<TResponseDTO>(created);
        }

        public async Task<TResponseDTO> updateAsync(int id, TUpdateDTO updateDto)
        {
            var existing = await _repository.getByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException();
            _mapper.Map(updateDto, existing);
            await _repository.updateAsync(existing);
            return _mapper.Map<TResponseDTO>(existing);
        }

        public async Task<bool> deleteAsync(int id)
        {
            var entity = await _repository.getByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            await _repository.deleteAsync(entity);
            return true;
        }
    }
}
