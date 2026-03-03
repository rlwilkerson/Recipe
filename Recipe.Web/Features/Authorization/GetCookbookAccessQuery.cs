using MediatR;

namespace Recipe.Web.Features.Authorization;

public record GetCookbookAccessQuery(string CookbookPublicId, string UserId) : IRequest<bool>;
