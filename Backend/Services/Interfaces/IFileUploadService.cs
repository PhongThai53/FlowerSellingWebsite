namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "blog-images");
        Task<List<string>> UploadMultipleImagesAsync(IFormFileCollection files, string folder = "blog-images");
        Task<bool> DeleteImageAsync(string filePath);
        Task<bool> ValidateImageAsync(IFormFile file);
        string GetImageUrl(string fileName, string folder = "blog-images");
    }
}
