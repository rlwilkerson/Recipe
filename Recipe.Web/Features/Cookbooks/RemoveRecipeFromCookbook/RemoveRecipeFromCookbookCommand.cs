using MediatR;

namespace Recipe.Web.Features.Cookbooks.RemoveRecipeFromCookbook;

public record RemoveRecipeFromCookbookCommand(string CookbookPublicId, string RecipePublicId, string OwnerId) : IRequest;
