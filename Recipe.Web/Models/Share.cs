namespace Recipe.Web.Models;

public enum ShareScope
{
    Cookbook,
    Recipe
}

public enum SharePermission
{
    Read,
    Update
}

public class Share
{
    public int Id { get; set; }
    public ShareScope Scope { get; set; }
    public SharePermission Permission { get; set; }
    public string OwnerId { get; set; } = default!;
    public ApplicationUser Owner { get; set; } = default!;
    public string TargetUserId { get; set; } = default!;
    public ApplicationUser TargetUser { get; set; } = default!;
    public int? CookbookId { get; set; }
    public Cookbook? Cookbook { get; set; }
    public int? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public DateTime CreatedAt { get; set; }
}
