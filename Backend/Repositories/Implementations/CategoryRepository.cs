using FlowerSelling.Data;
using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class CategoryRepository : BaseRepository<FlowerCategories>, ICategoryRepository
    {
        public CategoryRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FlowerCategories>> GetCategoriesByStatusAsync(bool isActive)
        {
            return await _context.Set<FlowerCategories>()
                .ToListAsync();
        }

        public async Task<int> GetProductCountByCategoryIdAsync(int categoryId)
        {
            return await _context.Set<FlowerCategories>()
                .Where(c => c.Id == categoryId)
                .SelectMany(c => c.Flowers)
                .CountAsync(f => !f.IsDeleted);
        }
    }
}
