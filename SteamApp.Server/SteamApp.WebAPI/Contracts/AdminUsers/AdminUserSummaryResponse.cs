namespace SteamApp.WebAPI.Contracts.AdminUsers;

public sealed record AdminUserSummaryResponse(
    string Id,
    string DisplayName,
    string? FirstName,
    string? LastName,
    string? UserName,
    string? Email,
    string? Phone,
    string[] Roles,
    string EffectiveRole,
    bool IsCurrentUser);
