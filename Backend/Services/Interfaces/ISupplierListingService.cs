using FlowerSellingWebsite.Models.DTOs;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface ISupplierListingService
    {
        Task<PagedResult<SupplierListingDTO>> ListSupplierListing(UrlQueryParams urlQueryParams);

        Task<bool> CreateSupplierListingAsync(SupplierListingDTO request);

        Task<SupplierListingDTO> GetSupplierListingDetail(int supplierId);

        Task<bool> UpdateSupplierListingAsync(SupplierListingDTO request);

        Task<bool> RemoveSupplierListingAsync(int supplierId, int flowerId);
    }
}
