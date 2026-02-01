using AutoMapper;
using SteamApp.Application.DTOs.Tag;
using SteamApp.Domain.Entities;

namespace SteamApp.WebAPI.Mapper
{
    public sealed class TagMappingProfile : Profile
    {
        public TagMappingProfile()
        {
            CreateMap<Tag, TagDto>()
                .ForMember(
                    dest => dest.GameName,
                    opt => opt.MapFrom(src => src.Game.Name)
                );

            CreateMap<TagCreateDto, Tag>()
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<TagUpdateDto, Tag>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.GameId, o => o.Ignore());
        }
    }
}