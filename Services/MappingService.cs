using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.CategoryRegistration;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Models.DTOs.Store;

namespace LocalMartOnline.Services
{
    public class MappingService : Profile
    {
        public MappingService()
        {
            // User <-> RegisterDTO
            CreateMap<User, RegisterDTO>().ReverseMap();
            // SellerRegistration <-> SellerRegistrationRequestDTO
            CreateMap<SellerRegistration, SellerRegistrationRequestDTO>().ReverseMap();
            // ProxyShopperRegistration <-> ProxyShopperRegistrationRequestDTO
            CreateMap<ProxyShopperRegistration, ProxyShopperRegistrationRequestDTO>().ReverseMap();

            // Category
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();

            // CategoryRegistration
            CreateMap<CategoryRegistration, CategoryRegistrationDto>()
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CategoryRegistrationDto, CategoryRegistration>()
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<CategoryRegistrationStatus>(src.Status)));

            // Store
            CreateMap<Store, StoreDto>();
            CreateMap<StoreCreateDto, Store>();
            CreateMap<StoreUpdateDto, Store>();

            // Product
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();
        }
    }
}
