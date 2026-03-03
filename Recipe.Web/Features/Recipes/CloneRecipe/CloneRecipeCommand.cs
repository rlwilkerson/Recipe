using MediatR;

namespace Recipe.Web.Features.Recipes.CloneRecipe;

public record CloneRecipeCommand(string SourceRecipePublicId, string OwnerId) : IRequest;
