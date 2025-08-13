using ProjectGreenLens.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;
using ProjectGreenLens.Services.Interfaces;

namespace ProjectGreenLens.Services.Implementations
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity
    {
        private readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<T>> getAllAsync()
        {
            return await _repository.getAllAsync();
        }

        public async Task<T> getByIdAsync(int id)
        {
            var entity = await _repository.getByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with Id={id} was not found.");

            return entity;
        }

        public async Task<T> createAsync(T entity)
        {
            return await _repository.createAsync(entity);
        }

        public async Task<T> updateAsync(T entity)
        {
            var existing = await _repository.getByIdAsync(entity.id);
            if (existing == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with Id={entity.id} was not found.");

            await _repository.updateAsync(entity);
            return entity;
        }

        public async Task<bool> deleteAsync(int id)
        {
            var entity = await _repository.getByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with Id={id} was not found.");

            await _repository.deleteAsync(entity);
            return true;
        }
    }
}
