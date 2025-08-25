using FlowerSellingWebsite.Models.DTOs;
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
    }
}
