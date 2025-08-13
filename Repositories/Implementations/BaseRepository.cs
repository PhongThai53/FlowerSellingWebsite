using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Infrastructure.dbContext;
using ProjectGreenLens.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace ProjectGreenLens.Repositories.Implementations
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity, new()
    {
        protected readonly GreenLensDbContext _context;
        public BaseRepository(GreenLensDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> getAllAsync()
        {
            return await _context.Set<T>().Where(e => !e.isDelete).ToListAsync();
        }
        public async Task<T> createAsync(T entity)
        {
            entity.uniqueGuid = Guid.NewGuid();
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task updateAsync(T entity)
        {
            entity.updatedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<T?> getByIdAsync(int id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(e => e.id == id && !e.isDelete);
        }
        public async Task deleteAsync(T entity)
        {
            entity.isDelete = true;
            entity.deletedAt = DateTime.UtcNow;
            await updateAsync(entity);
        }
    }
}
