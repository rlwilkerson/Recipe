using MediatR;

namespace Recipe.Web.Features.Cookbooks.GetCookbook;

public record GetCookbookQuery(string PublicId, string? UserId) : IRequest<GetCookbookResponse?>;
