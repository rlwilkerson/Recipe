using MediatR;
using Microsoft.AspNetCore.Identity;
using Recipe.AdminApi.Data;

namespace Recipe.AdminApi.Features.Users.SetUserAccess;

public class SetUserAccessHandler : IRequestHandler<SetUserAccessCommand, SetUserAccessResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SetUserAccessHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<SetUserAccessResponse> Handle(SetUserAccessCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new SetUserAccessResponse(false, $"User with ID {request.UserId} not found");
        }

        IdentityResult result;
        if (request.EnableAccess)
        {
            // Remove lockout
            result = await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            // Lock account indefinitely
            result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        if (result.Succeeded)
        {
            var action = request.EnableAccess ? "enabled" : "disabled";
            return new SetUserAccessResponse(true, $"User access {action} successfully");
        }

        return new SetUserAccessResponse(false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
