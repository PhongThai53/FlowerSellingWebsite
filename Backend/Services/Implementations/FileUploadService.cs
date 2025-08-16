using FlowerSellingWebsite.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "blog-images")
        {
            try
            {
                if (!await ValidateImageAsync(file))
                    throw new ArgumentException("Invalid image file.");

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<List<string>> UploadMultipleImagesAsync(IFormFileCollection files, string folder = "blog-images")
        {
            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var fileUrl = await UploadImageAsync(file, folder);
                    uploadedFiles.Add(fileUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image: {FileName}", file.FileName);
                    // Continue with other files, but log the error
                }
            }

            return uploadedFiles;
        }

        public async Task<bool> DeleteImageAsync(string filePath)
        {
            try
            {
                // Convert URL to physical path
                var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> ValidateImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size
            if (file.Length > _maxFileSize)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            var allowedMimeTypes = new[]
            {
                "image/jpeg",
                "image/jpg", 
                "image/png",
                "image/gif",
                "image/webp"
            };

            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        public string GetImageUrl(string fileName, string folder = "blog-images")
        {
            return $"/uploads/{folder}/{fileName}";
        }
    }
}
