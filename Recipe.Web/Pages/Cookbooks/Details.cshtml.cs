using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Cookbooks.AddRecipeToCookbook;
using Recipe.Web.Features.Cookbooks.DeleteCookbook;
using Recipe.Web.Features.Cookbooks.EditCookbook;
using Recipe.Web.Features.Cookbooks.GetCookbook;
using Recipe.Web.Features.Cookbooks.GetCookbookShares;
using Recipe.Web.Features.Cookbooks.RemoveRecipeFromCookbook;
using Recipe.Web.Features.Cookbooks.RevokeShare;
using Recipe.Web.Features.Cookbooks.ShareCookbook;
using Recipe.Web.Features.Recipes.CreateRecipe;
using Recipe.Web.Models;
using System.Security.Claims;

namespace Recipe.Web.Pages.Cookbooks;

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

    // Share
    [BindProperty] public string? ShareEmail { get; set; }
    [BindProperty] public string? SharePermission { get; set; }
    [BindProperty] public int? RevokeShareId { get; set; }

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

    public async Task<IActionResult> OnGetShareSectionAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var shares = await _mediator.Send(new GetCookbookSharesQuery(PublicId, userId));
        return Partial("_CookbookShareSection", shares);
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

    public async Task<IActionResult> OnPostShareAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var targetUser = await _userManager.FindByEmailAsync(ShareEmail ?? string.Empty);
        if (targetUser == null)
        {
            TempData["ShareError"] = $"No user found with email '{ShareEmail}'.";
            var shares = await _mediator.Send(new GetCookbookSharesQuery(PublicId, userId));
            return Partial("_CookbookShareSection", shares);
        }

        var permission = SharePermission == "Update" ? Models.SharePermission.Update : Models.SharePermission.Read;
        await _mediator.Send(new ShareCookbookCommand(PublicId, targetUser.Id, permission));

        var updatedShares = await _mediator.Send(new GetCookbookSharesQuery(PublicId, userId));
        return Partial("_CookbookShareSection", updatedShares);
    }

    public async Task<IActionResult> OnPostRevokeShareAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new RevokeShareCommand(RevokeShareId ?? 0, userId));
        var shares = await _mediator.Send(new GetCookbookSharesQuery(PublicId, userId));
        return Partial("_CookbookShareSection", shares);
    }
}
