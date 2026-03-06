namespace Recipe.AdminApi.Features.Users.GetUserDetails;

public record GetUserDetailsResponse(
    string Id,
    string? UserName,
    string? Email,
    string? DisplayName,
    bool EmailConfirmed,
    bool IsLocked,
    bool IsAdmin,
    DateTimeOffset? LockoutEnd,
    int RecipeCount,
    int CookbookCount);
