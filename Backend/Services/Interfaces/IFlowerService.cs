using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Flower;
using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IFlowerService
    {
        Task<PagedResult<FlowerResponse>> GetListFlowerAsync(UrlQueryParams queryParams, int? supplierId = null);

        Task<bool> CreateFlowerWithSupplier(CreateSupplierListingRequest request, int supplierId);
        Task<bool> CreateFlowerAsync(CreateFlowerWithSupplierRequest request, int supplerId);
        
        Task<IEnumerable<FlowerCategoryResponse>> GetFlowerCategoriesAsync();
        Task<IEnumerable<FlowerTypeResponse>> GetFlowerTypesAsync();
        Task<IEnumerable<FlowerColorResponse>> GetFlowerColorsAsync();
        Task<IEnumerable<FlowerDTO>> GetAllFlowersAsync();
    }
}
