using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.DTOs.ProductCategory;
using FlowerSellingWebsite.Models.DTOs.ProductPhoto;
using FlowerSellingWebsite.Models.DTOs.Cart;
using FlowerSellingWebsite.Models.Entities;
using System.Diagnostics;
using FlowerSellingWebsite.Models.DTOs.Order;

namespace FlowerSellingWebsite.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ------------------ Products ------------------
            CreateMap<Products, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name))
                .ForMember(dest => dest.ProductPhotos, opt => opt.MapFrom(src => src.ProductPhotos));

            CreateMap<Products, ProductListDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ProductPhotos.Any(pp => !pp.IsDeleted)
                        ? src.ProductPhotos.First(pp => !pp.IsDeleted).Url
                        : "/images/product/default-product.jpg"));

            CreateMap<CreateProductDTO, Products>();
            CreateMap<UpdateProductDTO, Products>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ------------------ ProductPhotos ------------------
            CreateMap<ProductPhotos, ProductPhotoDTO>();
            CreateMap<CreateProductPhotoDTO, ProductPhotos>();
            CreateMap<UpdateProductPhotoDTO, ProductPhotos>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<DeleteProductPhotoDTO, ProductPhotos>();

            // ------------------ ProductCategories ------------------
            CreateMap<ProductCategories, ProductCategoryResponseDTO>()
              .ForMember(dest => dest.TotalProducts, opt => opt.MapFrom(src => src.Products.Count));
            CreateMap<ProductCategoryCreateDTO, ProductCategories>();
            CreateMap<ProductCategoryUpdateDTO, ProductCategories>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ------------------ Users ------------------
            CreateMap<Users, UserDTO>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty))
                .ReverseMap();

            // ------------------ Cart ------------------
            CreateMap<Cart, CartDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));

            CreateMap<CartItem, CartItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.Url : string.Empty))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src =>
                    src.Product != null && src.Product.ProductPhotos.Any(pp => !pp.IsDeleted)
                        ? src.Product.ProductPhotos.First(pp => !pp.IsDeleted).Url
                        : "/images/product/default-product.jpg"));


            CreateMap<AddToCartDTO, CartItem>();
            CreateMap<UpdateCartItemDTO, CartItem>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Orders, OrderDTO>().ReverseMap();
        }
    }
}
