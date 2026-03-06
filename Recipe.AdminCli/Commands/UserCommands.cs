using Recipe.AdminCli.Auth;
using Recipe.AdminCli.Services;

namespace Recipe.AdminCli.Commands;

public class UserCommands
{
    private readonly AdminApiClient _apiClient;
    private readonly OidcAuthService _authService;

    public UserCommands(AdminApiClient apiClient, OidcAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<int> SearchUsersAsync(string? searchTerm, int page, int pageSize)
    {
        var token = await EnsureAuthenticatedAsync();
        if (token == null) return 1;

        try
        {
            var result = await _apiClient.SearchUsersAsync(searchTerm, page, pageSize);
            if (result == null)
            {
                Console.WriteLine("No results returned.");
                return 1;
            }

            Console.WriteLine($"\nFound {result.TotalCount} user(s) (Page {result.Page}/{Math.Ceiling((double)result.TotalCount / result.PageSize)}):\n");
            Console.WriteLine($"{"ID",-40} {"Username",-20} {"Email",-30} {"Admin",-8} {"Locked",-8}");
            Console.WriteLine(new string('-', 120));

            foreach (var user in result.Users)
            {
                Console.WriteLine($"{user.Id,-40} {user.UserName ?? "N/A",-20} {user.Email ?? "N/A",-30} {(user.IsAdmin ? "Yes" : "No"),-8} {(user.IsLocked ? "Yes" : "No"),-8}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    public async Task<int> GetUserDetailsAsync(string userId)
    {
        var token = await EnsureAuthenticatedAsync();
        if (token == null) return 1;

        try
        {
            var user = await _apiClient.GetUserDetailsAsync(userId);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return 1;
            }

            Console.WriteLine("\nUser Details:");
            Console.WriteLine($"  ID: {user.Id}");
            Console.WriteLine($"  Username: {user.UserName ?? "N/A"}");
            Console.WriteLine($"  Email: {user.Email ?? "N/A"} (Confirmed: {user.EmailConfirmed})");
            Console.WriteLine($"  Display Name: {user.DisplayName ?? "N/A"}");
            Console.WriteLine($"  Admin: {(user.IsAdmin ? "Yes" : "No")}");
            Console.WriteLine($"  Account Status: {(user.IsLocked ? "Locked" : "Active")}");
            if (user.LockoutEnd.HasValue)
            {
                Console.WriteLine($"  Lockout Until: {user.LockoutEnd.Value}");
            }
            Console.WriteLine($"  Recipes: {user.RecipeCount}");
            Console.WriteLine($"  Cookbooks: {user.CookbookCount}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    public async Task<int> EnableUserAsync(string userId)
    {
        var token = await EnsureAuthenticatedAsync();
        if (token == null) return 1;

        try
        {
            var result = await _apiClient.SetUserAccessAsync(userId, enableAccess: true);
            Console.WriteLine(result?.Message ?? "Operation completed.");
            return result?.Success == true ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    public async Task<int> DisableUserAsync(string userId)
    {
        var token = await EnsureAuthenticatedAsync();
        if (token == null) return 1;

        try
        {
            var result = await _apiClient.SetUserAccessAsync(userId, enableAccess: false);
            Console.WriteLine(result?.Message ?? "Operation completed.");
            return result?.Success == true ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    public async Task<int> AssignAdminRoleAsync(string userId)
    {
        var token = await EnsureAuthenticatedAsync();
        if (token == null) return 1;

        try
        {
            var result = await _apiClient.SetAdminRoleAsync(userId, assignRole: true);
            Console.WriteLine(result?.Message ?? "Operation completed.");
            return result?.Success == true ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    public async Task<int> RemoveAdminRoleAsync(string userId)
    {
        var token = await EnsureAuthenticatedAsync();
        if (token == null) return 1;

        try
        {
            var result = await _apiClient.SetAdminRoleAsync(userId, assignRole: false);
            Console.WriteLine(result?.Message ?? "Operation completed.");
            return result?.Success == true ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<string?> EnsureAuthenticatedAsync()
    {
        var token = await _authService.GetAccessTokenAsync();
        if (token == null)
        {
            Console.WriteLine("Authentication required. Please run 'login' command first.");
            return null;
        }

        _apiClient.SetAuthToken(token);
        return token;
    }
}
