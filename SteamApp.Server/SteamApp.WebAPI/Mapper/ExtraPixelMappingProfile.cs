using AutoMapper;
using SteamApp.Models.DTOs.ExtraPixel;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class ExtraPixelMappingProfile : Profile
    {
        public ExtraPixelMappingProfile()
        {
            CreateMap<Pixel, PixelDto>();

            CreateMap<PixelCreateDto, Pixel>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<PixelUpdateDto, Pixel>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameUrlId, o => o.Ignore());
        }
    }
}
