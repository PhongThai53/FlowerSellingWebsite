using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Flower;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/flower")]
    [ApiController]
    public class FlowersController : ControllerBase
    {
        private readonly IFlowerService _flowerService;

        public FlowersController(IFlowerService flowerService)
        {
            _flowerService = flowerService;
        }

        [HttpPost("list")]
        public async Task<IActionResult> ListFlowerAsync(UrlQueryParams queryParams,[FromQuery] int? supplerId = null)
        {
            var rs = await _flowerService.GetListFlowerAsync(queryParams, supplerId);
            
            return Ok(rs);
        }

        [HttpPost("{supplierId}/suplier-listing")]
        public async Task<IActionResult> CreateFlowerWithSupplier(CreateSupplierListingRequest request, int supplierId)
        {
            var rs = await _flowerService.CreateFlowerWithSupplier(request, supplierId);

            return Ok(rs);
        }

        [HttpPost("{supplierId}")]
        public async Task<IActionResult> CreateFlowerAsync(CreateFlowerWithSupplierRequest request, int supplierId)
        {
            var rs = await _flowerService.CreateFlowerAsync(request, supplierId);

            return Ok(rs);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetFlowerCategories()
        {
            try
            {
                var categories = await _flowerService.GetFlowerCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting flower categories", error = ex.Message });
            }
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetFlowerTypes()
        {
            try
            {
                var types = await _flowerService.GetFlowerTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting flower types", error = ex.Message });
            }
        }

        [HttpGet("colors")]
        public async Task<IActionResult> GetFlowerColors()
        {
            try
            {
                var colors = await _flowerService.GetFlowerColorsAsync();
                return Ok(colors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting flower colors", error = ex.Message });
            }
        }

        // Get all flowers for product recipe
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFlowers()
        {
            try
            {
                var flowers = await _flowerService.GetAllFlowersAsync();
                return Ok(ApiResponse<IEnumerable<FlowerDTO>>.Ok(flowers, "Flowers retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting flowers", error = ex.Message });
            }
        }
    }
}
