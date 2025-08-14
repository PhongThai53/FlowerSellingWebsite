using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Models.DTOs.Category;
using ProjectGreenLens.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface ICategoryService : IBaseService<FlowerCategory>
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO> GetCategoryByIdAsync(int id);
        Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO categoryDTO);
        Task<CategoryDTO> UpdateCategoryAsync(int id, UpdateCategoryDTO categoryDTO);
        Task<bool> DeleteCategoryAsync(int id);
        Task<IEnumerable<CategoryDTO>> GetCategoriesByStatusAsync(bool isActive);
        Task<bool> UpdateCategoryStatusAsync(int id, bool isActive);
    }
}
