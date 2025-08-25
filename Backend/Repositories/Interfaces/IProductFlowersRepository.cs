using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IProductFlowersRepository : IBaseRepository<ProductFlowers>
    {
        public Task<IEnumerable<ProductFlowers>> GetProductFlowers(int productId);
    }
}
