using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Recipes.DeleteRecipe;

public class DeleteRecipeHandler : IRequestHandler<DeleteRecipeCommand>
{
    private readonly AppDbContext _db;

    public DeleteRecipeHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Shares)
            .FirstOrDefaultAsync(r => r.PublicId == request.RecipePublicId, cancellationToken);

        if (recipe == null || recipe.OwnerId != request.OwnerId)
            throw new UnauthorizedAccessException("Recipe not found or you do not have permission to delete it.");

        // Nullify OriginalRecipeId on clones (Restrict FK prevents cascade delete)
        await _db.Recipes
            .Where(r => r.OriginalRecipeId == recipe.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.OriginalRecipeId, (int?)null), cancellationToken);

        // Remove shares explicitly (nullable FK defaults to SetNull, not Cascade)
        _db.Shares.RemoveRange(recipe.Shares);

        // Remove recipe (cascades CookbookRecipe join rows)
        _db.Recipes.Remove(recipe);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
