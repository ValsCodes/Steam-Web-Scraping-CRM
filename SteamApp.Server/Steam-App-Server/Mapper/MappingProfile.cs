using AutoMapper;
using SteamApp.Infrastructure.DTOs.Item;
using SteamApp.Infrastructure.DTOs.Product;
using SteamApp.Models;
using SteamApp.Models.Models;

namespace SteamApp.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Product Mapping

            CreateMap<Product, ProductDto>();

            CreateMap<ProductDto, Product>();

            CreateMap<CreateProductDto, Product>().ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<UpdateProductDto, Product>();

            CreateMap<ProductForPatchDto, Product>();
            CreateMap<ProductForPatchDto, Product>().ReverseMap();     

            #endregion

            #region Item Mapping

            CreateMap<ItemDto, Item>();

            CreateMap<Item, ItemDto>();

            CreateMap<CreateItemDto, Item>().ForMember(dest => dest.Id, opt => opt.Ignore());
            #endregion
        }
    }
}
