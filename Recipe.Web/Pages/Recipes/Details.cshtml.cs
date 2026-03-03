using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Recipes.GetRecipe;
using System.Security.Claims;

namespace Recipe.Web.Pages.Recipes;

public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;

    public DetailsModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [FromRoute]
    public string PublicId { get; set; } = default!;

    [FromRoute]
    public string Slug { get; set; } = default!;

    public GetRecipeResponse? Result { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetRecipeQuery(PublicId, userId));
        if (Result is null)
            return NotFound();

        return Page();
    }
}
