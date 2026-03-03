using MediatR;
using Recipe.Web.Data;
using Recipe.Web.Services;

namespace Recipe.Web.Features.Cookbooks.CreateCookbook;

public class CreateCookbookHandler : IRequestHandler<CreateCookbookCommand, CreateCookbookResponse>
{
    private readonly AppDbContext _db;
    private readonly IPublicIdService _publicIdService;
    private readonly ISlugService _slugService;

    public CreateCookbookHandler(AppDbContext db, IPublicIdService publicIdService, ISlugService slugService)
    {
        _db = db;
        _publicIdService = publicIdService;
        _slugService = slugService;
    }

    public async Task<CreateCookbookResponse> Handle(CreateCookbookCommand request, CancellationToken cancellationToken)
    {
        var publicId = await _publicIdService.GenerateForCookbookAsync(_db);
        var slug = _slugService.GenerateSlug(request.Name);

        var cookbook = new Models.Cookbook
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.OwnerId,
            PublicId = publicId,
            Slug = slug,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Cookbooks.Add(cookbook);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateCookbookResponse(cookbook.Id, cookbook.PublicId, cookbook.Slug, cookbook.Name);
    }
}
