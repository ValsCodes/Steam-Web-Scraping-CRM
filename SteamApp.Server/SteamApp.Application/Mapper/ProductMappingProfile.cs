using AutoMapper;
using SteamApp.Application.DTOs.Product;
using SteamApp.Domain.Entities;

namespace SteamApp.Application.Mapper
{
    public sealed class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Tag, ProductTagDetailDto>()
                .ForMember(dest => dest.ItemGroupName,
                    opt => opt.MapFrom(src => src.ItemGroup == null ? null : src.ItemGroup.Name));

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.Name))
                .ForMember(dest => dest.GameInternalId, opt => opt.MapFrom(src => src.Game.InternalId))
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.ProductTags.OrderBy(x => x.TagId).Select(x => x.Tag.Name).ToArray()))
                .ForMember(dest => dest.TagDetails,
                    opt => opt.MapFrom(src => src.ProductTags.OrderBy(x => x.TagId).Select(x => x.Tag).ToArray()));

            CreateMap<ProductCreateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
