using MediatR;

namespace Recipe.Web.Features.Recipes.CreateRecipe;

public record CreateRecipeCommand(
    string Title,
    string? Description,
    string? Ingredients,
    string? Instructions,
    int? PrepTime,
    int? CookTime,
    int? Servings,
    string OwnerId) : IRequest<CreateRecipeResponse>;
