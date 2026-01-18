using AutoMapper;
using SteamApp.Models.DTOs.WatchList;
using SteamApp.Models.Entities;

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
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameId, o => o.Ignore())
                .ForMember(d => d.GameUrlId, o => o.Ignore());
        }
    }
}

