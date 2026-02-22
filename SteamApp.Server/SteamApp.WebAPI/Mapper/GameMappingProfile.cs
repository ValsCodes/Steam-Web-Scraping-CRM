using AutoMapper;
using SteamApp.Application.DTOs.Game;
using SteamApp.Domain.Entities;

namespace SteamApp.WebAPI.Mapper;

public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {

        CreateMap<Game, GameDto>();

        CreateMap<GameCreateDto, Game>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<GameUpdateDto, Game>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

    }
}
