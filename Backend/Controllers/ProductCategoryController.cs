using FlowerSellingWebsite.Controllers.ProjectGreenLens.Controllers;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : BaseController<ProductCategories>
    {
        public ProductCategoryController(IProductCategoryService service)
            : base(service)
        {
        }
    }
}
