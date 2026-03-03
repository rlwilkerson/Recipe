using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Cookbooks.AddRecipeToCookbook;
using Recipe.Web.Features.Cookbooks.GetCookbook;
using Recipe.Web.Features.Recipes.CreateRecipe;
using System.Security.Claims;

namespace Recipe.Web.Pages.Cookbooks;

[Authorize]
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

    public GetCookbookResponse? Result { get; set; }

    [BindProperty]
    public string? RecipeTitle { get; set; }

    [BindProperty]
    public string? RecipeDescription { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        if (Result is null)
            return NotFound();

        return Page();
    }

    public IActionResult OnGetAddRecipeModal()
    {
        return Partial("_AddRecipeModal", null);
    }

    public async Task<IActionResult> OnPostAddRecipeAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // Create the recipe
        var createResult = await _mediator.Send(new CreateRecipeCommand(
            RecipeTitle!,
            RecipeDescription,
            null, null, null, null, null,
            userId));

        // Add it to this cookbook
        await _mediator.Send(new AddRecipeToCookbookCommand(PublicId, createResult.PublicId, null));

        // Return updated recipe list
        var cookbook = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        return Partial("_RecipesList", cookbook?.Recipes ?? []);
    }
}
