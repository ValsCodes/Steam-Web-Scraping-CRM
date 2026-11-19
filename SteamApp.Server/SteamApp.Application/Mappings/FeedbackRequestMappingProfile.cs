using AutoMapper;
using SteamApp.Contracts.FeedbackRequest;
using SteamApp.Application.Utilities;
using SteamApp.Domain.Entities;

namespace SteamApp.Application.Mappings;

public sealed class FeedbackRequestMappingProfile : Profile
{
    public FeedbackRequestMappingProfile()
    {
        CreateMap<FeedbackRequest, FeedbackRequestDto>()
            .ForMember(
                destination => destination.ReferenceId,
                options => options.MapFrom(source => FeedbackRequestReference.Format(source.Id)));

        CreateMap<FeedbackRequestHistory, FeedbackRequestHistoryDto>();
    }
}
