using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Authorization;

public class GetRecipeAccessHandler : IRequestHandler<GetRecipeAccessQuery, bool>
{
    private readonly AppDbContext _db;

    public GetRecipeAccessHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(GetRecipeAccessQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Shares)
            .Include(r => r.CookbookRecipes)
                .ThenInclude(cr => cr.Cookbook)
                    .ThenInclude(c => c.Shares)
            .FirstOrDefaultAsync(r => r.PublicId == request.RecipePublicId, cancellationToken);

        if (recipe == null)
            return false;

        // User is owner
        if (recipe.OwnerId == request.UserId)
            return true;

        // User has direct recipe share with Read or Update permission
        if (recipe.Shares.Any(s => 
            s.TargetUserId == request.UserId && 
            s.Scope == Models.ShareScope.Recipe &&
            (s.Permission == Models.SharePermission.Read || s.Permission == Models.SharePermission.Update)))
            return true;

        // User can read ANY cookbook containing this recipe
        return recipe.CookbookRecipes.Any(cr => 
            cr.Cookbook.OwnerId == request.UserId ||
            cr.Cookbook.Shares.Any(s => 
                s.TargetUserId == request.UserId && 
                s.Scope == Models.ShareScope.Cookbook &&
                (s.Permission == Models.SharePermission.Read || s.Permission == Models.SharePermission.Update)));
    }
}
