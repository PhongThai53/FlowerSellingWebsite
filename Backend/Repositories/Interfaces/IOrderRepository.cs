using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Orders>
    {
        Task<Orders?> GetOrderWithDetailsAsync(int orderId);
        Task<Orders?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<Orders>> GetOrdersByCustomerIdAsync(int customerId);
        Task<string> GenerateOrderNumberAsync();
        Task<bool> OrderNumberExistsAsync(string orderNumber);
        Task<Orders> CreateOrderAsync(Orders order);
        Task<OrderDetails> CreateOrderDetailAsync(OrderDetails orderDetail);
        Task<Payments> CreatePaymentAsync(Payments payment);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus);
        Task<PaymentMethods?> GetPaymentMethodByNameAsync(string methodName);
        Task<PaymentMethods> CreatePaymentMethodAsync(PaymentMethods paymentMethod);
        Task<bool> DeleteOrderAsync(int orderId);
    }
}
