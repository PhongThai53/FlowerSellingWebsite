using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.DTOs.ProductCategory;
using FlowerSellingWebsite.Models.DTOs.ProductPhoto;
using FlowerSellingWebsite.Models.DTOs.Cart;
using FlowerSellingWebsite.Models.Entities;
using System.Diagnostics;

namespace FlowerSellingWebsite.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ------------------ Products ------------------
            CreateMap<Products, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name));

            CreateMap<Products, ProductListDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => GetProductImageUrlFromProduct(src)));

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
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => GetProductImageUrl(src)));


            CreateMap<AddToCartDTO, CartItem>();
            CreateMap<UpdateCartItemDTO, CartItem>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

        private static string GetProductImageUrl(CartItem cartItem)
        {
            if (cartItem.Product != null && cartItem.Product.ProductPhotos.Any(pp => !pp.IsDeleted))
            {
                return cartItem.Product.ProductPhotos.First(pp => !pp.IsDeleted).Url;
            }
            return "/images/product/default-product.jpg";
        }

        private static string GetProductImageUrlFromProduct(Products product)
        {
            if (product.ProductPhotos.Any(pp => !pp.IsDeleted))
            {
                return product.ProductPhotos.First(pp => !pp.IsDeleted).Url;
            }
            return "/images/product/default-product.jpg";
        }
    }
}
