using AutoMapper;
using SteamApp.Contracts.GameUrl;
using SteamApp.Domain.Entities;

namespace SteamApp.Application.Mappings
{
    public class GameUrlMappingProfile : Profile
    {
        public GameUrlMappingProfile()
        {
            CreateMap<GameUrl, GameUrlDto>()
                .ForMember(
                    d => d.ItemGroupName,
                    o => o.MapFrom(s => s.ItemGroup != null ? s.ItemGroup.Name : null))
                .ForMember(
                    d => d.ScrapingModeName,
                    o => o.MapFrom(s => s.ScrapingMode != null ? s.ScrapingMode.Name : null));

            CreateMap<GameUrlCreateDto, GameUrl>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<GameUrlUpdateDto, GameUrl>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameId, o => o.Ignore());
        }
    }
}
