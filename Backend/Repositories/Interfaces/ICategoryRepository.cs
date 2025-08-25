using FlowerSellingWebsite.Models.Entities;
namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<FlowerCategories>
    {
        Task<IEnumerable<FlowerCategories>> GetCategoriesByStatusAsync(bool isActive);
        Task<int> GetProductCountByCategoryIdAsync(int categoryId);
    }
}
