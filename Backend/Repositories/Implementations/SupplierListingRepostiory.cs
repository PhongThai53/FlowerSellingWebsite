using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class SupplierListingRepostiory : BaseRepository<SupplierListings>, ISupplierListingRepository
    {
        private readonly FlowerSellingDbContext _context;
        public SupplierListingRepostiory(FlowerSellingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupplierListings>> GetByFlowerIdAsync(int flowerId)
        {
            return await _context.SupplierListings
                .Where(x => x.FlowerId == flowerId)
                .Include(x => x.Supplier)
                .ToListAsync();
        }


    }
}
