using MediatR;

namespace Recipe.Web.Features.Cookbooks.GetCookbookShares;

public record GetCookbookSharesQuery(string CookbookPublicId, string RequestingUserId) : IRequest<IReadOnlyList<CookbookShareItem>>;
