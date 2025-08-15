//using FlowerSelling.Data;
//using FlowerSelling.Data.FlowerSellingWebsite.Data;
//using FlowerSellingWebsite.Models.DTOs.Order;
//using FlowerSellingWebsite.Models.Entities;
//using FlowerSellingWebsite.Repositories.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using ProjectGreenLens.Repositories.Implementations;
//using ProjectGreenLens.Repositories.Interfaces;

//namespace FlowerSellingWebsite.Repositories.Implementations
//{
//    public class OrderRepository : BaseRepository<Orders>, IOrderRepository
//    {

//        public OrderRepository(FlowerSellingDbContext context) : base(context)
//        {
//            // Base constructor handles context initialization
//        }

//        public async Task<(IEnumerable<Orders> orders, int totalCount)> GetOrdersWithFiltersAsync(OrderFilterDTO filters)
//        {
//            IQueryable<Orders> query = _context.Orders
//                .Include(o => o.Customer)
//                .Include(o => o.Supplier)
//                .Include(o => o.CreatedByUser)
//                .Include(o => o.OrderDetails)
//                .Where(o => !o.IsDeleted);

//            // Apply filters
//            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
//            {
//                string searchTerm = filters.SearchTerm.ToLower();
//                query = query.Where(o =>
//                    o.OrderNumber.ToLower().Contains(searchTerm) ||
//                    (o.Customer != null && o.Customer.FullName != null && o.Customer.FullName.ToLower().Contains(searchTerm)) ||
//                    (o.Supplier != null && o.Supplier.SupplierName != null && o.Supplier.SupplierName.ToLower().Contains(searchTerm)) ||
//                    (o.Notes != null && o.Notes.ToLower().Contains(searchTerm))
//                );
//            }

//            if (filters.IsSaleOrder.HasValue)
//            {
//                query = query.Where(o => o.IsSaleOrder == filters.IsSaleOrder.Value);
//            }

//            if (filters.CustomerId.HasValue)
//            {
//                query = query.Where(o => o.CustomerId == filters.CustomerId.Value);
//            }

//            if (filters.SupplierId.HasValue)
//            {
//                query = query.Where(o => o.SupplierId == filters.SupplierId.Value);
//            }

//            if (!string.IsNullOrWhiteSpace(filters.Status))
//            {
//                query = query.Where(o => o.Status.ToLower() == filters.Status.ToLower());
//            }

//            if (filters.StartDate.HasValue)
//            {
//                query = query.Where(o => o.OrderDate >= filters.StartDate.Value);
//            }

//            if (filters.EndDate.HasValue)
//            {
//                // Include the entire end date (up to 23:59:59)
//                DateTime endOfDay = filters.EndDate.Value.Date.AddDays(1).AddTicks(-1);
//                query = query.Where(o => o.OrderDate <= endOfDay);
//            }

//            if (filters.MinAmount.HasValue)
//            {
//                query = query.Where(o => o.EstimatedTotalAmount >= filters.MinAmount.Value);
//            }

//            if (filters.MaxAmount.HasValue)
//            {
//                query = query.Where(o => o.EstimatedTotalAmount <= filters.MaxAmount.Value);
//            }

//            // Get total count before pagination
//            int totalCount = await query.CountAsync();

//            // Apply sorting
//            query = ApplySorting(query, filters.SortBy, filters.SortDescending);

//            // Apply pagination
//            var orders = await query
//                .Skip((filters.PageNumber - 1) * filters.PageSize)
//                .Take(filters.PageSize)
//                .ToListAsync();

//            return (orders, totalCount);
//        }

//        private IQueryable<Order> ApplySorting(IQueryable<Order> query, string sortBy, bool sortDescending)
//        {
//            switch (sortBy.ToLower())
//            {
//                case "ordernumber":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.OrderNumber) 
//                        : query.OrderBy(o => o.OrderNumber);
//                case "orderdate":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.OrderDate) 
//                        : query.OrderBy(o => o.OrderDate);
//                case "requireddate":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.RequiredDate) 
//                        : query.OrderBy(o => o.RequiredDate);
//                case "status":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.Status) 
//                        : query.OrderBy(o => o.Status);
//                case "estimatedtotalamount":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.EstimatedTotalAmount) 
//                        : query.OrderBy(o => o.EstimatedTotalAmount);
//                case "finaltotalamount":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.FinalTotalAmount) 
//                        : query.OrderBy(o => o.FinalTotalAmount);
//                case "customername":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.Customer != null ? o.Customer.FullName : string.Empty)
//                        : query.OrderBy(o => o.Customer != null ? o.Customer.FullName : string.Empty);
//                case "suppliername":
//                    return sortDescending 
//                        ? query.OrderByDescending(o => o.Supplier != null ? o.Supplier.SupplierName : string.Empty) 
//                        : query.OrderBy(o => o.Supplier != null ? o.Supplier.SupplierName : string.Empty);
//                default:
//                    // Default sort by OrderDate descending
//                    return query.OrderByDescending(o => o.OrderDate);
//            }
//        }

//        public async Task<Orders?> GetOrderWithDetailsAsync(int id)
//        {
//            return await _context.Orders
//                .Include(o => o.Customer)
//                .Include(o => o.Supplier)
//                .Include(o => o.CreatedByUser)
//                .Include(o => o.OrderDetails)
//                    .ThenInclude(od => od.Product)
//                .Include(o => o.OrderDetails)
//                    .ThenInclude(od => od.FlowerBatch)
//                .Include(o => o.OrderDetails)
//                    .ThenInclude(od => od.SupplierListing)
//                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
//        }

//        public async Task<Orders?> GetOrderWithDetailsByPublicIdAsync(Guid publicId)
//        {
//            return await _context.Orders
//                .Include(o => o.Customer)
//                .Include(o => o.Supplier)
//                .Include(o => o.CreatedByUser)
//                .Include(o => o.OrderDetails)
//                    .ThenInclude(od => od.Product)
//                .Include(o => o.OrderDetails)
//                    .ThenInclude(od => od.FlowerBatch)
//                .Include(o => o.OrderDetails)
//                    .ThenInclude(od => od.SupplierListing)
//                .FirstOrDefaultAsync(o => o.PublicId == publicId && !o.IsDeleted);
//        }

//        public async Task<OrderDetails?> GetOrderDetailAsync(int id)
//        {
//            return await _context.OrderDetails
//                .Include(od => od.Product)
//                .Include(od => od.FlowerBatch)
//                .Include(od => od.SupplierListing)
//                .FirstOrDefaultAsync(od => od.Id == id && !od.IsDeleted);
//        }

//        public async Task<OrderDetails?> GetOrderDetailByPublicIdAsync(Guid publicId)
//        {
//            return await _context.OrderDetails
//                .Include(od => od.Product)
//                .Include(od => od.FlowerBatch)
//                .Include(od => od.SupplierListing)
//                .FirstOrDefaultAsync(od => od.PublicId == publicId && !od.IsDeleted);
//        }

//        public async Task<IEnumerable<Orders>> GetOrdersByCustomerIdAsync(int customerId)
//        {
//            return await _context.Orders
//                .Include(o => o.Customer)
//                .Include(o => o.CreatedByUser)
//                .Include(o => o.OrderDetails)
//                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
//                .OrderByDescending(o => o.OrderDate)
//                .ToListAsync();
//        }

//        public async Task<IEnumerable<Orders>> GetOrdersBySupplierIdAsync(int supplierId)
//        {
//            return await _context.Orders
//                .Include(o => o.Supplier)
//                .Include(o => o.CreatedByUser)
//                .Include(o => o.OrderDetails)
//                .Where(o => o.SupplierId == supplierId && !o.IsDeleted)
//                .OrderByDescending(o => o.OrderDate)
//                .ToListAsync();
//        }

//        public async Task<IEnumerable<Orders>> GetOrdersByStatusAsync(string status)
//        {
//            return await _context.Orders
//                .Include(o => o.Customer)
//                .Include(o => o.Supplier)
//                .Include(o => o.CreatedByUser)
//                .Include(o => o.OrderDetails)
//                .Where(o => o.Status.ToLower() == status.ToLower() && !o.IsDeleted)
//                .OrderByDescending(o => o.OrderDate)
//                .ToListAsync();
//        }

//        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
//        {
//            var order = await _context.Orders.FindAsync(id);
//            if (order == null || order.IsDeleted)
//                return false;

//            order.Status = status;
//            order.UpdatedAt = DateTime.UtcNow;
            
//            _context.Orders.Update(order);
//            return await _context.SaveChangesAsync() > 0;
//        }

//        public async Task<bool> AddOrderDetailAsync(OrderDetails orderDetail)
//        {
//            await _context.OrderDetails.AddAsync(orderDetail);
            
//            // Update the order's estimated total amount
//            var order = await _context.Orders.FindAsync(orderDetail.OrderId);
//            if (order != null)
//            {
//                order.EstimatedTotalAmount += orderDetail.EstimatedAmount;
//                order.UpdatedAt = DateTime.UtcNow;
//                _context.Orders.Update(order);
//            }
            
//            return await _context.SaveChangesAsync() > 0;
//        }

//        public async Task<bool> UpdateOrderDetailAsync(OrderDetails orderDetail)
//        {
//            // Get the original order detail to calculate the difference in amount
//            var originalOrderDetail = await _context.OrderDetails.AsNoTracking()
//                .FirstOrDefaultAsync(od => od.Id == orderDetail.Id);
            
//            if (originalOrderDetail == null || originalOrderDetail.IsDeleted)
//                return false;

//            _context.OrderDetails.Update(orderDetail);
            
//            // Update the order's estimated total amount
//            var order = await _context.Orders.FindAsync(orderDetail.OrderId);
//            if (order != null)
//            {
//                // Adjust the order's total amount by the difference
//                order.EstimatedTotalAmount = order.EstimatedTotalAmount - originalOrderDetail.EstimatedAmount + orderDetail.EstimatedAmount;
                
//                // If final amounts are set, update those too
//                if (orderDetail.FinalAmount.HasValue && originalOrderDetail.FinalAmount.HasValue && order.FinalTotalAmount.HasValue)
//                {
//                    order.FinalTotalAmount = order.FinalTotalAmount.Value - originalOrderDetail.FinalAmount.Value + orderDetail.FinalAmount.Value;
//                }
                
//                order.UpdatedAt = DateTime.UtcNow;
//                _context.Orders.Update(order);
//            }
            
//            return await _context.SaveChangesAsync() > 0;
//        }

//        public async Task<bool> RemoveOrderDetailAsync(int orderDetailId)
//        {
//            var orderDetail = await _context.OrderDetails.FindAsync(orderDetailId);
//            if (orderDetail == null || orderDetail.IsDeleted)
//                return false;

//            // Soft delete the order detail
//            orderDetail.IsDeleted = true;
//            orderDetail.DeletedAt = DateTime.UtcNow;
//            _context.OrderDetails.Update(orderDetail);
            
//            // Update the order's estimated total amount
//            var order = await _context.Orders.FindAsync(orderDetail.OrderId);
//            if (order != null)
//            {
//                order.EstimatedTotalAmount -= orderDetail.EstimatedAmount;
                
//                // If final amount is set, update that too
//                if (orderDetail.FinalAmount.HasValue && order.FinalTotalAmount.HasValue)
//                {
//                    order.FinalTotalAmount = order.FinalTotalAmount.Value - orderDetail.FinalAmount.Value;
//                }
                
//                order.UpdatedAt = DateTime.UtcNow;
//                _context.Orders.Update(order);
//            }
            
//            return await _context.SaveChangesAsync() > 0;
//        }
//    }
//}
