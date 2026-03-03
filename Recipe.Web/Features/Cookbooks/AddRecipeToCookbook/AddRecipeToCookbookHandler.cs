using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Cookbooks.AddRecipeToCookbook;

public class AddRecipeToCookbookHandler : IRequestHandler<AddRecipeToCookbookCommand>
{
    private readonly AppDbContext _db;

    public AddRecipeToCookbookHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(AddRecipeToCookbookCommand request, CancellationToken cancellationToken)
    {
        var cookbook = await _db.Cookbooks
            .Include(c => c.CookbookRecipes)
            .FirstOrDefaultAsync(c => c.PublicId == request.CookbookPublicId, cancellationToken);

        if (cookbook == null)
            throw new InvalidOperationException($"Cookbook with PublicId {request.CookbookPublicId} not found");

        var recipe = await _db.Recipes
            .FirstOrDefaultAsync(r => r.PublicId == request.RecipePublicId, cancellationToken);

        if (recipe == null)
            throw new InvalidOperationException($"Recipe with PublicId {request.RecipePublicId} not found");

        // Check if already exists (idempotent)
        if (cookbook.CookbookRecipes.Any(cr => cr.RecipeId == recipe.Id))
            return;

        var sortOrder = request.SortOrder ?? (cookbook.CookbookRecipes.Any() 
            ? cookbook.CookbookRecipes.Max(cr => cr.SortOrder) + 1 
            : 1);

        var cookbookRecipe = new Models.CookbookRecipe
        {
            CookbookId = cookbook.Id,
            RecipeId = recipe.Id,
            SortOrder = sortOrder
        };

        _db.CookbookRecipes.Add(cookbookRecipe);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
