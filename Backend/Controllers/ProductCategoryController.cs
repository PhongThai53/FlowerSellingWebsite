using FlowerSellingWebsite.Controllers.ProjectGreenLens.Controllers;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.ProductCategory;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController
    : BaseController<ProductCategoryCreateDTO, ProductCategoryUpdateDTO, ProductCategoryResponseDTO>
    {
        private readonly IProductCategoryService _productCategoryService;
        public ProductCategoryController(IProductCategoryService productCategoryService)
            : base(productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        // GET api/productcategory/with-products
        [HttpGet("with-products")]
        public async Task<IActionResult> GetWithTotalProducts()
        {
            var categories = await _productCategoryService.GetProductCategoryWithProduct();
            return Ok(ApiResponse<IEnumerable<ProductCategoryResponseDTO>>.Ok(categories));
        }
    }
}
