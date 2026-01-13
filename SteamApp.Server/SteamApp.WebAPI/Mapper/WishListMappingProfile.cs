using AutoMapper;
using SteamApp.Models.DTOs.WishList;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class WishListMappingProfile : Profile
    {
        public WishListMappingProfile()
        {
            CreateMap<WishListItem, WishListDto>();

            CreateMap<WishListCreateDto, WishListItem>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<WishListUpdateDto, WishListItem>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameId, o => o.Ignore());
        }
    }
}
