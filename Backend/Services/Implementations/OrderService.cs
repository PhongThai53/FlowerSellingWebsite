using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;
using ProjectGreenLens.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IBaseRepository<Orders> _orderRepository;
        private readonly IMapper _mapper;

        public OrderService(IBaseRepository<Orders> orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<OrderDTO>> GetOrderHistoryAsync(UrlQueryParams urlQueryParams, int? customerId = null)
        {
            var query = customerId != null ? _orderRepository.AsQueryable().Where(x => x.CustomerId == customerId) : _orderRepository.AsQueryable();

            if (!string.IsNullOrWhiteSpace(urlQueryParams.SearchBy))
            {
                if (!string.IsNullOrEmpty(urlQueryParams.SearchValue))
                {
                    query = urlQueryParams.SearchBy switch
                    {
                        "order_code" => query.Where(x => x.OrderNumber.Contains(urlQueryParams.SearchValue)),
                        //Add more search field
                        _ => query
                    };
                }
            }

            if (urlQueryParams.FilterParams != null && urlQueryParams.FilterParams.Count > 0)
            {
                foreach (var item in urlQueryParams.FilterParams)
                {
                    query = item.Key switch
                    {
                        "status" => query.Where(x => x.Status == item.Value),
                        "payment_status" => query.Where(x => x.PaymentStatus == item.Value),
                        "start_date" => query.Where(x => x.CreatedAt >= DateTime.Parse(item.Value)),
                        "end_date" => query.Where(x => x.CreatedAt <= DateTime.Parse(item.Value)),
                        _ => query
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(urlQueryParams.SortBy))
            {
                query = urlQueryParams.SortBy.ToLower() switch
                {
                    "create_date" => urlQueryParams.SortDirection == SortDirection.Asc ? query.OrderBy(x => x.OrderDate) : query.OrderByDescending(x => x.OrderDate),
                    _ => query.OrderBy(x => x.CreatedAt) 
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            var totalItems = await query.CountAsync();

            var orders = await query
                .Skip((urlQueryParams.Page - 1) * urlQueryParams.PageSize)
                .Take(urlQueryParams.PageSize)
                .ToListAsync();

            return new PagedResult<OrderDTO>
            {
                Page = urlQueryParams.Page,
                PageSize = urlQueryParams.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)urlQueryParams.PageSize),
                Items = _mapper.Map<List<OrderDTO>>(orders)
            };
        }

       

        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == orderId);

            if(order == null) return null;

            return _mapper.Map<OrderDTO>(order);
        }
    }
}
