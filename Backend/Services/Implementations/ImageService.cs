using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;


namespace FlowerSellingWebsite.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public async Task<List<string>> UploadProductImages(int productId, List<IFormFile> files)
        {
            if (files == null || !files.Any()) return new List<string>();
            return await _imageRepository.UploadProductImages(productId, files);
        }

        public async Task<List<string>> UpdateProductImages(int productId, List<IFormFile> files)
        {
            if (files == null) return new List<string>();
            return await _imageRepository.UpdateProductImages(productId, files);
        }

        public async Task<bool> DeleteProductImage(int productId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            return await _imageRepository.DeleteProductImage(productId, fileName);
        }
    }
}
