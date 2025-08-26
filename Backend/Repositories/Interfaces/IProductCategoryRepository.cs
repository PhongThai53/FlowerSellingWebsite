using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IProductCategoryRepository : IBaseRepository<ProductCategories>
    {
        Task<IEnumerable<ProductCategories>> GetProductCategoryWithProduct();
    }
}
