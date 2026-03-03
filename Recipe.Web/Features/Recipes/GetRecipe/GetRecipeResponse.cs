namespace Recipe.Web.Features.Recipes.GetRecipe;

public record GetRecipeResponse(
    string PublicId,
    string Slug,
    string Title,
    string? Description,
    string? Ingredients,
    string? Instructions,
    int? PrepTime,
    int? CookTime,
    int? Servings,
    string OwnerName,
    string? OriginalRecipePublicId,
    DateTime CreatedAt,
    bool IsOwner,
    IReadOnlyList<RecipeCookbookInfo> Cookbooks);
