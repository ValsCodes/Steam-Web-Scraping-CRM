using AutoMapper;
using SteamApp.Models.DTOs.Item;
using SteamApp.Models.DTOs.Product;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<WatchItem, ProductDto>();

        CreateMap<ProductDto, WatchItem>();

        CreateMap<CreateProductDto, WatchItem>().ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<UpdateProductDto, WatchItem>();

        CreateMap<UpdateProductDto, WatchItem>().ReverseMap();     

        CreateMap<ItemDto, Item>();

        CreateMap<Item, ItemDto>();

        CreateMap<CreateItemDto, Item>().ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
