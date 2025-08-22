using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        // Upload nhiều ảnh
        [HttpPost("products/{productId}")]
        public async Task<IActionResult> UploadProductImages(int productId, List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(ApiResponse<string>.Fail("No files uploaded."));

            var imagePaths = await _imageService.UploadProductImages(productId, files);
            return Ok(ApiResponse<List<string>>.Ok(imagePaths, "Images uploaded successfully."));
        }

        // Update nhiều ảnh
        [HttpPut("products/{productId}")]
        public async Task<IActionResult> UpdateProductImages(int productId, List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(ApiResponse<string>.Fail("No files uploaded."));

            var imagePaths = await _imageService.UpdateProductImages(productId, files);
            return Ok(ApiResponse<List<string>>.Ok(imagePaths, "Images updated successfully."));
        }

        // Xoá 1 ảnh theo tên
        [HttpDelete("products/{productId}/{fileName}")]
        public async Task<IActionResult> DeleteProductImage(int productId, string fileName)
        {
            var result = await _imageService.DeleteProductImage(productId, fileName);

            if (!result)
                return NotFound(ApiResponse<string>.Fail("Image not found."));

            return Ok(ApiResponse<string>.Ok(fileName, "Image deleted successfully."));
        }
    }
}
