﻿using SteamApp.Models.DTOs;

namespace SteamApp.Infrastructure.Services;

public interface ISteamService
{
    IEnumerable<string> GetWeaponBatchUrls(short fromIndex, short batchSize);

    IEnumerable<string> GetHatBatchUrls(short fromPage, short batchSize);

    Task<string> GetPaintInfoFromSource(string src, CancellationToken cancellationToken);

    Task<IEnumerable<ListingDto>> GetDeserializedLisitngsFromUrl(short page, CancellationToken cancellationToken);

    Task<IEnumerable<ListingDto>> ScrapePage(short page, CancellationToken cancellationToken);

    Task<IEnumerable<ListingDto>> ScrapePageWithSrcPixelPaintCheck(short page, bool isGoodPaintsOnly, CancellationToken cancellationToken);

    Task<IEnumerable<PaintedListingsDto>> ScrapePageForPaintedListingsOnly(short page, CancellationToken cancellationToken);

    Task<PaintedListingDto> CheckIsListingPainted(string name, CancellationToken cancellationToken);
}