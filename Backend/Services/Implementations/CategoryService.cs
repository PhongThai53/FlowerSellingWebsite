using FlowerSellingWebsite.Models.DTOs.Category;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Services.Implementations;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class CategoryService : BaseService<FlowerCategories>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository) : base(categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.getAllAsync();
            var categoryDTOs = new List<CategoryDTO>();

            foreach (var category in categories)
            {
                var productCount = await _categoryRepository.GetProductCountByCategoryIdAsync(category.Id);
                categoryDTOs.Add(MapToDTO(category, productCount));
            }

            return categoryDTOs;
        }

        public async Task<CategoryDTO> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.getByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with Id={id} was not found.");

            var productCount = await _categoryRepository.GetProductCountByCategoryIdAsync(id);
            return MapToDTO(category, productCount);
        }

        public async Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO categoryDTO)
        {
            var category = new FlowerCategories
            {
                CategoryName = categoryDTO.CategoryName,
                Description = categoryDTO.Description,
                //Color = categoryDTO.Color,
                //IsActive = categoryDTO.IsActive
            };

            var createdCategory = await _categoryRepository.createAsync(category);
            return MapToDTO(createdCategory, 0);
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(int id, UpdateCategoryDTO categoryDTO)
        {
            var existingCategory = await _categoryRepository.getByIdAsync(id);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Category with Id={id} was not found.");

            existingCategory.CategoryName = categoryDTO.CategoryName;
            existingCategory.Description = categoryDTO.Description;
            //existingCategory.Color = categoryDTO.Color;
            //existingCategory.IsActive = categoryDTO.IsActive;

            await _categoryRepository.updateAsync(existingCategory);
            var productCount = await _categoryRepository.GetProductCountByCategoryIdAsync(id);
            return MapToDTO(existingCategory, productCount);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.getByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with Id={id} was not found.");

            await _categoryRepository.deleteAsync(category);
            return true;
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategoriesByStatusAsync(bool isActive)
        {
            var categories = await _categoryRepository.GetCategoriesByStatusAsync(isActive);
            var categoryDTOs = new List<CategoryDTO>();

            foreach (var category in categories)
            {
                var productCount = await _categoryRepository.GetProductCountByCategoryIdAsync(category.Id);
                categoryDTOs.Add(MapToDTO(category, productCount));
            }

            return categoryDTOs;
        }

        public async Task<bool> UpdateCategoryStatusAsync(int id, bool isActive)
        {
            var category = await _categoryRepository.getByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with Id={id} was not found.");

            //category.IsActive = isActive;
            await _categoryRepository.updateAsync(category);
            return true;
        }

        private CategoryDTO MapToDTO(FlowerCategories category, int productCount)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                //Color = category.Color,
                //IsActive = category.IsActive,
                ProductCount = productCount,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}
