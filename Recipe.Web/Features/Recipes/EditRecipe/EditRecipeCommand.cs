using MediatR;
namespace Recipe.Web.Features.Recipes.EditRecipe;
public record EditRecipeCommand(
    string PublicId,
    string Title,
    string? Description,
    string? Ingredients,
    string? Instructions,
    int? PrepTime,
    int? CookTime,
    int? Servings,
    string RequestingUserId) : IRequest<EditRecipeResponse>;
