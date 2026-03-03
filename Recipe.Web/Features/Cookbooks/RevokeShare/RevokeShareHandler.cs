using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.RevokeShare;

public class RevokeShareHandler : IRequestHandler<RevokeShareCommand>
{
    private readonly AppDbContext _db;

    public RevokeShareHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RevokeShareCommand request, CancellationToken cancellationToken)
    {
        var share = await _db.Shares
            .FirstOrDefaultAsync(s => s.Id == request.ShareId, cancellationToken);

        if (share == null || share.OwnerId != request.RequestingUserId)
            return; // Silently ignore — not found or not owner

        _db.Shares.Remove(share);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
