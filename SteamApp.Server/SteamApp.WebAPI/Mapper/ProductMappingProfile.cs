using AutoMapper;
using SteamApp.Application.DTOs.Product;
using SteamApp.Domain.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.Name))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>  src.ProductTags.Select(x => x.Tag.Name).ToArray()));

            CreateMap<ProductCreateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
