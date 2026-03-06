using MediatR;

namespace Recipe.AdminApi.Features.Users.SearchUsers;

public record SearchUsersQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IRequest<SearchUsersResponse>;
