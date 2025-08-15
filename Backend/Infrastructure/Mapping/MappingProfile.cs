using AutoMapper;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Order mappings
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => 
                    src.Customer != null ? src.Customer.FullName : null))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => 
                    src.Supplier != null ? src.Supplier.SupplierName : null))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => 
                    src.CreatedByUser != null ? src.CreatedByUser.FullName : null))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => 
                    src.OrderDetails.Where(od => !od.IsDeleted)));
                    
            CreateMap<CreateOrderDTO, Order>();
            CreateMap<UpdateOrderDTO, Order>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Order detail mappings
            CreateMap<OrderDetail, OrderDetailDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.FlowerBatchName, opt => opt.MapFrom(src => 
                    src.FlowerBatch != null ? src.FlowerBatch.BatchCode : null))
                .ForMember(dest => dest.SupplierListingName, opt => opt.MapFrom(src => 
                    src.SupplierListing != null ? ("Supplier ID: " + src.SupplierListing.SupplierId.ToString()) : null));
                    
            CreateMap<CreateOrderDetailDTO, OrderDetail>();
            CreateMap<UpdateOrderDetailDTO, OrderDetail>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
