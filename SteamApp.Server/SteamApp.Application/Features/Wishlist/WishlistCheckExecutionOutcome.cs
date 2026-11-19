using SteamApp.Contracts.WishListItem;
using SteamApp.Application.Common.Results;

namespace SteamApp.Application.Features.Wishlist;

public sealed record WishlistCheckExecutionOutcome(
    WishListCheckHistoryDto Trace,
    Error? CheckError);
