namespace Recipe.Web.Features.Cookbooks.GetCookbook;

public record GetCookbookResponse(
    string PublicId,
    string Slug,
    string Name,
    string? Description,
    string OwnerName,
    DateTime CreatedAt,
    bool IsOwner,
    IReadOnlyList<CookbookRecipeItem> Recipes);

public record CookbookRecipeItem(
    string PublicId, 
    string Slug, 
    string Title, 
    int SortOrder,
    string? Description,
    int? PrepTime,
    int? CookTime,
    int? Servings);
