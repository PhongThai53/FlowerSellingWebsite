using AutoMapper;
using FlowerSellingWebsite.Models.DTOs.ProductCategory;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Services.Implementations;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class ProductCategoryService
        : BaseService<ProductCategories, ProductCategoryCreateDTO, ProductCategoryUpdateDTO, ProductCategoryResponseDTO>,
          IProductCategoryService
    {
        private readonly IProductCategoryRepository _repo;
        private readonly IMapper _mapper;

        public ProductCategoryService(IProductCategoryRepository repo, IMapper mapper)
            : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductCategoryResponseDTO>> GetProductCategoryWithProduct()
        {
            var categories = await _repo.GetProductCategoryWithProduct();
            var dtoList = _mapper.Map<IEnumerable<ProductCategoryResponseDTO>>(categories);
            return dtoList;
        }
    }
}
