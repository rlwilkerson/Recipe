using MediatR;
using Recipe.Web.Data;
using Recipe.Web.Services;

namespace Recipe.Web.Features.Recipes.CreateRecipe;

public class CreateRecipeHandler : IRequestHandler<CreateRecipeCommand, CreateRecipeResponse>
{
    private readonly AppDbContext _db;
    private readonly IPublicIdService _publicIdService;
    private readonly ISlugService _slugService;

    public CreateRecipeHandler(AppDbContext db, IPublicIdService publicIdService, ISlugService slugService)
    {
        _db = db;
        _publicIdService = publicIdService;
        _slugService = slugService;
    }

    public async Task<CreateRecipeResponse> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var publicId = await _publicIdService.GenerateForRecipeAsync(_db);
        var slug = _slugService.GenerateSlug(request.Title);

        var recipe = new Models.Recipe
        {
            Title = request.Title,
            Description = request.Description,
            Ingredients = request.Ingredients,
            Instructions = request.Instructions,
            PrepTime = request.PrepTime,
            CookTime = request.CookTime,
            Servings = request.Servings,
            OwnerId = request.OwnerId,
            PublicId = publicId,
            Slug = slug,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateRecipeResponse(recipe.Id, recipe.PublicId, recipe.Slug, recipe.Title);
    }
}
