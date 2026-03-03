using MediatR;

namespace Recipe.Web.Features.Recipes.DeleteRecipe;

public record DeleteRecipeCommand(string RecipePublicId, string OwnerId) : IRequest;
