using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Features.Cookbooks.CreateCookbook;
using Recipe.Web.Features.Cookbooks.ListCookbooks;
using Recipe.Web.Features.Shared.GetSharedWithMe;
using System.Security.Claims;

namespace Recipe.Web.Pages.Cookbooks;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public ListCookbooksResponse? Result { get; set; }
    public SharedWithMeResponse? SharedWithMe { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        Result = await _mediator.Send(new ListCookbooksQuery(OwnerId: userId));
        SharedWithMe = await _mediator.Send(new GetSharedWithMeQuery(userId));
    }

    public IActionResult OnGetCreateCookbookModal()
    {
        return Partial("_CreateCookbookModal", null);
    }

    public async Task<IActionResult> OnPostCreateCookbookAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _mediator.Send(new CreateCookbookCommand(Name!, Description, userId));
        var listResult = await _mediator.Send(new ListCookbooksQuery(OwnerId: userId));
        return Partial("_CookbookList", listResult.Cookbooks);
    }
}
