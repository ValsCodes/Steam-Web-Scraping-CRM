using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;

namespace SteamApp.WebAPI.Services;

public sealed record WishlistCheckExecutionOutcome(
    WishListCheckHistoryDto Trace,
    Error? CheckError);
