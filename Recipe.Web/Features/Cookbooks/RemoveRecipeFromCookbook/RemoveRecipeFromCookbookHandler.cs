using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.RemoveRecipeFromCookbook;

public class RemoveRecipeFromCookbookHandler : IRequestHandler<RemoveRecipeFromCookbookCommand>
{
    private readonly AppDbContext _db;

    public RemoveRecipeFromCookbookHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RemoveRecipeFromCookbookCommand request, CancellationToken cancellationToken)
    {
        var joinRow = await _db.CookbookRecipes
            .Include(cr => cr.Cookbook)
            .Include(cr => cr.Recipe)
            .FirstOrDefaultAsync(cr =>
                cr.Cookbook.PublicId == request.CookbookPublicId &&
                cr.Recipe.PublicId == request.RecipePublicId, cancellationToken);

        if (joinRow == null || joinRow.Cookbook.OwnerId != request.OwnerId)
            return; // Silently ignore — recipe not in cookbook or not owner

        _db.CookbookRecipes.Remove(joinRow);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
