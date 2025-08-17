using AutoMapper;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.DTOs.ProductCategoryDTO;
using FlowerSellingWebsite.Models.DTOs.ProductPhoto;
using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Order mappings
            CreateMap<Orders, OrderDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null ? src.Customer.FullName : null))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src =>
                    src.Supplier != null ? src.Supplier.SupplierName : null))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null ? src.CreatedByUser.FullName : null))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src =>
                    src.OrderDetails.Where(od => !od.IsDeleted)));

            CreateMap<CreateOrderDTO, Orders>();
            CreateMap<UpdateOrderDTO, Orders>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Order detail mappings
            CreateMap<OrderDetails, OrderDetailDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                    src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.FlowerBatchName, opt => opt.MapFrom(src =>
                    src.FlowerBatch != null ? src.FlowerBatch.BatchCode : null))
                .ForMember(dest => dest.SupplierListingName, opt => opt.MapFrom(src =>
                    src.SupplierListing != null ? ("Supplier ID: " + src.SupplierListing.SupplierId.ToString()) : null));

            CreateMap<CreateOrderDetailDTO, OrderDetails>();
            CreateMap<UpdateOrderDetailDTO, OrderDetails>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ------------------ Products ------------------
            CreateMap<Products, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name));

            CreateMap<Products, ProductListDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name));

            CreateMap<CreateProductDTO, Products>();
            CreateMap<UpdateProductDTO, Products>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // only map when null

            // ------------------ ProductPhotos ------------------
            CreateMap<ProductPhotos, ProductPhotoDTO>();
            CreateMap<CreateProductPhotoDTO, ProductPhotos>();
            CreateMap<UpdateProductPhotoDTO, ProductPhotos>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<DeleteProductPhotoDTO, ProductPhotos>();

            // ------------------ ProductCategories ------------------
            CreateMap<ProductCategories, ProductCategoryDTO>();
            CreateMap<CreateProductCategoryDTO, ProductCategories>();
            CreateMap<UpdateProductCategoryDTO, ProductCategories>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<DeleteProductCategoryDTO, ProductCategories>();
        }
    }
}
