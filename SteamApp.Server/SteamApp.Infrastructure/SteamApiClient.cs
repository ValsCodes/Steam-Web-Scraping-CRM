using SteamApp.Models.DTOs.ExtraPixel;
using SteamApp.Models.DTOs.Game;
using SteamApp.Models.DTOs.GameUrl;
using SteamApp.Models.DTOs.Product;
using SteamApp.Models.DTOs.WatchList;
using SteamApp.Models.DTOs.WishList;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SteamApp.Infrastructure
{
    public sealed class SteamApiClient(HttpClient http, AuthApiClient auth)
    {
        private string? _cachedToken;
        private DateTime _expiresAt;

        private async Task EnsureTokenAsync(CancellationToken ct)
        {
            if (_cachedToken != null && _expiresAt > DateTime.UtcNow.AddMinutes(1))
            {
                return;
            }

            var token = await auth.GetTokenAsync(ct);

            _cachedToken = token;

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            _expiresAt = jwt.ValidTo;

            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // =====================
        // Game
        // =====================

        public async Task<List<GameDto>> GetGamesAsync(CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            return await http.GetFromJsonAsync<List<GameDto>>("api/games", ct)
                   ?? [];
        }

        public async Task<GameDto?> GetGameAsync(long id, CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            return await http.GetFromJsonAsync<GameDto>($"api/games/{id}", ct);
        }

        public async Task<GameDto> CreateGameAsync(GameCreateDto dto, CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            var res = await http.PostAsJsonAsync("api/games", dto, ct);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<GameDto>(ct))!;
        }

        // =====================
        // GameUrl
        // =====================

        public async Task<List<GameUrlDto>> GetGameUrlsAsync(CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            return await http.GetFromJsonAsync<List<GameUrlDto>>("api/game-urls", ct)
                   ?? [];
        }

        public async Task<GameUrlDto> CreateGameUrlAsync(GameUrlCreateDto dto, CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            var res = await http.PostAsJsonAsync("api/game-urls", dto, ct);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<GameUrlDto>(ct))!;
        }

        // =====================
        // Product
        // =====================

        public async Task<List<ProductDto>> GetProductsAsync(CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            return await http.GetFromJsonAsync<List<ProductDto>>("api/products", ct)
                   ?? [];
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateDto dto, CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            var res = await http.PostAsJsonAsync("api/products", dto, ct);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<ProductDto>(ct))!;
        }

        // =====================
        // ExtraPixel
        // =====================

        public async Task<List<ExtraPixelDto>> GetExtraPixelsAsync(CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            return await http.GetFromJsonAsync<List<ExtraPixelDto>>("api/extra-pixels", ct)
                   ?? [];
        }

        public async Task<ExtraPixelDto> CreateExtraPixelAsync(
            ExtraPixelCreateDto dto,
            CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            var res = await http.PostAsJsonAsync("api/extra-pixels", dto, ct);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<ExtraPixelDto>(ct))!;
        }

        // =====================
        // WatchList
        // =====================

        public async Task<List<WatchListDto>> GetWatchListAsync(CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            return await http.GetFromJsonAsync<List<WatchListDto>>("api/watch-list", ct)
                   ?? [];
        }

        public async Task<WatchListDto> CreateWatchListItemAsync(
            WatchListCreateDto dto,
            CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            var res = await http.PostAsJsonAsync("api/watch-list", dto, ct);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<WatchListDto>(ct))!;
        }

        // =====================
        // WishList
        // =====================

        public async Task<List<WishListDto>> GetWishListAsync(CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            return await http.GetFromJsonAsync<List<WishListDto>>("api/wish-list", ct)
                   ?? [];
        }

        public async Task<WishListDto> CreateWishListItemAsync(
            WishListCreateDto dto,
            CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            var res = await http.PostAsJsonAsync("api/wish-list", dto, ct);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<WishListDto>(ct))!;
        }
    }
}
