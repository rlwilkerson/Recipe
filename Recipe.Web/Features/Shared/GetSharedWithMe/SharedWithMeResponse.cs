namespace Recipe.Web.Features.Shared.GetSharedWithMe;

public record SharedWithMeResponse(
    IReadOnlyList<SharedCookbookItem> Cookbooks,
    IReadOnlyList<SharedRecipeItem> Recipes);

public record SharedCookbookItem(
    string PublicId,
    string Slug,
    string Name,
    string? Description,
    string OwnerName,
    string Permission);

public record SharedRecipeItem(
    string PublicId,
    string Slug,
    string Title,
    string? Description,
    string OwnerName,
    string Permission);
