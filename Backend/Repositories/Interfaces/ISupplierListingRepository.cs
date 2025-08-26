using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ISupplierListingRepository : IBaseRepository<SupplierListings>
    {
        Task<IEnumerable<SupplierListings>> GetByFlowerIdAsync(int flowerId);
    }
}
