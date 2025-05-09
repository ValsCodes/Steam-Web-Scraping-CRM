using AutoMapper;
using SteamApp.Infrastructure.DTOs;
using SteamApp.Infrastructure.DTOs.Product;
using SteamApp.Infrastructure.Models;
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

            #endregion

            #region Item Mapping

            CreateMap<ItemDto, Item>();

            CreateMap<Item, ItemDto>();

            CreateMap<ItemDto, IItem>();

            CreateMap<IItem, ItemDto>();
            #endregion
        }
    }
}
