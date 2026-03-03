using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.GetCookbookShares;

public class GetCookbookSharesHandler : IRequestHandler<GetCookbookSharesQuery, IReadOnlyList<CookbookShareItem>>
{
    private readonly AppDbContext _db;

    public GetCookbookSharesHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CookbookShareItem>> Handle(GetCookbookSharesQuery request, CancellationToken cancellationToken)
    {
        var cookbook = await _db.Cookbooks
            .FirstOrDefaultAsync(c => c.PublicId == request.CookbookPublicId, cancellationToken);

        if (cookbook == null || cookbook.OwnerId != request.RequestingUserId)
            return [];

        return await _db.Shares
            .Include(s => s.TargetUser)
            .Where(s => s.CookbookId == cookbook.Id && s.Scope == Models.ShareScope.Cookbook)
            .Select(s => new CookbookShareItem(
                s.Id,
                s.TargetUser.Email ?? s.TargetUser.UserName ?? "Unknown",
                s.Permission.ToString()))
            .ToListAsync(cancellationToken);
    }
}
