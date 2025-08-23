namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IImageService
    {
        Task<List<string>> UploadProductImages(int productId, List<IFormFile> files);
        Task<List<string>> UpdateProductImages(int productId, List<IFormFile> files);
        Task<bool> DeleteProductImage(int productId, string fileName);
    }
}
