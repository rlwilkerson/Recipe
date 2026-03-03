using MediatR;

namespace Recipe.Web.Features.Recipes.GetRecipeShares;

public record GetRecipeSharesQuery(string RecipePublicId, string RequestingUserId) : IRequest<IReadOnlyList<RecipeShareItem>>;
