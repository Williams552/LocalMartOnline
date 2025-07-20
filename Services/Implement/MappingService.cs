using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.CategoryRegistration;
using LocalMartOnline.Models.DTOs.Faq;
using LocalMartOnline.Models.DTOs.FastBargain;
using LocalMartOnline.Models.DTOs.Market;
using LocalMartOnline.Models.DTOs.Order;
using LocalMartOnline.Models.DTOs.PlatformPolicy;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Models.DTOs.ProductUnit;
using LocalMartOnline.Models.DTOs.ProxyShopping;
using LocalMartOnline.Models.DTOs.Seller;
using LocalMartOnline.Models.DTOs.Store;
using LocalMartOnline.Models.DTOs.User;

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
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryCreateDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<CategoryUpdateDto, Category>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

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

            // MarketFee
            CreateMap<MarketFee, MarketFeeDto>().ReverseMap();
            CreateMap<MarketFeeCreateDto, MarketFee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<MarketFeeUpdateDto, MarketFee>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // MarketFeePayment
            CreateMap<MarketFeePayment, MarketFeePaymentDto>()
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")));
            CreateMap<MarketFeePaymentCreateDto, MarketFeePayment>()
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // FastBargain <-> FastBargainResponseDTO
            CreateMap<FastBargain, FastBargainResponseDTO>()
                .ForMember(dest => dest.BargainId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Proposals, opt => opt.MapFrom(src => src.Proposals));
            CreateMap<FastBargainProposal, FastBargainProposalDTO>()
                .ForMember(dest => dest.BargainId, opt => opt.Ignore());

            // Category
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();

            // CategoryRegistration
            CreateMap<CategoryRegistration, CategoryRegistrationDto>()
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CategoryRegistrationCreateDto, CategoryRegistration>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.Status, opt => opt.Ignore())
               .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

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

            //oductUnit <-> ProductUnitDto, ProductUnitCreateDto, ProductUnitUpdateDto
            CreateMap<ProductUnitCreateDto, ProductUnit>();
            CreateMap<ProductUnitUpdateDto, ProductUnit>();
            CreateMap<ProductUnit, ProductUnitDto>();

            // User <-> UserDTO
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();

            // UserCreateDTO, UserUpdateDTO -> User
            CreateMap<UserCreateDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<UserUpdateDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<SellerRegistration, SellerRegistrationResponseDTO>();
            // ProxyShopperRegistration <-> ProxyShopperRegistrationResponseDTO
            CreateMap<ProxyShopperRegistration, ProxyShopperRegistrationResponseDTO>();
        }
    }
}
