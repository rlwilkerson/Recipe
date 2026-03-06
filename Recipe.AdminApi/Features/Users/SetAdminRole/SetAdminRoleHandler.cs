using MediatR;
using Microsoft.AspNetCore.Identity;
using Recipe.AdminApi.Data;

namespace Recipe.AdminApi.Features.Users.SetAdminRole;

public class SetAdminRoleHandler : IRequestHandler<SetAdminRoleCommand, SetAdminRoleResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SetAdminRoleHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<SetAdminRoleResponse> Handle(SetAdminRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new SetAdminRoleResponse(false, $"User with ID {request.UserId} not found");
        }

        // Ensure Admin role exists
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        IdentityResult result;
        if (request.AssignRole)
        {
            result = await _userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            result = await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        if (result.Succeeded)
        {
            var action = request.AssignRole ? "assigned" : "removed";
            return new SetAdminRoleResponse(true, $"Admin role {action} successfully");
        }

        return new SetAdminRoleResponse(false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
