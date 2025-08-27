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

            // Get existing product images to determine next number
            var existingFiles = Directory.GetFiles(folderPath)
                .Where(f => Path.GetFileName(f).StartsWith("product"))
                .Select(f => Path.GetFileName(f))
                .ToList();

            int nextNumber = 1;
            if (existingFiles.Any())
            {
                var numbers = existingFiles
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .Where(f => f.StartsWith("product"))
                    .Select(f => f.Replace("product", ""))
                    .Where(n => int.TryParse(n, out _))
                    .Select(int.Parse)
                    .ToList();
                
                if (numbers.Any())
                {
                    nextNumber = numbers.Max() + 1;
                }
            }

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"product{nextNumber}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return only filename, not full path
                savedPaths.Add(fileName);
                nextNumber++;
            }

            return savedPaths;
        }

        public async Task<List<string>> UpdateProductImages(int productId, List<IFormFile> files)
        {
            var folderPath = GetProductFolderPath(productId);

            if (Directory.Exists(folderPath))
            {
                // Delete only product*.jpg files, keep primary.jpg
                foreach (var oldFile in Directory.GetFiles(folderPath))
                {
                    var fileName = Path.GetFileName(oldFile);
                    if (fileName.StartsWith("product"))
                    {
                        try { File.Delete(oldFile); } catch { }
                    }
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
