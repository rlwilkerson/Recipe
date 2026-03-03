using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.ListCookbooks;

public class ListCookbooksHandler : IRequestHandler<ListCookbooksQuery, ListCookbooksResponse>
{
    private readonly AppDbContext _db;

    public ListCookbooksHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ListCookbooksResponse> Handle(ListCookbooksQuery request, CancellationToken cancellationToken)
    {
        var cookbooks = await _db.Cookbooks
            .Where(c => c.OwnerId == request.OwnerId || 
                        c.Shares.Any(s => s.TargetUserId == request.OwnerId && s.Scope == Models.ShareScope.Cookbook))
            .Select(c => new CookbookListItem(
                c.PublicId,
                c.Slug,
                c.Name,
                c.Description,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ListCookbooksResponse(cookbooks);
    }
}
