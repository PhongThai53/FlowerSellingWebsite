using FlowerSelling.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class CategoryRepository : BaseRepository<FlowerCategory>, ICategoryRepository
    {
        public CategoryRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FlowerCategory>> GetCategoriesByStatusAsync(bool isActive)
        {
            return await _context.Set<FlowerCategory>()
                .Where(c => !c.IsDeleted && c.IsActive == isActive)
                .ToListAsync();
        }

        public async Task<int> GetProductCountByCategoryIdAsync(int categoryId)
        {
            return await _context.Set<FlowerCategory>()
                .Where(c => c.Id == categoryId && !c.IsDeleted)
                .SelectMany(c => c.Flowers)
                .CountAsync(f => !f.IsDeleted);
        }
    }
}
