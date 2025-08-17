using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class ProductCategoryRepository : BaseRepository<ProductCategories>, IProductCategoryRepository
    {
        public ProductCategoryRepository(FlowerSellingDbContext context) : base(context)
        {
        }
    }
}
