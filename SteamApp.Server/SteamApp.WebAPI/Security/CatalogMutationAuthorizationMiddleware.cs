using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SteamApp.WebAPI.Security;

public sealed class CatalogMutationAuthorizationMiddleware(RequestDelegate next)
{
    private static readonly string[] CatalogPrefixes =
    [
        "/api/games",
        "/api/game-urls",
        "/api/products",
        "/api/pixels",
        "/api/tags",
        "/api/item-groups",
        "/api/game-url-products",
        "/api/game-url-pixels",
        "/api/product-tags",
        "/api/scraping-modes"
    ];

    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService)
    {
        if (IsCatalogMutation(context.Request))
        {
            var authorization = await authorizationService.AuthorizeAsync(
                context.User,
                resource: null,
                SecurityPolicies.AdminOnly);
            if (!authorization.Succeeded)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Catalog mutation forbidden",
                    Detail = "Only administrators can change global catalog resources."
                });
                return;
            }
        }

        await next(context);
    }

    private static bool IsCatalogMutation(HttpRequest request)
    {
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method))
        {
            return false;
        }

        return CatalogPrefixes.Any(prefix => request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
