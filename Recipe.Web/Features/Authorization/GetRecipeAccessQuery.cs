using MediatR;

namespace Recipe.Web.Features.Authorization;

public record GetRecipeAccessQuery(string RecipePublicId, string UserId) : IRequest<bool>;
