using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.DeleteCookbook;

public class DeleteCookbookHandler : IRequestHandler<DeleteCookbookCommand>
{
    private readonly AppDbContext _db;

    public DeleteCookbookHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteCookbookCommand request, CancellationToken cancellationToken)
    {
        var cookbook = await _db.Cookbooks
            .Include(c => c.CookbookRecipes)
                .ThenInclude(cr => cr.Recipe)
                    .ThenInclude(r => r.Shares)
            .Include(c => c.Shares)
            .FirstOrDefaultAsync(c => c.PublicId == request.CookbookPublicId, cancellationToken);

        if (cookbook == null || cookbook.OwnerId != request.OwnerId)
            throw new UnauthorizedAccessException("Cookbook not found or you do not have permission to delete it.");

        var recipes = cookbook.CookbookRecipes.Select(cr => cr.Recipe).ToList();

        // Nullify OriginalRecipeId on recipes that clone any of these recipes (Restrict FK)
        var recipeIds = recipes.Select(r => r.Id).ToList();
        if (recipeIds.Count > 0)
        {
            await _db.Recipes
                .Where(r => r.OriginalRecipeId != null && recipeIds.Contains(r.OriginalRecipeId.Value))
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.OriginalRecipeId, (int?)null), cancellationToken);
        }

        // Remove recipe shares explicitly (nullable FK defaults to SetNull, not Cascade)
        foreach (var recipe in recipes)
        {
            _db.Shares.RemoveRange(recipe.Shares);
        }

        // Remove cookbook shares
        _db.Shares.RemoveRange(cookbook.Shares);

        // Remove recipes (cascades CookbookRecipe join rows for all cookbooks)
        _db.Recipes.RemoveRange(recipes);

        // Remove cookbook
        _db.Cookbooks.Remove(cookbook);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
