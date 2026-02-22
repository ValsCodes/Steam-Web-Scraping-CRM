using AutoMapper;
using SteamApp.Application.DTOs.GameUrl;
using SteamApp.Domain.Entities;

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
