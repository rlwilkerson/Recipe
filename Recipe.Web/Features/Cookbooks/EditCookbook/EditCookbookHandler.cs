using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;
using Recipe.Web.Services;

namespace Recipe.Web.Features.Cookbooks.EditCookbook;

public class EditCookbookHandler : IRequestHandler<EditCookbookCommand, EditCookbookResponse>
{
    private readonly AppDbContext _db;
    private readonly ISlugService _slugService;

    public EditCookbookHandler(AppDbContext db, ISlugService slugService)
    {
        _db = db;
        _slugService = slugService;
    }

    public async Task<EditCookbookResponse> Handle(EditCookbookCommand request, CancellationToken cancellationToken)
    {
        var cookbook = await _db.Cookbooks
            .FirstOrDefaultAsync(c => c.PublicId == request.PublicId, cancellationToken);

        if (cookbook == null || cookbook.OwnerId != request.OwnerId)
            throw new UnauthorizedAccessException("Cookbook not found or you do not have permission to edit it.");

        cookbook.Name = request.Name;
        cookbook.Description = request.Description;
        cookbook.Slug = _slugService.GenerateSlug(request.Name);
        cookbook.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new EditCookbookResponse(cookbook.PublicId, cookbook.Slug, cookbook.Name);
    }
}
