using AutoMapper;
using SteamApp.Models.DTOs.Product;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductDto>();

            CreateMap<ProductCreateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameUrlId, o => o.Ignore());
        }
    }
}
