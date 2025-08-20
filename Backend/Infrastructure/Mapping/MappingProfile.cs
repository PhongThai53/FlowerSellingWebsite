using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
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
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name));

            CreateMap<Products, ProductListDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.Name));

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
        }
    }
}
