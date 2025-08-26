using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Cart;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.DTOs.ProductCategory;
using FlowerSellingWebsite.Models.DTOs.ProductPhoto;
using FlowerSellingWebsite.Models.Entities;

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

            // ------------------ Update Product DTO ------------------
            CreateMap<UpdateProductDTO, Products>()
                .ForMember(dest => dest.ProductPhotos, opt => opt.Ignore()) // Handle manually
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Products, UpdateProductDTO>()
                .ForMember(dest => dest.ProductPhotos, opt => opt.MapFrom(src => src.ProductPhotos));

            // Create Product - DTO to Entity (ignore ProductPhotos to handle manually)
            CreateMap<ProductPhotos, ProductPhotoDTO>();

            CreateMap<ProductPhotoDTO, ProductPhotos>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<CreateProductPhotoDTO, ProductPhotos>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<UpdateProductPhotoDTO, ProductPhotos>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<DeleteProductPhotoDTO, ProductPhotos>();

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

            // ------------------ Create Product DTO ------------------
            CreateMap<CreateProductDTO, Products>()
                .ForMember(dest => dest.ProductPhotos, opt => opt.MapFrom(src => src.ProductPhotos))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Products, CreateProductDTO>()
                .ForMember(dest => dest.ProductPhotos, opt => opt.MapFrom(src => src.ProductPhotos));

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

            // ------------------ Flowers ------------------
            CreateMap<Flowers, FlowerResponse>()
                .ForMember(dest => dest.FlowerCategory, opt => opt.MapFrom(src => src.FlowerCategory))
                .ForMember(dest => dest.FlowerType, opt => opt.MapFrom(src => src.FlowerType))
                .ForMember(dest => dest.FlowerColor, opt => opt.MapFrom(src => src.FlowerColor));

            CreateMap<FlowerCategories, FlowerCategoryResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<FlowerTypes, FlowerTypeResponse>()
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<FlowerColors, FlowerColorResponse>()
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.ColorName))
                .ForMember(dest => dest.HexCode, opt => opt.MapFrom(src => src.HexCode))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}
