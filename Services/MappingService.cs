using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.CategoryRegistration;
using LocalMartOnline.Models.DTOs.Faq;
using LocalMartOnline.Models.DTOs.Market;
using LocalMartOnline.Models.DTOs.Order;
using LocalMartOnline.Models.DTOs.PlatformPolicy;
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

            // Store
            CreateMap<Store, StoreDto>();
            CreateMap<StoreCreateDto, Store>();
            CreateMap<StoreUpdateDto, Store>();

            // Product
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            // Order
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));
            CreateMap<OrderCreateDto, Order>();
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<OrderItemDto, OrderItem>();

            // FAQ <-> FAQDto, FAQCreateDto, FAQUpdateDto
            CreateMap<Faq, FaqDto>().ReverseMap();
            CreateMap<FaqCreateDto, Faq>();
            CreateMap<FaqUpdateDto, Faq>();

            // PlatformPolicy <-> PlatformPolicyDto, PlatformPolicyUpdateDto
            CreateMap<PlatformPolicy, PlatformPolicyDto>().ReverseMap();
            CreateMap<PlatformPolicyUpdateDto, PlatformPolicy>();

            // Market <-> MarketDto, MarketCreateDto, MarketUpdateDto
            CreateMap<Market, MarketDto>().ReverseMap();
            CreateMap<MarketCreateDto, Market>();
            CreateMap<MarketUpdateDto, Market>();
        }
    }
}
