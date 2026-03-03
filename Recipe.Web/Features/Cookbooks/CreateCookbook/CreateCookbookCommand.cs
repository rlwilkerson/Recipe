using MediatR;

namespace Recipe.Web.Features.Cookbooks.CreateCookbook;

public record CreateCookbookCommand(string Name, string? Description, string OwnerId) : IRequest<CreateCookbookResponse>;
