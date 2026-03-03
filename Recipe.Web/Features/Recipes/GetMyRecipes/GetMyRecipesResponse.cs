namespace Recipe.Web.Features.Recipes.GetMyRecipes;

public record MyRecipeItem(
    string PublicId,
    string Slug,
    string Title,
    string? Description,
    int? PrepTime,
    int? CookTime,
    int? Servings,
    DateTime CreatedAt);
