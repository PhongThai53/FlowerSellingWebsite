using Microsoft.AspNetCore.Mvc;
using FlowerSellingWebsite.Models.DTOs.SupplierListing;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Infrastructure.Authorization;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierListingController : ControllerBase
    {
        private readonly ISupplierListingService _supplierListingService;

        public SupplierListingController(ISupplierListingService supplierListingService)
        {
            _supplierListingService = supplierListingService;
        }

        [HttpPost("create")]
        [Role("Supplier")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierListingDTO request)
        {
            try
            {
                var result = await _supplierListingService.CreateAsync(request);
                if (result)
                {
                    return Ok(new { message = "Thêm hoa vào kho thành công!" });
                }
                return BadRequest(new { message = "Không thể thêm hoa vào kho" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("list")]
        [Role("Supplier")]
        public async Task<IActionResult> GetList([FromQuery] SupplierListingListRequestDTO request)
        {
            try
            {
                var result = await _supplierListingService.GetListAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
