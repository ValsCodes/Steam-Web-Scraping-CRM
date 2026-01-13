using AutoMapper;
using SteamApp.Models.DTOs.Game;
using SteamApp.Models.Entities;

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
