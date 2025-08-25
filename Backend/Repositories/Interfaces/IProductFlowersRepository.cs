using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IProductFlowersRepository : IBaseRepository<ProductFlowers>
    {
        Task<IEnumerable<ProductFlowers>> GetProductFlowers(int productId);
        Task<IEnumerable<ProductFlowers>> GetFlowerRequirementsForProductAsync(int productId);
        Task<ProductFlowers?> GetProductFlowerAsync(int productId, int flowerId);
    }
}
