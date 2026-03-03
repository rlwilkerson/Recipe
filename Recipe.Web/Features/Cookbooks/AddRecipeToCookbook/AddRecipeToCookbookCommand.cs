using MediatR;

namespace Recipe.Web.Features.Cookbooks.AddRecipeToCookbook;

public record AddRecipeToCookbookCommand(string CookbookPublicId, string RecipePublicId, int? SortOrder) : IRequest;
