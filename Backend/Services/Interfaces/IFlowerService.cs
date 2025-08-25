using FlowerSellingWebsite.Models.DTOs;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IFlowerService
    {
        Task<PagedResult<FlowerResponse>> GetListFlowerAsync(UrlQueryParams queryParams, int? supplierId = null);

        Task<bool> CreateFlowerWithSupplier(CreateSupplierListingRequest request, int supplierId);
        Task<bool> CreateFlowerAsync(CreateFlowerWithSupplierRequest request, int supplerId);
    }
}
