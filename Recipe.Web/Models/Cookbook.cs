namespace Recipe.Web.Models;

public class Cookbook
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = default!;
    public ApplicationUser Owner { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string PublicId { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<CookbookRecipe> CookbookRecipes { get; set; } = [];
    public ICollection<Share> Shares { get; set; } = [];
}
