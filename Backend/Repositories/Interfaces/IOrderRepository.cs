using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Orders>
    {
        Task<(IEnumerable<Orders> orders, int totalCount)> GetOrdersWithFiltersAsync(OrderFilterDTO filters);
        Task<Orders?> GetOrderWithDetailsAsync(int id);
        Task<Orders?> GetOrderWithDetailsByPublicIdAsync(Guid publicId);
        Task<OrderDetails?> GetOrderDetailsAsync(int id);
        Task<OrderDetails?> GetOrderDetailsByPublicIdAsync(Guid publicId);
        Task<IEnumerable<Orders>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<Orders>> GetOrdersBySupplierIdAsync(int supplierId);
        Task<IEnumerable<Orders>> GetOrdersByStatusAsync(string status);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> AddOrderDetailsAsync(OrderDetails orderDetail);
        Task<bool> UpdateOrderDetailsAsync(OrderDetails orderDetail);
        Task<bool> RemoveOrderDetailsAsync(int orderDetailId);
    }
}
