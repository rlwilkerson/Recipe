using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Recipes.GetMyRecipes;
using System.Security.Claims;

namespace Recipe.Web.Pages.Recipes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IReadOnlyList<MyRecipeItem> Recipes { get; set; } = [];

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Recipes = await _mediator.Send(new GetMyRecipesQuery(userId));
    }
}
