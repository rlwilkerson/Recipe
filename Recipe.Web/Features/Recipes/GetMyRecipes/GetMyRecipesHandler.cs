using MediatR;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Features.Recipes.GetMyRecipes;

public class GetMyRecipesHandler : IRequestHandler<GetMyRecipesQuery, IReadOnlyList<MyRecipeItem>>
{
    private readonly AppDbContext _db;

    public GetMyRecipesHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MyRecipeItem>> Handle(GetMyRecipesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Recipes
            .Where(r => r.OwnerId == request.OwnerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MyRecipeItem(
                r.PublicId,
                r.Slug,
                r.Title,
                r.Description,
                r.PrepTime,
                r.CookTime,
                r.Servings,
                r.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
