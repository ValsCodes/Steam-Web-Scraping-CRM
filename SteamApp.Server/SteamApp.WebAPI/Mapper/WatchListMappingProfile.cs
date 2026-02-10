using AutoMapper;
using SteamApp.Application.DTOs.WatchList;
using SteamApp.Application.DTOs.WatchListItem;
using SteamApp.Domain.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class WatchListMappingProfile : Profile
    {
        public WatchListMappingProfile()
        {
            CreateMap<WatchList, WatchListDto>();

            CreateMap<WatchListCreateDto, WatchList>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<WatchListUpdateDto, WatchList>()
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}

