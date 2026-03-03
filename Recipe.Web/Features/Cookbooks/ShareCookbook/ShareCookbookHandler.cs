using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.ShareCookbook;

public class ShareCookbookHandler : IRequestHandler<ShareCookbookCommand>
{
    private readonly AppDbContext _db;

    public ShareCookbookHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(ShareCookbookCommand request, CancellationToken cancellationToken)
    {
        var cookbook = await _db.Cookbooks
            .FirstOrDefaultAsync(c => c.PublicId == request.CookbookPublicId, cancellationToken);

        if (cookbook == null)
            throw new InvalidOperationException($"Cookbook with PublicId {request.CookbookPublicId} not found");

        // Check if share already exists
        var existingShare = await _db.Shares
            .FirstOrDefaultAsync(s => 
                s.Scope == Models.ShareScope.Cookbook && 
                s.CookbookId == cookbook.Id && 
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
                Scope = Models.ShareScope.Cookbook,
                Permission = request.Permission,
                CookbookId = cookbook.Id,
                OwnerId = cookbook.OwnerId,
                TargetUserId = request.SharedWithUserId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Shares.Add(share);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
