using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IProductPhotoRepository : IBaseRepository<ProductPhotos>
    {
        Task<IEnumerable<ProductPhotos>> GetPhotosByProductIdAsync(int productId);
    }
}
