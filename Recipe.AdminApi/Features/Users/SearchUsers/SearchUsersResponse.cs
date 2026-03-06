namespace Recipe.AdminApi.Features.Users.SearchUsers;

public record SearchUsersResponse(
    List<UserSearchResult> Users,
    int TotalCount,
    int Page,
    int PageSize);

public record UserSearchResult(
    string Id,
    string? UserName,
    string? Email,
    string? DisplayName,
    bool IsLocked,
    bool IsAdmin,
    DateTimeOffset? LockoutEnd);
