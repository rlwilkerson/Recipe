namespace Recipe.Web.Features.Cookbooks.ListCookbooks;

public record ListCookbooksResponse(IReadOnlyList<CookbookListItem> Cookbooks);

public record CookbookListItem(string PublicId, string Slug, string Name, string? Description, DateTime CreatedAt);
