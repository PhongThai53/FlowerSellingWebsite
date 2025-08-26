using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class ProductPhotoRepository : BaseRepository<ProductPhotos>, IProductPhotoRepository
    {
        public ProductPhotoRepository(FlowerSellingDbContext context) : base(context)
        {
        }
    }
}
