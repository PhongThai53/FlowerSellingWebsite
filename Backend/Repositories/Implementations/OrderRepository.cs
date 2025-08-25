using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;
using FlowerSellingWebsite.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class OrderRepository : BaseRepository<Orders>, IOrderRepository
    {
        public OrderRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<Orders?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Orders?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Orders?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<Orders>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            string orderNumber;
            do
            {
                // Generate order number in format: ORD-YYYYMMDD-XXXX
                var date = DateTime.Now.ToString("yyyyMMdd");
                var random = new Random();
                var randomPart = random.Next(1000, 9999).ToString();
                orderNumber = $"ORD-{date}-{randomPart}";
            } while (await OrderNumberExistsAsync(orderNumber));

            return orderNumber;
        }

        public async Task<bool> OrderNumberExistsAsync(string orderNumber)
        {
            return await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Orders> CreateOrderAsync(Orders order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<OrderDetails> CreateOrderDetailAsync(OrderDetails orderDetail)
        {
            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();
            return orderDetail;
        }

        public async Task<Payments> CreatePaymentAsync(Payments payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.PaymentStatus = paymentStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentEntityStatusAsync(int orderId, string paymentStatus)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null) return false;

            // Map payment status values
            var mappedStatus = paymentStatus switch
            {
                "Paid" => "Completed",
                "Pending" => "Pending",
                "Failed" => "Failed",
                "Refunded" => "Refunded",
                _ => paymentStatus
            };

            payment.Status = mappedStatus;
            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaymentMethods?> GetPaymentMethodByNameAsync(string methodName)
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.MethodName == methodName);
        }

        public async Task<PaymentMethods> CreatePaymentMethodAsync(PaymentMethods paymentMethod)
        {
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return false;

                // Remove related entities first
                if (order.OrderDetails != null)
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                }

                if (order.Payments != null)
                {
                    _context.Payments.RemoveRange(order.Payments);
                }

                // Remove the order
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error deleting order {orderId}: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<OrderDetails>?> GetOrderDetailsByOrderIdAsync(int orderId)
        {
            return await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }
    }
}
