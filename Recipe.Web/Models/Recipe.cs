namespace Recipe.Web.Models;

public class Recipe
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = default!;
    public ApplicationUser Owner { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? Ingredients { get; set; }
    public string? Instructions { get; set; }
    public int? PrepTime { get; set; }
    public int? CookTime { get; set; }
    public int? Servings { get; set; }
    public string PublicId { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public int? OriginalRecipeId { get; set; }
    public Recipe? OriginalRecipe { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<CookbookRecipe> CookbookRecipes { get; set; } = [];
    public ICollection<Share> Shares { get; set; } = [];
}
