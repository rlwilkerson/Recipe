using MediatR;

namespace Recipe.Web.Features.Cookbooks.ShareCookbook;

public record ShareCookbookCommand(string CookbookPublicId, string SharedWithUserId, Models.SharePermission Permission) : IRequest;
