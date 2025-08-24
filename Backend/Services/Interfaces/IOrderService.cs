using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Order;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PagedResult<OrderDTO>> GetOrderHistoryAsync(UrlQueryParams urlQueryParams, int? customerId = null);

        Task<OrderDTO?> GetOrderByIdAsync(int orderId);

        Task<CheckoutResponseDTO> ProcessCheckoutAsync(CheckoutRequestDTO checkoutRequest, int customerId, string clientIpAddress = null);
        
        Task<UserDTO?> GetUserByPublicIdAsync(Guid publicId);

        Task<CheckoutResponseDTO> ConfirmCODOrderAsync(int orderId, int customerId);
    }
}
