using FlowerSellingWebsite.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class ImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _env;

        public ImageRepository(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string GetProductFolderPath(int productId) =>
            Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "images", "products", productId.ToString());

        public async Task<List<string>> UploadProductImages(int productId, List<IFormFile> files)
        {
            var savedPaths = new List<string>();
            var folderPath = GetProductFolderPath(productId);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (var file in files)
            {
                var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                savedPaths.Add($"/images/products/{productId}/{fileName}");
            }

            return savedPaths;
        }

        public async Task<List<string>> UpdateProductImages(int productId, List<IFormFile> files)
        {
            var folderPath = GetProductFolderPath(productId);

            if (Directory.Exists(folderPath))
            {
                foreach (var oldFile in Directory.GetFiles(folderPath))
                {
                    try { File.Delete(oldFile); } catch { }
                }
            }
            else
            {
                Directory.CreateDirectory(folderPath);
            }

            return await UploadProductImages(productId, files);
        }

        public async Task<bool> DeleteProductImage(int productId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            var folderPath = GetProductFolderPath(productId);
            var filePath = Path.Combine(folderPath, fileName);

            if (!File.Exists(filePath)) return false;

            try
            {
                File.Delete(filePath);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }
    }
}
