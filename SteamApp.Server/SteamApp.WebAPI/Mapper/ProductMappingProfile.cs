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
                .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.Name)); ;

            CreateMap<ProductCreateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
