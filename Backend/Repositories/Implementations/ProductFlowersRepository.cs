using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class ProductFlowersRepository : BaseRepository<ProductFlowers>, IProductFlowersRepository
    {
        private readonly FlowerSellingDbContext _context;
        public ProductFlowersRepository(FlowerSellingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductFlowers>> GetProductFlowers(int productId)
        {
            return await _context.ProductFlowers
                .Where(pf => pf.ProductId == productId)
                .ToListAsync();
        }
    }
}
