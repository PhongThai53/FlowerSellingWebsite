using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class ProductPhotoRepository : BaseRepository<ProductPhotos>, IProductPhotoRepository
    {
        public ProductPhotoRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductPhotos>> GetPhotosByProductIdAsync(int productId)
        {
            return await _context.ProductPhotos
                .Where(p => p.ProductId == productId && !p.IsDeleted)
                .ToListAsync();
        }
    }
}
