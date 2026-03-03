using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Cookbooks.AddRecipeToCookbook;
using Recipe.Web.Features.Recipes.CreateRecipe;
using Recipe.Web.Features.Recipes.DeleteRecipe;
using Recipe.Web.Features.Recipes.EditRecipe;
using Recipe.Web.Features.Recipes.GetRecipe;
using Recipe.Web.Features.Recipes.GetRecipeShares;
using Recipe.Web.Features.Recipes.ShareRecipe;
using Recipe.Web.Features.Cookbooks.RevokeShare;
using Recipe.Web.Models;
using System.Security.Claims;

namespace Recipe.Web.Pages.Recipes;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
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

    // Share
    [BindProperty] public string? ShareEmail { get; set; }
    [BindProperty] public string? SharePermission { get; set; }
    [BindProperty] public int? RevokeShareId { get; set; }

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

        // Get original recipe to know which cookbooks it belongs to
        var original = await _mediator.Send(new GetRecipeQuery(PublicId, userId));

        // Create the cloned recipe
        var result = await _mediator.Send(new CreateRecipeCommand(
            EditTitle!, EditDescription,
            EditIngredients, EditInstructions,
            EditPrepTime, EditCookTime, EditServings, userId));

        // Add to same cookbook(s) as original
        foreach (var cookbook in original?.Cookbooks ?? [])
        {
            await _mediator.Send(new AddRecipeToCookbookCommand(cookbook.PublicId, result.PublicId, null));
        }

        Response.Headers["HX-Redirect"] = $"/recipes/{result.PublicId}/{result.Slug}";
        return new OkResult();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new DeleteRecipeCommand(PublicId, userId));
        Response.Headers["HX-Redirect"] = "/Cookbooks";
        return new OkResult();
    }

    public async Task<IActionResult> OnGetShareSectionAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var shares = await _mediator.Send(new GetRecipeSharesQuery(PublicId, userId));
        return Partial("_RecipeShareSection", shares);
    }

    public async Task<IActionResult> OnPostShareAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var targetUser = await _userManager.FindByEmailAsync(ShareEmail ?? string.Empty);
        if (targetUser == null)
        {
            var shares = await _mediator.Send(new GetRecipeSharesQuery(PublicId, userId));
            return Partial("_RecipeShareSection", shares);
        }

        var permission = SharePermission == "Update" ? Models.SharePermission.Update : Models.SharePermission.Read;
        await _mediator.Send(new ShareRecipeCommand(PublicId, targetUser.Id, permission));

        var updatedShares = await _mediator.Send(new GetRecipeSharesQuery(PublicId, userId));
        return Partial("_RecipeShareSection", updatedShares);
    }

    public async Task<IActionResult> OnPostRevokeShareAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new RevokeShareCommand(RevokeShareId ?? 0, userId));
        var shares = await _mediator.Send(new GetRecipeSharesQuery(PublicId, userId));
        return Partial("_RecipeShareSection", shares);
    }
}
