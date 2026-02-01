using AutoMapper;
using SteamApp.Application.DTOs.Pixel;
using SteamApp.Domain.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class PixelMappingProfile : Profile
    {
        public PixelMappingProfile()
        {
            CreateMap<Pixel, PixelDto>();

            CreateMap<PixelCreateDto, Pixel>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<PixelUpdateDto, Pixel>()
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
