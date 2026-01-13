using AutoMapper;
using SteamApp.Models.DTOs.ExtraPixel;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class ExtraPixelMappingProfile : Profile
    {
        public ExtraPixelMappingProfile()
        {
            CreateMap<ExtraPixel, ExtraPixelDto>();

            CreateMap<ExtraPixelCreateDto, ExtraPixel>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<ExtraPixelUpdateDto, ExtraPixel>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameUrlId, o => o.Ignore());
        }
    }
}
