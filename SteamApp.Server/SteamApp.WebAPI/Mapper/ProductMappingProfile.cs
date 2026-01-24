using AutoMapper;
using SteamApp.Application.DTOs.Product;
using SteamApp.Domain.Entities;

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
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
