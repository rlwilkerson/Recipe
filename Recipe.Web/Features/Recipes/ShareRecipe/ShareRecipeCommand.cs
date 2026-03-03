using MediatR;

namespace Recipe.Web.Features.Recipes.ShareRecipe;

public record ShareRecipeCommand(string RecipePublicId, string SharedWithUserId, Models.SharePermission Permission) : IRequest;
