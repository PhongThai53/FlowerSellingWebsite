using FlowerSellingWebsite.Models.DTOs.ProductCategory;
using ProjectGreenLens.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IProductCategoryService
        : IBaseService<ProductCategoryCreateDTO, ProductCategoryUpdateDTO, ProductCategoryResponseDTO>
    {
        public Task<IEnumerable<ProductCategoryResponseDTO>> GetProductCategoryWithProduct();
    }
}
