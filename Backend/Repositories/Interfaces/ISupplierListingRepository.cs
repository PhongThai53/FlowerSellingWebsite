using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ISupplierListingRepository : IBaseRepository<SupplierListings>
    {
        Task<IEnumerable<SupplierListings>> GetByFlowerIdAsync(int flowerId);
    }
}
