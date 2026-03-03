using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Authorization;

public class GetCookbookAccessHandler : IRequestHandler<GetCookbookAccessQuery, bool>
{
    private readonly AppDbContext _db;

    public GetCookbookAccessHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(GetCookbookAccessQuery request, CancellationToken cancellationToken)
    {
        var cookbook = await _db.Cookbooks
            .Include(c => c.Shares)
            .FirstOrDefaultAsync(c => c.PublicId == request.CookbookPublicId, cancellationToken);

        if (cookbook == null)
            return false;

        // User is owner
        if (cookbook.OwnerId == request.UserId)
            return true;

        // User has a share with Read or Update permission
        return cookbook.Shares.Any(s => 
            s.TargetUserId == request.UserId && 
            s.Scope == Models.ShareScope.Cookbook &&
            (s.Permission == Models.SharePermission.Read || s.Permission == Models.SharePermission.Update));
    }
}
