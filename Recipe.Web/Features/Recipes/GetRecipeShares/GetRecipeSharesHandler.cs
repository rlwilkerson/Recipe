using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Recipes.GetRecipeShares;

public class GetRecipeSharesHandler : IRequestHandler<GetRecipeSharesQuery, IReadOnlyList<RecipeShareItem>>
{
    private readonly AppDbContext _db;

    public GetRecipeSharesHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RecipeShareItem>> Handle(GetRecipeSharesQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _db.Recipes
            .FirstOrDefaultAsync(r => r.PublicId == request.RecipePublicId, cancellationToken);

        if (recipe == null || recipe.OwnerId != request.RequestingUserId)
            return [];

        return await _db.Shares
            .Include(s => s.TargetUser)
            .Where(s => s.RecipeId == recipe.Id && s.Scope == Models.ShareScope.Recipe)
            .Select(s => new RecipeShareItem(
                s.Id,
                s.TargetUser.Email ?? s.TargetUser.UserName ?? "Unknown",
                s.Permission.ToString()))
            .ToListAsync(cancellationToken);
    }
}
