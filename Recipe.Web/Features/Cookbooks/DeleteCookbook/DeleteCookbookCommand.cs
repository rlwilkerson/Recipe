using MediatR;

namespace Recipe.Web.Features.Cookbooks.DeleteCookbook;

public record DeleteCookbookCommand(string CookbookPublicId, string OwnerId) : IRequest;
