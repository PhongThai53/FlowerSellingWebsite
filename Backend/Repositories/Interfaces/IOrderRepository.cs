using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<(IEnumerable<Order> orders, int totalCount)> GetOrdersWithFiltersAsync(OrderFilterDTO filters);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<Order?> GetOrderWithDetailsByPublicIdAsync(Guid publicId);
        Task<OrderDetail?> GetOrderDetailAsync(int id);
        Task<OrderDetail?> GetOrderDetailByPublicIdAsync(Guid publicId);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<Order>> GetOrdersBySupplierIdAsync(int supplierId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> AddOrderDetailAsync(OrderDetail orderDetail);
        Task<bool> UpdateOrderDetailAsync(OrderDetail orderDetail);
        Task<bool> RemoveOrderDetailAsync(int orderDetailId);
    }
}
