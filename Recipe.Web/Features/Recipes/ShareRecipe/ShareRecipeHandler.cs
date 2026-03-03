using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Recipes.ShareRecipe;

public class ShareRecipeHandler : IRequestHandler<ShareRecipeCommand>
{
    private readonly AppDbContext _db;

    public ShareRecipeHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(ShareRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _db.Recipes
            .FirstOrDefaultAsync(r => r.PublicId == request.RecipePublicId, cancellationToken);

        if (recipe == null)
            throw new InvalidOperationException($"Recipe with PublicId {request.RecipePublicId} not found");

        // Check if share already exists
        var existingShare = await _db.Shares
            .FirstOrDefaultAsync(s => 
                s.Scope == Models.ShareScope.Recipe && 
                s.RecipeId == recipe.Id && 
                s.TargetUserId == request.SharedWithUserId, 
                cancellationToken);

        if (existingShare != null)
        {
            existingShare.Permission = request.Permission;
        }
        else
        {
            var share = new Models.Share
            {
                Scope = Models.ShareScope.Recipe,
                Permission = request.Permission,
                RecipeId = recipe.Id,
                OwnerId = recipe.OwnerId,
                TargetUserId = request.SharedWithUserId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Shares.Add(share);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
