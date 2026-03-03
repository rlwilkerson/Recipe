using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.GetCookbook;

public class GetCookbookHandler : IRequestHandler<GetCookbookQuery, GetCookbookResponse?>
{
    private readonly AppDbContext _db;

    public GetCookbookHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetCookbookResponse?> Handle(GetCookbookQuery request, CancellationToken cancellationToken)
    {
        var cookbook = await _db.Cookbooks
            .Include(c => c.Owner)
            .Include(c => c.CookbookRecipes)
                .ThenInclude(cr => cr.Recipe)
            .Include(c => c.Shares)
            .FirstOrDefaultAsync(c => c.PublicId == request.PublicId, cancellationToken);

        if (cookbook == null)
            return null;

        // Check authorization: user is owner OR has a share allowing Read/Update
        if (!string.IsNullOrEmpty(request.UserId))
        {
            bool canRead = cookbook.OwnerId == request.UserId ||
                           cookbook.Shares.Any(s =>
                               s.TargetUserId == request.UserId &&
                               s.Scope == Models.ShareScope.Cookbook &&
                               (s.Permission == Models.SharePermission.Read || s.Permission == Models.SharePermission.Update));

            if (!canRead)
                return null; // Return NotFound to avoid revealing existence
        }

        var recipeItems = cookbook.CookbookRecipes
            .OrderBy(cr => cr.SortOrder)
            .Select(cr => new CookbookRecipeItem(
                cr.Recipe.PublicId,
                cr.Recipe.Slug,
                cr.Recipe.Title,
                cr.SortOrder))
            .ToList();

        return new GetCookbookResponse(
            cookbook.PublicId,
            cookbook.Slug,
            cookbook.Name,
            cookbook.Description,
            cookbook.Owner.UserName ?? "Unknown",
            cookbook.CreatedAt,
            recipeItems);
    }
}
