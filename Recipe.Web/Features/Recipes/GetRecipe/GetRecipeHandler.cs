using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Recipes.GetRecipe;

public class GetRecipeHandler : IRequestHandler<GetRecipeQuery, GetRecipeResponse?>
{
    private readonly AppDbContext _db;

    public GetRecipeHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetRecipeResponse?> Handle(GetRecipeQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Owner)
            .Include(r => r.Shares)
            .Include(r => r.OriginalRecipe)
            .Include(r => r.CookbookRecipes)
                .ThenInclude(cr => cr.Cookbook)
                    .ThenInclude(c => c.Shares)
            .FirstOrDefaultAsync(r => r.PublicId == request.PublicId, cancellationToken);

        if (recipe == null)
            return null;

        // Check authorization: user is owner OR has direct recipe share OR can read any cookbook containing this recipe
        if (!string.IsNullOrEmpty(request.UserId))
        {
            bool canRead = recipe.OwnerId == request.UserId ||
                           recipe.Shares.Any(s =>
                               s.TargetUserId == request.UserId &&
                               s.Scope == Models.ShareScope.Recipe &&
                               (s.Permission == Models.SharePermission.Read || s.Permission == Models.SharePermission.Update)) ||
                           recipe.CookbookRecipes.Any(cr =>
                               cr.Cookbook.OwnerId == request.UserId ||
                               cr.Cookbook.Shares.Any(s =>
                                   s.TargetUserId == request.UserId &&
                                   s.Scope == Models.ShareScope.Cookbook &&
                                   (s.Permission == Models.SharePermission.Read || s.Permission == Models.SharePermission.Update)));

            if (!canRead)
                return null; // Return NotFound to avoid revealing existence
        }

        return new GetRecipeResponse(
            recipe.PublicId,
            recipe.Slug,
            recipe.Title,
            recipe.Description,
            recipe.Ingredients,
            recipe.Instructions,
            recipe.PrepTime,
            recipe.CookTime,
            recipe.Servings,
            recipe.Owner.UserName ?? "Unknown",
            recipe.OriginalRecipe?.PublicId,
            recipe.CreatedAt,
            recipe.OwnerId == request.UserId);
    }
}
