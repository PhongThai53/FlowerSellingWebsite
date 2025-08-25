using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IFlowersRepository : IBaseRepository<Flowers>
    {
        Task<IEnumerable<ProductFlowers>> GetFlowerRequirementsForProductAsync(int productId);
    }
}
