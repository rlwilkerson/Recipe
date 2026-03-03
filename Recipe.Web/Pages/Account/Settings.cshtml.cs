using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe.Web.Models;
using System.Security.Claims;

namespace Recipe.Web.Pages.Account;

[Authorize]
public class SettingsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SettingsModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string? CurrentDisplayName { get; set; }

    [BindProperty] public string? DisplayName { get; set; }
    [BindProperty] public string? CurrentPassword { get; set; }
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();
        CurrentDisplayName = user.DisplayName;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateDisplayNameAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        user.DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? null : DisplayName.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
            TempData["SuccessMessage"] = "Display name updated.";
        else
            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        if (NewPassword != ConfirmPassword)
        {
            TempData["ErrorMessage"] = "New password and confirmation do not match.";
            return RedirectToPage();
        }

        var result = await _userManager.ChangePasswordAsync(user, CurrentPassword!, NewPassword!);
        if (result.Succeeded)
            TempData["SuccessMessage"] = "Password changed successfully.";
        else
            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToPage();
    }
}
