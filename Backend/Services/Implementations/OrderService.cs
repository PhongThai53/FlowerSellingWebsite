using AutoMapper;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Services.Implementations;
using System.Linq;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper) : base(orderRepository)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<PagedOrdersResultDTO> GetOrdersWithFiltersAsync(OrderFilterDTO filters)
        {
            var (orders, totalCount) = await _orderRepository.GetOrdersWithFiltersAsync(filters);
            
            var orderDTOs = _mapper.Map<List<OrderDTO>>(orders);
            
            return new PagedOrdersResultDTO
            {
                Orders = orderDTOs,
                TotalCount = totalCount,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize),
                HasPreviousPage = filters.PageNumber > 1,
                HasNextPage = filters.PageNumber < (int)Math.Ceiling(totalCount / (double)filters.PageSize)
            };
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");
                
            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> GetOrderByPublicIdAsync(Guid publicId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByPublicIdAsync(publicId);
            if (order == null)
                throw new KeyNotFoundException($"Order with Public ID {publicId} not found.");
                
            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDetailDTO> GetOrderDetailByIdAsync(int id)
        {
            var orderDetail = await _orderRepository.GetOrderDetailAsync(id);
            if (orderDetail == null)
                throw new KeyNotFoundException($"Order detail with ID {id} not found.");
                
            return _mapper.Map<OrderDetailDTO>(orderDetail);
        }

        public async Task<OrderDetailDTO> GetOrderDetailByPublicIdAsync(Guid publicId)
        {
            var orderDetail = await _orderRepository.GetOrderDetailByPublicIdAsync(publicId);
            if (orderDetail == null)
                throw new KeyNotFoundException($"Order detail with Public ID {publicId} not found.");
                
            return _mapper.Map<OrderDetailDTO>(orderDetail);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersBySupplierIdAsync(int supplierId)
        {
            var orders = await _orderRepository.GetOrdersBySupplierIdAsync(supplierId);
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDTO, int userId)
        {
            // Create new order
            var order = _mapper.Map<Order>(createOrderDTO);
            order.CreatedByUserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.RequiredDate = createOrderDTO.RequiredDate;
            order.Status = "Pending";
            order.Notes = createOrderDTO.Notes;
            order.PublicId = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;
            order.EstimatedTotalAmount = 0; // Will be calculated from order details

            // Add order details
            foreach (var detailDTO in createOrderDTO.OrderDetails)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = detailDTO.ProductId,
                    FlowerBatchId = detailDTO.FlowerBatchId,
                    SupplierListingId = detailDTO.SupplierListingId,
                    ItemName = detailDTO.ItemName,
                    RequestedQuantity = detailDTO.RequestedQuantity,
                    UnitPrice = detailDTO.UnitPrice,
                    EstimatedAmount = detailDTO.RequestedQuantity * detailDTO.UnitPrice,
                    Notes = detailDTO.Notes,
                    PublicId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                };
                
                order.OrderDetails.Add(orderDetail);
                order.EstimatedTotalAmount += orderDetail.EstimatedAmount;
            }

            // Save the order
            // Use the createAsync method from the base service
            await base.createAsync(order);
            
            // Get the complete order with details
            var savedOrder = await _orderRepository.GetOrderWithDetailsAsync(order.Id);
            return _mapper.Map<OrderDTO>(savedOrder);
        }

        public async Task<bool> UpdateOrderAsync(int id, UpdateOrderDTO updateOrderDTO)
        {
            var existingOrder = await base.getByIdAsync(id);
            if (existingOrder == null)
                return false;

            // Update only the provided fields
            if (!string.IsNullOrEmpty(updateOrderDTO.OrderNumber))
                existingOrder.OrderNumber = updateOrderDTO.OrderNumber;
                
            if (updateOrderDTO.RequiredDate.HasValue)
                existingOrder.RequiredDate = updateOrderDTO.RequiredDate;
                
            if (!string.IsNullOrEmpty(updateOrderDTO.Notes))
                existingOrder.Notes = updateOrderDTO.Notes;
                
            if (!string.IsNullOrEmpty(updateOrderDTO.SupplierNotes))
                existingOrder.SupplierNotes = updateOrderDTO.SupplierNotes;
                
            if (!string.IsNullOrEmpty(updateOrderDTO.Status))
                existingOrder.Status = updateOrderDTO.Status;
                
            existingOrder.UpdatedAt = DateTime.UtcNow;

            try {
                // updateAsync returns the updated Order object, but we need to return bool
                var updatedOrder = await base.updateAsync(existingOrder);
                return updatedOrder != null;
            } catch (Exception) {
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            return await _orderRepository.UpdateOrderStatusAsync(id, status);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            return await base.deleteAsync(id);
        }

        public async Task<OrderDetailDTO> AddOrderDetailAsync(int orderId, CreateOrderDetailDTO createOrderDetailDTO)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            // Create new order detail
            var orderDetail = _mapper.Map<OrderDetail>(createOrderDetailDTO);
            orderDetail.OrderId = orderId;
            orderDetail.PublicId = Guid.NewGuid();
            orderDetail.CreatedAt = DateTime.UtcNow;
            orderDetail.EstimatedAmount = orderDetail.RequestedQuantity * orderDetail.UnitPrice;

            var success = await _orderRepository.AddOrderDetailAsync(orderDetail);
            if (!success)
                throw new Exception("Failed to add order detail.");

            var savedOrderDetail = await _orderRepository.GetOrderDetailAsync(orderDetail.Id);
            return _mapper.Map<OrderDetailDTO>(savedOrderDetail!);
        }

        public async Task<bool> UpdateOrderDetailAsync(int orderDetailId, UpdateOrderDetailDTO updateOrderDetailDTO)
        {
            var orderDetail = await _orderRepository.GetOrderDetailAsync(orderDetailId);
            if (orderDetail == null)
                return false;

            // Update only the provided fields
            if (updateOrderDetailDTO.RequestedQuantity.HasValue)
            {
                orderDetail.RequestedQuantity = updateOrderDetailDTO.RequestedQuantity.Value;
                // Recalculate estimated amount
                orderDetail.EstimatedAmount = orderDetail.RequestedQuantity * orderDetail.UnitPrice;
            }
                
            if (updateOrderDetailDTO.ApprovedQuantity.HasValue)
                orderDetail.ApprovedQuantity = updateOrderDetailDTO.ApprovedQuantity.Value;
                
            if (updateOrderDetailDTO.UnitPrice.HasValue)
            {
                orderDetail.UnitPrice = updateOrderDetailDTO.UnitPrice.Value;
                // Recalculate estimated amount
                orderDetail.EstimatedAmount = orderDetail.RequestedQuantity * orderDetail.UnitPrice;
            }
                
            if (updateOrderDetailDTO.FinalUnitPrice.HasValue)
            {
                orderDetail.FinalUnitPrice = updateOrderDetailDTO.FinalUnitPrice.Value;
                // Calculate final amount if approved quantity is set
                if (orderDetail.ApprovedQuantity.HasValue)
                {
                    orderDetail.FinalAmount = orderDetail.ApprovedQuantity.Value * updateOrderDetailDTO.FinalUnitPrice.Value;
                }
            }
                
            if (!string.IsNullOrEmpty(updateOrderDetailDTO.Notes))
                orderDetail.Notes = updateOrderDetailDTO.Notes;
                
            orderDetail.UpdatedAt = DateTime.UtcNow;

            return await _orderRepository.UpdateOrderDetailAsync(orderDetail);
        }

        public async Task<bool> DeleteOrderDetailAsync(int orderDetailId)
        {
            return await _orderRepository.RemoveOrderDetailAsync(orderDetailId);
        }

        // AutoMapper is now used for all mapping operations
    }
}
