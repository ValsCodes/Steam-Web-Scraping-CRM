using SteamApp.Application.JsonObjects;

namespace SteamApp.Application.Abstractions.Services;

public interface IManualCheckListingSource
{
    TimeSpan DefaultCheckDelay { get; }

    string SelectUserAgent(long runId);

    Task<Listing> FetchListingAsync(Uri uri, bool bypassCache, string userAgent, CancellationToken cancellationToken);
}
