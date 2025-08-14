using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<FlowerCategory>
    {
        Task<IEnumerable<FlowerCategory>> GetCategoriesByStatusAsync(bool isActive);
        Task<int> GetProductCountByCategoryIdAsync(int categoryId);
    }
}
