using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Faq;
using LocalMartOnline.Models.DTOs.Market;
using LocalMartOnline.Models.DTOs.PlatformPolicy;

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
