using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Cookbooks.AddRecipeToCookbook;
using Recipe.Web.Features.Cookbooks.DeleteCookbook;
using Recipe.Web.Features.Cookbooks.EditCookbook;
using Recipe.Web.Features.Cookbooks.GetCookbook;
using Recipe.Web.Features.Cookbooks.RemoveRecipeFromCookbook;
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

    // Recipe add
    [BindProperty] public string? RecipeTitle { get; set; }
    [BindProperty] public string? RecipeDescription { get; set; }
    [BindProperty] public string? RecipeIngredients { get; set; }
    [BindProperty] public string? RecipeInstructions { get; set; }
    [BindProperty] public int? RecipePrepTime { get; set; }
    [BindProperty] public int? RecipeCookTime { get; set; }
    [BindProperty] public int? RecipeServings { get; set; }

    // Cookbook edit
    [BindProperty] public string? EditName { get; set; }
    [BindProperty] public string? EditDescription { get; set; }

    // Remove recipe
    [BindProperty] public string? RecipeToRemovePublicId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        if (Result is null)
            return NotFound();

        return Page();
    }

    public IActionResult OnGetAddRecipeFormAsync()
    {
        return Partial("_AddRecipeForm", null);
    }

    public async Task<IActionResult> OnGetRecipeListAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        if (Result is null)
            return NotFound();
        return Partial("_RecipeList", Result);
    }

    public async Task<IActionResult> OnGetEditFormAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        if (Result is null || !Result.IsOwner)
            return NotFound();
        return Partial("_CookbookEditForm", Result);
    }

    public async Task<IActionResult> OnGetViewHeaderAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Result = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        if (Result is null)
            return NotFound();
        return Partial("_CookbookHeader", Result);
    }

    public async Task<IActionResult> OnPostAddRecipeAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var createResult = await _mediator.Send(new CreateRecipeCommand(
            RecipeTitle!, RecipeDescription, RecipeIngredients, RecipeInstructions,
            RecipePrepTime, RecipeCookTime, RecipeServings, userId));

        await _mediator.Send(new AddRecipeToCookbookCommand(PublicId, createResult.PublicId, null));

        var cookbook = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        return Partial("_RecipeList", cookbook);
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new EditCookbookCommand(PublicId, EditName!, EditDescription, userId));
        Response.Headers["HX-Redirect"] = $"/cookbooks/{result.PublicId}/{result.Slug}";
        return new OkResult();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new DeleteCookbookCommand(PublicId, userId));
        Response.Headers["HX-Redirect"] = "/Cookbooks";
        return new OkResult();
    }

    public async Task<IActionResult> OnPostRemoveRecipeAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new RemoveRecipeFromCookbookCommand(PublicId, RecipeToRemovePublicId!, userId));
        var cookbook = await _mediator.Send(new GetCookbookQuery(PublicId, userId));
        return Partial("_RecipeList", cookbook);
    }
}
