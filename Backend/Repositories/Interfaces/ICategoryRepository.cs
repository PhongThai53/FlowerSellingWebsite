using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<FlowerCategories>
    {
        Task<IEnumerable<FlowerCategories>> GetCategoriesByStatusAsync(bool isActive);
        Task<int> GetProductCountByCategoryIdAsync(int categoryId);
    }
}
