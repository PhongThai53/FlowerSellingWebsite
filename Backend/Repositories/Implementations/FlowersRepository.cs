using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class FlowersRepository : BaseRepository<Flowers>, IFlowersRepository
    {
        public FlowersRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductFlowers>> GetFlowerRequirementsForProductAsync(int productId)
        {
            return await _context.ProductFlowers
                .Include(pf => pf.Flower)
                .Where(pf => pf.ProductId == productId)
                .ToListAsync();
        }
    }
}
