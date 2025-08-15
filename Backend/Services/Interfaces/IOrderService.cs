using FlowerSellingWebsite.Models.DTOs.Order;
using ProjectGreenLens.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IOrderService : IBaseService<Models.Entities.Orders>
    {
        Task<PagedOrdersResultDTO> GetOrdersWithFiltersAsync(OrderFilterDTO filters);
        Task<OrderDTO> GetOrderByIdAsync(int id);
        Task<OrderDTO> GetOrderByPublicIdAsync(Guid publicId);
        Task<OrderDetailDTO> GetOrderDetailByIdAsync(int id);
        Task<OrderDetailDTO> GetOrderDetailByPublicIdAsync(Guid publicId);
        Task<IEnumerable<OrderDTO>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrderDTO>> GetOrdersBySupplierIdAsync(int supplierId);
        Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status);
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDTO, int userId);
        Task<bool> UpdateOrderAsync(int id, UpdateOrderDTO updateOrderDTO);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> DeleteOrderAsync(int id);
        Task<OrderDetailDTO> AddOrderDetailAsync(int orderId, CreateOrderDetailDTO createOrderDetailDTO);
        Task<bool> UpdateOrderDetailAsync(int orderDetailId, UpdateOrderDetailDTO updateOrderDetailDTO);
        Task<bool> DeleteOrderDetailAsync(int orderDetailId);
    }
}
