using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.CategoryRegistration;
using LocalMartOnline.Models.DTOs.Product;
using LocalMartOnline.Models.DTOs.Store;
using LocalMartOnline.Models.DTOs.FastBargain;

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
        }
    }
}
