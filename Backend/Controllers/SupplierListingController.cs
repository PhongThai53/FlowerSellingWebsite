using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierListingController : ControllerBase
    {
        private readonly ISupplierListingService _supplierListingService;

        public SupplierListingController(ISupplierListingService supplierListingService)
        {
            _supplierListingService = supplierListingService;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList([FromBody] UrlQueryParams queryParams)
        {
            var result = await _supplierListingService.ListSupplierListing(queryParams);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("{supplierId}")]
        public async Task<IActionResult> GetDetail(int supplierId)
        {
            var result = await _supplierListingService.GetSupplierListingDetail(supplierId);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SupplierListingDTO request)
        {
            var success = await _supplierListingService.CreateSupplierListingAsync(request);
            return success ? Ok(new { message = "Supplier listing created successfully." })
                           : BadRequest(new { message = "Failed to create supplier listing." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] SupplierListingDTO request)
        {
            var success = await _supplierListingService.UpdateSupplierListingAsync(request);
            return success ? Ok(new { message = "Supplier listing updated successfully." })
                           : NotFound(new { message = "Supplier listing not found." });
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> Remove([FromQuery] int supplierId, [FromQuery] int flowerId)
        {
            var success = await _supplierListingService.RemoveSupplierListingAsync(supplierId, flowerId);
            return success ? Ok(new { message = "Supplier listing removed successfully." })
                           : NotFound(new { message = "Supplier listing not found." });
        }
    }
}
