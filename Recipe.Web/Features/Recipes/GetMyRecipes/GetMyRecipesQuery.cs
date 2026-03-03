using MediatR;

namespace Recipe.Web.Features.Recipes.GetMyRecipes;

public record GetMyRecipesQuery(string OwnerId) : IRequest<IReadOnlyList<MyRecipeItem>>;
