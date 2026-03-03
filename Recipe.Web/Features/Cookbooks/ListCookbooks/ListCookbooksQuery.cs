using MediatR;

namespace Recipe.Web.Features.Cookbooks.ListCookbooks;

public record ListCookbooksQuery(string OwnerId) : IRequest<ListCookbooksResponse>;
