using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;
using Recipe.Web.Services;

namespace Recipe.Web.Features.Recipes.EditRecipe;

public class EditRecipeHandler : IRequestHandler<EditRecipeCommand, EditRecipeResponse>
{
    private readonly AppDbContext _db;
    private readonly ISlugService _slugService;

    public EditRecipeHandler(AppDbContext db, ISlugService slugService)
    {
        _db = db;
        _slugService = slugService;
    }

    public async Task<EditRecipeResponse> Handle(EditRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _db.Recipes
            .FirstOrDefaultAsync(r => r.PublicId == request.PublicId, cancellationToken);

        if (recipe == null || recipe.OwnerId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Recipe not found or you do not have permission to edit it.");

        recipe.Title = request.Title;
        recipe.Description = request.Description;
        recipe.Ingredients = request.Ingredients;
        recipe.Instructions = request.Instructions;
        recipe.PrepTime = request.PrepTime;
        recipe.CookTime = request.CookTime;
        recipe.Servings = request.Servings;
        recipe.Slug = _slugService.GenerateSlug(request.Title);
        recipe.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new EditRecipeResponse(recipe.PublicId, recipe.Slug, recipe.Title);
    }
}
