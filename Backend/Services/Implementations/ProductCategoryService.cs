using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Repositories.Interfaces;
using ProjectGreenLens.Services.Implementations;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class ProductCategoryService : BaseService<ProductCategories>, IProductCategoryService
    {
        public ProductCategoryService(IBaseRepository<ProductCategories> repository)
            : base(repository)
        {
        }
    }
}
