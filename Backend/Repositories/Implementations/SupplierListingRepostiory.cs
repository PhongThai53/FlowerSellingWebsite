using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using FlowerSelling.Data.FlowerSellingWebsite.Data;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class SupplierListingRepository : BaseRepository<SupplierListings>, ISupplierListingsRepository
    {
        private readonly FlowerSellingDbContext _context;
        
        public SupplierListingRepository(FlowerSellingDbContext context) : base(context)
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

        public async Task<IEnumerable<SupplierListings>> GetAvailableFlowersByFlowerIdAsync(int flowerId)
        {
            return await _context.SupplierListings
                .Include(sl => sl.Supplier)
                .Include(sl => sl.Flower)
                .Where(sl => sl.FlowerId == flowerId && sl.AvailableQuantity > 0 && sl.Status == "available")
                .OrderBy(sl => sl.UnitPrice)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupplierListings>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.SupplierListings
                .Where(x => x.SupplierId == supplierId)
                .Include(x => x.Flower)
                .ToListAsync();
        }

        public async Task<bool> UpdateAvailableQuantityAsync(int supplierId, int flowerId, int newQuantity)
        {
            var listing = await _context.SupplierListings
                .FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.FlowerId == flowerId);
            
            if (listing != null)
            {
                listing.AvailableQuantity = newQuantity;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeductAvailableQuantityAsync(int supplierListingId, int quantity)
        {
            var listing = await _context.SupplierListings.FirstOrDefaultAsync(x => x.Id == supplierListingId);
            if (listing == null) return false;

            if (quantity <= 0) return true;
            if (listing.AvailableQuantity < quantity) return false;

            listing.AvailableQuantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
