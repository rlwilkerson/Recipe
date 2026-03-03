namespace Recipe.Web.Models;

public class CookbookRecipe
{
    public int CookbookId { get; set; }
    public Cookbook Cookbook { get; set; } = default!;
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;
    public int SortOrder { get; set; }
}
