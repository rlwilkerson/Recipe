using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Recipe.AdminApi.Data;

namespace Recipe.AdminApi.Features.Users.GetUserDetails;

public class GetUserDetailsHandler : IRequestHandler<GetUserDetailsQuery, GetUserDetailsResponse>
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserDetailsHandler(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<GetUserDetailsResponse> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

        // Count recipes and cookbooks - simplified for now since we're admin-only
        var recipeCount = 0; // Would query Recipe.Web's schema if shared
        var cookbookCount = 0;

        return new GetUserDetailsResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.EmailConfirmed,
            isLocked,
            isAdmin,
            user.LockoutEnd,
            recipeCount,
            cookbookCount);
    }
}
