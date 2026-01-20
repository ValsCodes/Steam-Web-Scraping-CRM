using AutoMapper;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class WishListMappingProfile : Profile
    {
        public WishListMappingProfile()
        {
            CreateMap<WishList, WishListDto>();

            CreateMap<WishListCreateDto, WishList>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<WishListUpdateDto, WishList>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameId, o => o.Ignore());
        }
    }
}
