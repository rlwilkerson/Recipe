using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Shared.GetSharedWithMe;

public class GetSharedWithMeHandler : IRequestHandler<GetSharedWithMeQuery, SharedWithMeResponse>
{
    private readonly AppDbContext _db;

    public GetSharedWithMeHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SharedWithMeResponse> Handle(GetSharedWithMeQuery request, CancellationToken cancellationToken)
    {
        var shares = await _db.Shares
            .Include(s => s.Cookbook).ThenInclude(c => c!.Owner)
            .Include(s => s.Recipe).ThenInclude(r => r!.Owner)
            .Where(s => s.TargetUserId == request.UserId)
            .ToListAsync(cancellationToken);

        var cookbooks = shares
            .Where(s => s.Scope == Models.ShareScope.Cookbook && s.Cookbook != null)
            .Select(s => new SharedCookbookItem(
                s.Cookbook!.PublicId,
                s.Cookbook.Slug,
                s.Cookbook.Name,
                s.Cookbook.Description,
                s.Cookbook.Owner?.UserName ?? "Unknown",
                s.Permission.ToString()))
            .ToList();

        var recipes = shares
            .Where(s => s.Scope == Models.ShareScope.Recipe && s.Recipe != null)
            .Select(s => new SharedRecipeItem(
                s.Recipe!.PublicId,
                s.Recipe.Slug,
                s.Recipe.Title,
                s.Recipe.Description,
                s.Recipe.Owner?.UserName ?? "Unknown",
                s.Permission.ToString()))
            .ToList();

        return new SharedWithMeResponse(cookbooks, recipes);
    }
}
