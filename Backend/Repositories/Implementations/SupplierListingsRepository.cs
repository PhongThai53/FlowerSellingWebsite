using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class SupplierListingsRepository : BaseRepository<SupplierListings>, ISupplierListingsRepository
    {
        public SupplierListingsRepository(FlowerSellingDbContext context) : base(context)
        {
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
                .Include(sl => sl.Flower)
                .Where(sl => sl.SupplierId == supplierId)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupplierListings>> GetByFlowerIdAsync(int flowerId)
        {
            return await _context.SupplierListings
                .Include(sl => sl.Supplier)
                .Where(sl => sl.FlowerId == flowerId)
                .ToListAsync();
        }

        public async Task<bool> UpdateAvailableQuantityAsync(int supplierId, int flowerId, int newQuantity)
        {
            var listing = await _context.SupplierListings
                .FirstOrDefaultAsync(sl => sl.SupplierId == supplierId && sl.FlowerId == flowerId);

            if (listing == null)
                return false;

            listing.AvailableQuantity = newQuantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeductAvailableQuantityAsync(int supplierListingId, int quantity)
        {
            var listing = await _context.SupplierListings.FirstOrDefaultAsync(sl => sl.Id == supplierListingId);
            if (listing == null) return false;
            if (quantity <= 0) return true;
            if (listing.AvailableQuantity < quantity) return false;

            listing.AvailableQuantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


