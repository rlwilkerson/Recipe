using MediatR;

namespace Recipe.Web.Features.Cookbooks.EditCookbook;

public record EditCookbookCommand(string PublicId, string Name, string? Description, string OwnerId) : IRequest<EditCookbookResponse>;
