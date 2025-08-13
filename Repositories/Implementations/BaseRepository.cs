using Microsoft.EntityFrameworkCore;
//using ProjectGreenLens.Infrastructure.dbContext;
using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;
using FlowerSelling.Data;

namespace ProjectGreenLens.Repositories.Implementations
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity, new()
    {
        protected readonly FlowerSellingDbContext _context;
        public BaseRepository(FlowerSellingDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> getAllAsync()
        {
            return await _context.Set<T>().Where(e => !e.IsDeleted).ToListAsync();
        }
        public async Task<T> createAsync(T entity)
        {
            entity.Id = Guid.NewGuid();
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task updateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<T?> getByIdAsync(Guid id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }
        public async Task deleteAsync(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            await updateAsync(entity);
        }
    }
}
