using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ISupplierListingsRepository : IBaseRepository<SupplierListings>
    {
        Task<IEnumerable<SupplierListings>> GetAvailableFlowersByFlowerIdAsync(int flowerId);
        Task<IEnumerable<SupplierListings>> GetBySupplierIdAsync(int supplierId);
        Task<IEnumerable<SupplierListings>> GetByFlowerIdAsync(int flowerId);
        Task<bool> UpdateAvailableQuantityAsync(int supplierId, int flowerId, int newQuantity);
        Task<bool> DeductAvailableQuantityAsync(int supplierListingId, int quantity);
    }
}

