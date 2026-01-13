using AutoMapper;
using SteamApp.Models.DTOs.GameUrl;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public class GameUrlMappingProfile : Profile
    {
        public GameUrlMappingProfile()
        {
            CreateMap<GameUrl, GameUrlDto>();

            CreateMap<GameUrlCreateDto, GameUrl>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<GameUrlUpdateDto, GameUrl>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameId, o => o.Ignore());
        }
    }
}
