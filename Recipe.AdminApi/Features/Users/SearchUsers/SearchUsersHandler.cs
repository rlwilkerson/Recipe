using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Recipe.AdminApi.Data;

namespace Recipe.AdminApi.Features.Users.SearchUsers;

public class SearchUsersHandler : IRequestHandler<SearchUsersQuery, SearchUsersResponse>
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SearchUsersHandler(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<SearchUsersResponse> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(searchLower)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                (u.DisplayName != null && u.DisplayName.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var results = new List<UserSearchResult>();
        foreach (var user in users)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
            
            results.Add(new UserSearchResult(
                user.Id,
                user.UserName,
                user.Email,
                user.DisplayName,
                isLocked,
                isAdmin,
                user.LockoutEnd));
        }

        return new SearchUsersResponse(results, totalCount, request.Page, request.PageSize);
    }
}
