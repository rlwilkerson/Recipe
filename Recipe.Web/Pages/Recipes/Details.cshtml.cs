using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Recipes.CreateRecipe;
using Recipe.Web.Features.Recipes.EditRecipe;
using Recipe.Web.Features.Recipes.GetRecipe;
using System.Security.Claims;

namespace Recipe.Web.Pages.Recipes;

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

    public GetRecipeResponse? Result { get; set; }

    [BindProperty] public string? EditTitle { get; set; }
    [BindProperty] public string? EditDescription { get; set; }
    [BindProperty] public string? EditIngredients { get; set; }
    [BindProperty] public string? EditInstructions { get; set; }
    [BindProperty] public int? EditPrepTime { get; set; }
    [BindProperty] public int? EditCookTime { get; set; }
    [BindProperty] public int? EditServings { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetRecipeQuery(PublicId, userId));
        if (Result is null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnGetEditFormAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetRecipeQuery(PublicId, userId));
        if (Result is null || !Result.IsOwner)
            return NotFound();
        return Partial("_RecipeEditForm", Result);
    }

    public async Task<IActionResult> OnGetViewContentAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetRecipeQuery(PublicId, userId));
        if (Result is null)
            return NotFound();
        return Partial("_RecipeViewContent", Result);
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new EditRecipeCommand(
            PublicId, EditTitle!, EditDescription,
            EditIngredients, EditInstructions,
            EditPrepTime, EditCookTime, EditServings, userId));
        // HTMX redirect to updated URL
        Response.Headers["HX-Redirect"] = $"/recipes/{result.PublicId}/{result.NewSlug}";
        return new OkResult();
    }

    public async Task<IActionResult> OnGetCloneFormAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetRecipeQuery(PublicId, userId));
        if (Result is null)
            return NotFound();
        return Partial("_RecipeCloneForm", Result);
    }

    public async Task<IActionResult> OnPostSaveCloneAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new CreateRecipeCommand(
            EditTitle!, EditDescription,
            EditIngredients, EditInstructions,
            EditPrepTime, EditCookTime, EditServings, userId));
        Response.Headers["HX-Redirect"] = $"/recipes/{result.PublicId}/{result.Slug}";
        return new OkResult();
    }
}
