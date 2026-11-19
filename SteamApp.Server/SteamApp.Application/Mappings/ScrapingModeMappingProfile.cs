using AutoMapper;
using SteamApp.Contracts.ScrapingMode;
using SteamApp.Domain.Entities;

namespace SteamApp.Application.Mappings
{
    public sealed class ScrapingModeMappingProfile : Profile
    {
        public ScrapingModeMappingProfile()
        {
            CreateMap<ScrapingMode, ScrapingModeDto>();

            CreateMap<ScrapingModeCreateDto, ScrapingMode>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<ScrapingModeUpdateDto, ScrapingMode>()
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
