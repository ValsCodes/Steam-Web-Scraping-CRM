using System.Collections.Concurrent;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.Services;

namespace SteamApp.IntegrationTests.Support;

public sealed class FakeWishlistService : IWishlistService
{
    private readonly ConcurrentDictionary<long, WhishListResponse> responses = new();

    public int CheckCalls { get; private set; }

    public IReadOnlyCollection<WishListDto> Items { get; set; } =
    [
        new WishListDto
        {
            Id = 1,
            Name = "Active Wish",
            Price = 5,
            IsActive = true
        },
        new WishListDto
        {
            Id = 2,
            Name = "Inactive Wish",
            Price = 5,
            IsActive = false
        }
    ];

    public FakeWishlistService()
    {
        responses[1] = new WhishListResponse
        {
            GameName = "Active Game",
            CurrentPrice = 4.5,
            IsPriceReached = true
        };
    }

    public Task<WhishListResponse> CheckWishlistItem(long id)
    {
        CheckCalls++;

        return Task.FromResult(
            responses.TryGetValue(id, out var response)
                ? response
                : new WhishListResponse
                {
                    GameName = $"Game {id}",
                    CurrentPrice = 99,
                    IsPriceReached = false
                });
    }

    public Task<IEnumerable<WishListDto>> GetAllAsync(CancellationToken ct)
    {
        return Task.FromResult(Items.AsEnumerable());
    }

    public Task<WishListDto> GetAsync(long id, CancellationToken ct)
    {
        return Task.FromResult(Items.Single(x => x.Id == id));
    }

    public void SetResponse(long id, WhishListResponse response)
    {
        responses[id] = response;
    }

    public void Reset()
    {
        CheckCalls = 0;
        responses.Clear();
        responses[1] = new WhishListResponse
        {
            GameName = "Active Game",
            CurrentPrice = 4.5,
            IsPriceReached = true
        };
    }
}
