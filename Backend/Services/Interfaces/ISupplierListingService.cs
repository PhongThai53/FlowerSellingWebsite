using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.SupplierListing;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface ISupplierListingService
    {
        Task<bool> CreateAsync(CreateSupplierListingDTO request);
        Task<PagedResult<SupplierListingResponseDTO>> GetListAsync(SupplierListingListRequestDTO request);
    }
}
