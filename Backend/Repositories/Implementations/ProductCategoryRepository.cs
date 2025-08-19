using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class ProductCategoryRepository : BaseRepository<ProductCategories>, IProductCategoryRepository
    {
        public ProductCategoryRepository(FlowerSellingDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductCategories>> GetProductCategoryWithProduct()
        {
            var categories = await _context.Set<ProductCategories>()
                                           .Include(c => c.Products)
                                           .ToListAsync();

            return categories;
        }
    }
}
