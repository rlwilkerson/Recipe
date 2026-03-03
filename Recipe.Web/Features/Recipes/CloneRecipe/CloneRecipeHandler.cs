using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;
using Recipe.Web.Services;

namespace Recipe.Web.Features.Recipes.CloneRecipe;

public class CloneRecipeHandler : IRequestHandler<CloneRecipeCommand>
{
    private readonly AppDbContext _db;
    private readonly IPublicIdService _publicIdService;

    public CloneRecipeHandler(AppDbContext db, IPublicIdService publicIdService)
    {
        _db = db;
        _publicIdService = publicIdService;
    }

    public async Task Handle(CloneRecipeCommand request, CancellationToken cancellationToken)
    {
        var sourceRecipe = await _db.Recipes
            .FirstOrDefaultAsync(r => r.PublicId == request.SourceRecipePublicId, cancellationToken);

        if (sourceRecipe == null)
            throw new InvalidOperationException($"Source recipe with PublicId {request.SourceRecipePublicId} not found");

        var publicId = await _publicIdService.GenerateForRecipeAsync(_db);

        var clonedRecipe = new Models.Recipe
        {
            Title = sourceRecipe.Title,
            Description = sourceRecipe.Description,
            Ingredients = sourceRecipe.Ingredients,
            Instructions = sourceRecipe.Instructions,
            PrepTime = sourceRecipe.PrepTime,
            CookTime = sourceRecipe.CookTime,
            Servings = sourceRecipe.Servings,
            OwnerId = request.OwnerId,
            PublicId = publicId,
            Slug = sourceRecipe.Slug,
            OriginalRecipeId = sourceRecipe.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Recipes.Add(clonedRecipe);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
