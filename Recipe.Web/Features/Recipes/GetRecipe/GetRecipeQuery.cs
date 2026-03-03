using MediatR;

namespace Recipe.Web.Features.Recipes.GetRecipe;

public record GetRecipeQuery(string PublicId, string? UserId) : IRequest<GetRecipeResponse?>;
